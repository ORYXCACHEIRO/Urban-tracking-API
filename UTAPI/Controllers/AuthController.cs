using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UTAPI.Interfaces;
using UTAPI.Requests.Auth;
using UTAPI.Requests.User;
using UTAPI.Security;

namespace UTAPI.Controllers
{
    // Route for API version 1 of authentication-related endpoints
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // Declare dependencies for services related to authentication, session management, and token generation
        private readonly IAuthServices _authServices;
        private readonly ISessionServices _sessionServices;
        private readonly TokenGenerator _tokenGenerator;

        // Constructor to inject the required services
        public AuthController(IAuthServices authServices, TokenGenerator tokenGenerator, ISessionServices sessionServices)
        {
            _authServices = authServices;
            _tokenGenerator = tokenGenerator;
            _sessionServices = sessionServices;
        }

        /// <summary>
        /// Logs the user in by validating their credentials and generating a token.
        /// </summary>
        /// <param name="request">Login request containing user credentials.</param>
        /// <returns>HTTP response with a token or an error message.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(Login request)
        {
            // Validate the login request data
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Check if the user credentials are valid
                var user = await _authServices.CheckUserAsync(request);

                if (user == null) return BadRequest();

                // Generate a JWT token for the authenticated user
                var token = _tokenGenerator.GenerateToken(user.Id, user.Email, user.Role);

                // Create a session for the user
                await _sessionServices.CreateSessionAsync(token, user.Id);

                // Return the generated token to the client
                return Ok(new { token = token });
            }
            catch (Exception ex)
            {
                // Return a 404 status code with the exception message if an error occurs
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Registers a new user by validating and saving their details.
        /// </summary>
        /// <param name="request">Registration request containing new user details.</param>
        /// <returns>HTTP response indicating success or failure.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(Register request)
        {
            // Validate the registration request data
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // Register the new user
                await _authServices.RegisterAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                // Return a 404 status code with the exception message if an error occurs
                return StatusCode(404, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's details.
        /// </summary>
        /// <returns>HTTP response with the user's information or an error message.</returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<OneUser>> InfoMeAsync()
        {
            try
            {
                // Retrieve the 'sub' claim which is used as the unique user identifier
                var subClaim = User.FindFirstValue("sub");

                // Check if the 'sub' claim is a valid Guid
                if (Guid.TryParse(subClaim, out var userId))
                {
                    // Fetch user information based on the userId
                    var user = await _authServices.GetMeInfoAsync(userId);

                    if (user == null) return BadRequest();

                    // Return the user details if found
                    return Ok(user);
                }
                else
                {
                    // If the 'sub' claim is not a valid Guid, return a BadRequest response
                    return BadRequest("Invalid user ID format.");
                }
            }
            catch (Exception ex)
            {
                // Return a 404 status code with the exception message if an error occurs
                return StatusCode(404, new { message = ex.Message });
            }
        }
    }
}
