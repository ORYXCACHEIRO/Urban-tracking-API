namespace UTAPI.Websocket.Models
{
    /// <summary>
    /// Represents a message sent over a WebSocket connection.
    /// </summary>
    public class WebSocketMessage
    {
        /// <summary>
        /// Gets or sets the event type associated with the message.
        /// This is used to identify the type of message being sent (e.g., "join", "leave", etc.).
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// Gets or sets the content of the message.
        /// This contains the data or information related to the event.
        /// </summary>
        public string Message { get; set; }
    }
}
