using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UTAPI.Websocket.Models;

namespace UTAPI.Websocket
{
    /// <summary>
    /// Handles WebSocket connections, processes incoming messages, and manages room interactions.
    /// </summary>
    public class WebSocketHandler
    {
        private readonly RoomManager _roomManager;
        private readonly int _role;
        private readonly ILogger<WebSocketHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the WebSocketHandler class.
        /// </summary>
        /// <param name="roomManager">The manager for handling rooms.</param>
        /// <param name="role">The role of the user (e.g., driver, user).</param>
        /// <param name="logger">The logger for logging events related to WebSocket handling.</param>
        public WebSocketHandler(RoomManager roomManager, int role, ILogger<WebSocketHandler> logger)
        {
            _roomManager = roomManager;
            _role = role;
            _logger = logger;
        }

        /// <summary>
        /// Handles an incoming WebSocket connection, listens for messages, and processes them.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection to handle.</param>
        /// <param name="claimsPrincipal">The claims associated with the connected user (including their role and user ID).</param>
        public async Task HandleAsync(WebSocket webSocket, ClaimsPrincipal claimsPrincipal)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // Listen for incoming messages as long as the WebSocket is open
                while (webSocket.State == WebSocketState.Open)
                {
                    var buffer = new byte[1024 * 4]; // Buffer to store received data
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    // If the message is text, process it
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessMessageAsync(message, webSocket, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing WebSocket message.");
            }
            finally
            {
                // Cleanup WebSocket connection when done
                _roomManager.RemoveConnection(webSocket);
            }
        }

        /// <summary>
        /// Processes a received message, interpreting its action and executing the corresponding logic.
        /// </summary>
        /// <param name="message">The incoming message from the WebSocket.</param>
        /// <param name="webSocket">The WebSocket connection associated with the message.</param>
        /// <param name="userId">The ID of the user who sent the message.</param>
        private async Task ProcessMessageAsync(string message, WebSocket webSocket, string userId)
        {
            var parsedMessage = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message);
            if (parsedMessage == null || !parsedMessage.TryGetValue("action", out var action))
                return;

            // Handle different types of actions
            switch (action.ToString().ToLower())
            {
                case "create":
                    if (_role != 1) // Only drivers can create rooms
                    {
                        // Log and deny room creation if user is not a driver
                        _logger.LogWarning("User with role {Role} tried to create a room but does not have permission.", _role);
                        return;
                    }
                    if (parsedMessage.TryGetValue("room", out var roomName))
                    {
                        try
                        {
                            _roomManager.CreateRoom(roomName.ToString(), webSocket);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error creating room.");
                        }
                    }
                    break;

                case "join":
                    // Handle user joining a room
                    if (parsedMessage.TryGetValue("roomId", out var roomId))
                    {
                        _roomManager.JoinRoom(roomId.ToString(), webSocket, _role);
                    }
                    break;

                case "leave":
                    // Handle user leaving a room
                    if (parsedMessage.TryGetValue("roomId", out var roomIdToLeave))
                    {
                        _roomManager.LeaveRoom(roomIdToLeave.ToString(), webSocket);
                    }
                    break;

                case "broadcastlocation":
                    if (_role != 1) return; // Only drivers can broadcast location

                    try
                    {
                        // Ensure required fields are present and valid
                        if (parsedMessage.TryGetValue("roomId", out var broadcastRoomId) && parsedMessage.TryGetValue("location", out var locationObj))
                        {
                            var location = JsonSerializer.Deserialize<Location>(locationObj.ToString());
                            if (location != null && location.Lat is >= -90 and <= 90 && location.Lng is >= -180 and <= 180)
                            {
                                // Broadcast location to room
                                await _roomManager.BroadcastLocationToRoomAsync(broadcastRoomId.ToString(), location, webSocket);
                                _logger.LogInformation("Location broadcasted to room {RoomId}: {Location} at {Date}", broadcastRoomId, location, DateTime.Now);
                            }
                            else
                            {
                                _logger.LogError("Invalid location data.");
                            }
                        }
                        else
                        {
                            _logger.LogError("Required data (roomId or location) is missing.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while processing broadcastLocation.");
                    }
                    break;
            }
        }
    }
}
