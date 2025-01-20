using System.Net.WebSockets;
using System.Collections.Concurrent;
using UTAPI.Websocket.Models;
using System.Text.Json;
using System.Text;

namespace UTAPI.Websocket
{
    /// <summary>
    /// Manages the WebSocket rooms, including room creation, user joining, broadcasting messages,
    /// and handling user disconnections.
    /// </summary>
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new();
        private readonly ILogger<RoomManager> _logger;
        private readonly TimeSpan RoomTimeout = TimeSpan.FromMinutes(2); // Set timeout for inactive rooms

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomManager"/> class.
        /// </summary>
        /// <param name="logger">The logger used for logging activities in this class.</param>
        public RoomManager(ILogger<RoomManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a new room with a driver or reassigns an existing room to a new driver.
        /// </summary>
        /// <param name="roomName">The name of the room to create or assign.</param>
        /// <param name="driverSocket">The WebSocket connection of the driver.</param>
        /// <returns>The created or updated room, or null if the room creation failed.</returns>
        public Room CreateRoom(string roomName, WebSocket driverSocket)
        {
            var existingRoom = _rooms.Values.FirstOrDefault(r => r.RoomName == roomName);

            if (existingRoom != null && existingRoom.IsActive)
            {
                _logger.LogWarning("Another driver tried to create a room with the same name {RoomName}, but it is already active.", roomName);
                NotifyUser(driverSocket, "room_error", $"Room {roomName} is already active with a driver.");
                return null;
            }
            else if(existingRoom != null && !existingRoom.IsActive)
            {
                _logger.LogInformation("Room {RoomId} is active but has no driver. Assigning new driver.", existingRoom.RoomId);
                existingRoom.DriverSocket = driverSocket;
                existingRoom.IsActive = true;
                existingRoom.LastActive = DateTime.UtcNow;
                NotifyUsersInRoom(existingRoom.RoomId, "driver_reconnected", "The room has been reassigned to a new driver.");
                NotifyUser(driverSocket, "room_assigned", $"You have taken over room {roomName}.");
                return existingRoom;
            }

            var room = new Room
            {
                RoomName = roomName,
                DriverSocket = driverSocket,
                IsActive = true
            };

            _rooms.TryAdd(room.RoomId, room);
            _logger.LogInformation("Room created: {RoomId}, Name: {RoomName}", room.RoomId, roomName);
            NotifyUser(driverSocket, "room_created", $"Room created: {room.RoomId}");
            return room;
        }

        /// <summary>
        /// Allows a user to join an existing room.
        /// </summary>
        /// <param name="roomName">The name of the room to join.</param>
        /// <param name="userSocket">The WebSocket connection of the user.</param>
        /// <param name="role">The role of the user (e.g., driver or passenger).</param>
        /// <returns>True if the user successfully joined, otherwise false.</returns>
        public bool JoinRoom(string roomName, WebSocket userSocket, int role)
        {
            var room = _rooms.Values.FirstOrDefault(r => r.RoomName == roomName);

            if (room == null)
            {
                _logger.LogWarning("Room with name {RoomName} not found. Unable to join.", roomName);
                return false;
            }

            if (room.UserSockets.Contains(userSocket) || room.DriverSocket == userSocket)
            {
                _logger.LogInformation("User is already in room {RoomName}, skipping further logic.", roomName);
                return true;
            }

            // Handle user being in another room
            foreach (var roomChecked in _rooms.Values)
            {
                if (roomChecked.UserSockets.Contains(userSocket) || roomChecked.DriverSocket == userSocket)
                {
                    // Handle leaving the old room
                    if (roomChecked.DriverSocket == userSocket)
                    {
                        roomChecked.IsActive = false;
                        roomChecked.DriverSocket = null;
                        roomChecked.LastActive = DateTime.UtcNow;
                        NotifyUser(userSocket, "room_left", $"Room left: {roomChecked.RoomName}");
                        _logger.LogInformation("Driver socket removed from room {RoomName}.", roomChecked.RoomName);
                    }
                    else
                    {
                        roomChecked.UserSockets.Remove(userSocket);
                    }

                    // Clean up empty rooms
                    if (roomChecked.UserSockets.Count == 0 && roomChecked.DriverSocket == null)
                    {
                        _rooms.TryRemove(roomChecked.RoomId, out _);
                        _logger.LogInformation("Room {RoomName} removed due to no users or driver.", roomChecked.RoomName);
                    }
                    else if (roomChecked.UserSockets.Count > 0 && roomChecked.DriverSocket == null)
                    {
                        NotifyUsersInRoom(roomChecked.RoomId, "driver_left", "The driver has disconnected. The room is now inactive.");
                    }

                    _logger.LogInformation("User socket removed from room {RoomName}.", roomChecked.RoomName);
                    break;
                }
            }

            if (role == 1 && !room.IsActive)
            {
                room.IsActive = true;
                room.DriverSocket = userSocket;
                room.LastActive = DateTime.UtcNow;
                NotifyUsersInRoom(room.RoomId, "driver_reconnected", "The driver has reconnected. The room is active again.");
                _logger.LogInformation("Driver rejoined room {RoomName}, room reactivated.", roomName);
                return true;
            }

            if (!room.IsActive)
            {
                _logger.LogWarning("User tried to join inactive room {RoomName}, but no driver is present.", roomName);
                return false;
            }

            if (!room.UserSockets.Contains(userSocket))
            {
                room.UserSockets.Add(userSocket);
                NotifyUser(userSocket, "room_joined", $"Room joined: {room.RoomName}");
                _logger.LogInformation("User socket joined room {RoomName}.", roomName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Broadcasts location updates to all users in the specified room.
        /// </summary>
        /// <param name="roomName">The name of the room to broadcast the location to.</param>
        /// <param name="location">The location data to broadcast.</param>
        /// <param name="senderSocket">The WebSocket connection of the sender (driver).</param>
        /// <returns>A task representing the async operation.</returns>
        public async Task BroadcastLocationToRoomAsync(string roomName, Location location, WebSocket senderSocket)
        {
            try
            {
                var room = _rooms.Values.FirstOrDefault(r => r.RoomName == roomName);

                if (room == null)
                {
                    _logger.LogWarning("Room with name {RoomName} not found for broadcasting location.", roomName);
                    return;
                }

                if (room.DriverSocket != senderSocket)
                {
                    _logger.LogWarning("Only the driver can broadcast in room: {RoomName}. Sender is not the driver.", roomName);
                    return;
                }

                var messageWrapper = new WebSocketMessage
                {
                    Event = "location_update",
                    Message = JsonSerializer.Serialize(location)
                };

                var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageWrapper));

                foreach (var socket in room.UserSockets)
                {
                    if (socket != senderSocket && socket.State == WebSocketState.Open)
                    {
                        try
                        {
                            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send location update to user socket in room: {RoomName}", roomName);
                        }
                    }
                }

                room.LastActive = DateTime.UtcNow;
                _logger.LogInformation("Location broadcasted to room: {RoomName}, Location: {Location}", roomName, location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while broadcasting location to room: {RoomName}", roomName);
            }
        }

        /// <summary>
        /// Handles the process of a user or driver leaving a room.
        /// </summary>
        /// <param name="roomName">The name of the room to leave.</param>
        /// <param name="socket">The WebSocket connection of the user or driver leaving.</param>
        /// <returns>True if successfully left the room, otherwise false.</returns>
        public bool LeaveRoom(string roomName, WebSocket socket)
        {
            var room = _rooms.Values.FirstOrDefault(r => r.RoomName == roomName);

            if (room != null)
            {
                if (room.DriverSocket == socket)
                {
                    room.IsActive = false;
                    room.DriverSocket = null;
                    _logger.LogInformation("Driver left room: {RoomName}. Marking room as inactive.", roomName);
                    NotifyUser(socket, "room_left", $"Room left: {room.RoomName}");

                    if (room.UserSockets.Count == 0 && room.DriverSocket == null)
                    {
                        _rooms.TryRemove(room.RoomId, out _);
                        _logger.LogInformation("Room {RoomName} removed as there are no users or driver left.", roomName);
                    }
                    else
                    {
                        NotifyUsersInRoom(room.RoomId, "driver_left", "The driver has disconnected.");
                    }
                }
                else if (room.UserSockets.Contains(socket))
                {
                    room.UserSockets.Remove(socket);
                    _logger.LogInformation("User left room: {RoomName}.", roomName);
                    NotifyUser(socket, "room_left", $"Room left: {room.RoomName}");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a connection (either driver or user) from a room. 
        /// If the driver disconnects, the room becomes inactive, and users are notified. 
        /// If a room becomes empty (no users or driver), the room is removed from the list.
        /// </summary>
        /// <param name="socket">The WebSocket connection to be removed.</param>
        public void RemoveConnection(WebSocket socket)
        {
            // Iterate over all rooms in the dictionary
            foreach (var room in _rooms.Values.ToList())
            {
                // If the socket is the driver's socket
                if (room.DriverSocket == socket)
                {
                    // Deactivate the room and remove the driver
                    room.IsActive = false;
                    room.DriverSocket = null;

                    _logger.LogInformation("Driver disconnected from room: {RoomId}.", room.RoomId);

                    // Notify users in the room about the driver disconnection, if any users are present
                    if (room.UserSockets.Count > 0)
                    {
                        NotifyUsersInRoom(room.RoomId, "driver_disconnected", "The driver has disconnected. The room is now inactive.");
                    }

                    // Check if the room has no users or driver left and remove it
                    if (room.UserSockets.Count == 0 && room.DriverSocket == null)
                    {
                        _rooms.TryRemove(room.RoomId, out _);
                        _logger.LogInformation("Room {RoomId} removed as there are no users or driver left.", room.RoomId);
                    }

                    return; // Exit once the driver's socket is processed
                }

                // If the socket is a user's socket
                else if (room.UserSockets.Remove(socket))
                {
                    _logger.LogInformation("User disconnected from room: {RoomId}.", room.RoomId);

                    // Check if the room is now empty and has no driver
                    if (room.UserSockets.Count == 0 && room.DriverSocket == null)
                    {
                        _rooms.TryRemove(room.RoomId, out _);
                        _logger.LogInformation("Room {RoomId} removed as there are no users or driver left.", room.RoomId);
                    }

                    return; // Exit once the user's socket is processed
                }
            }
        }


        /// <summary>
        /// Notifies a single user via their WebSocket connection.
        /// </summary>
        /// <param name="userSocket">The WebSocket connection of the user to notify.</param>
        /// <param name="eventType">The type of event to notify.</param>
        /// <param name="message">The message to send.</param>
        private void NotifyUser(WebSocket userSocket, string eventType, string message)
        {
            var notification = new WebSocketMessage
            {
                Event = eventType,
                Message = message
            };

            var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(notification));
            if (userSocket.State == WebSocketState.Open)
            {
                userSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// Notifies all users within a specific room.
        /// </summary>
        /// <param name="roomId">The ID of the room to notify users in.</param>
        /// <param name="eventType">The type of event to notify.</param>
        /// <param name="message">The message to send.</param>
        private void NotifyUsersInRoom(string roomId, string eventType, string message)
        {
            var room = _rooms.Values.FirstOrDefault(r => r.RoomId == roomId);

            if (room != null)
            {
                foreach (var socket in room.UserSockets)
                {
                    NotifyUser(socket, eventType, message);
                }
            }
        }

        /// <summary>
        /// Starts a background task that regularly checks for inactive rooms and removes them
        /// if they have been inactive beyond the specified timeout period.
        /// </summary>
        public void StartInactiveRoomCleanup()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var now = DateTime.UtcNow;

                        foreach (var room in _rooms.Values.ToList()) // Take a snapshot of the rooms
                        {
                            // Check if the room is inactive and if the timeout has passed
                            if (!room.IsActive && (now - room.LastActive) > RoomTimeout)
                            {
                                // Notify users before removing the room
                                if (room.UserSockets.Count > 0)
                                {
                                    NotifyUsersInRoom(room.RoomId, "room_closed", "The room has been permanently closed due to inactivity.");
                                }

                                // Remove the room after notification
                                if (_rooms.TryRemove(room.RoomId, out _))
                                {
                                    _logger.LogInformation("Inactive room {RoomId} removed due to timeout.", room.RoomId);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during inactive room cleanup.");
                    }

                    // Run the cleanup task every minute
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            });
        }

    }
}
