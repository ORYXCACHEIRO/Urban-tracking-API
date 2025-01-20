using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UTAPI.Websocket
{
    /// <summary>
    /// Middleware responsible for handling WebSocket connections and validating JWT tokens.
    /// This middleware ensures that the WebSocket request has a valid token and processes the connection accordingly.
    /// </summary>
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly RoomManager _roomManager;
        private readonly ILogger<WebSocketHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the WebSocketMiddleware class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="tokenValidationParameters">The parameters used for token validation.</param>
        /// <param name="roomManager">The room manager used to handle WebSocket connections.</param>
        /// <param name="logger">The logger for logging events related to WebSocket handling.</param>
        public WebSocketMiddleware(RequestDelegate next, TokenValidationParameters tokenValidationParameters, RoomManager roomManager, ILogger<WebSocketHandler> logger)
        {
            _next = next;
            _tokenValidationParameters = tokenValidationParameters;
            _roomManager = roomManager;
            _logger = logger; // Initialize logger
        }

        /// <summary>
        /// Validates a JWT token using the specified token validation parameters.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>A ClaimsPrincipal representing the token's claims.</returns>
        private ClaimsPrincipal ValidateJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, _tokenValidationParameters, out _);
        }

        /// <summary>
        /// Processes the WebSocket connection and validates the JWT token.
        /// </summary>
        /// <param name="context">The HTTP context containing the WebSocket request.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is a WebSocket request
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context); // If not, pass the request to the next middleware
                return;
            }

            // Retrieve the token from the query string
            var token = context.Request.Query["access_token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized; // Unauthorized if token is missing
                return;
            }

            ClaimsPrincipal claimsPrincipal;
            try
            {
                // Validate the token and get the claims
                claimsPrincipal = ValidateJwtToken(token);
                //Console.WriteLine("Token successfully validated.");
            }
            catch (Exception ex)
            {
                // Log and respond with Unauthorized if token validation fails
                //Console.WriteLine($"Token validation failed: {ex.Message}");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // Retrieve the user's role from the token claims
            var roleClaim = claimsPrincipal.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var role = int.TryParse(roleClaim, out var parsedRole) ? parsedRole : -1;

            // Accept the WebSocket request and handle the connection
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var handler = new WebSocketHandler(_roomManager, role, _logger);
            await handler.HandleAsync(webSocket, claimsPrincipal);
        }
    }
}
