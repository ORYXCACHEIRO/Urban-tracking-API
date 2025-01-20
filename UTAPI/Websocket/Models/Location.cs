namespace UTAPI.Websocket.Models
{
    /// <summary>
    /// Represents a geographical location with latitude and longitude.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Gets or sets the latitude of the location.
        /// Latitude is in decimal degrees. It can be null if not provided.
        /// </summary>
        public double? Lat { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the location.
        /// Longitude is in decimal degrees. It can be null if not provided.
        /// </summary>
        public double? Lng { get; set; }
    }
}

