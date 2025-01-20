using System.Net.WebSockets;

namespace UTAPI.Websocket.Models
{
    /// <summary>
    /// Represents a WebSocket room for users and a driver.
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Gets or sets the unique identifier for the room.
        /// This is automatically generated using a GUID.
        /// </summary>
        public string RoomId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the name of the room.
        /// This can be any descriptive name for the room.
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// Gets or sets the WebSocket connection for the driver.
        /// The driver is a special participant in the room.
        /// </summary>
        public WebSocket DriverSocket { get; set; }

        /// <summary>
        /// Gets or sets the list of WebSocket connections for users in the room.
        /// A room can have multiple user participants.
        /// </summary>
        public List<WebSocket> UserSockets { get; set; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the room is active.
        /// If set to <c>false</c>, the room is considered inactive.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the last active time of the room.
        /// This tracks when the room was last used or interacted with.
        /// </summary>
        public DateTime LastActive { get; set; } = DateTime.UtcNow; // Tracks the last active time
    }
}

