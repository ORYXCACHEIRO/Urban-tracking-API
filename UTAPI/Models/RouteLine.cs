using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a line in a route, with coordinates and direction information.
    /// </summary>
    public class RouteLine
    {
        /// <summary>
        /// Gets or sets the unique identifier for the route line.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the route associated with the route line.
        /// </summary>
        [Required]
        public Guid RouteId { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the first point in the route line.
        /// </summary>
        [Required]
        public double FirstLat { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the first point in the route line.
        /// </summary>
        [Required]
        public double FirstLong { get; set; }

        /// <summary>
        /// Gets or sets the direction of the route line. It represents the direction in which the route line is going.
        /// </summary>
        [Required]
        public int Direction { get; set; }  // Changed to int

        /// <summary>
        /// Gets or sets the color of the route line in hexadecimal format (e.g., #FFFFFF for white).
        /// </summary>
        [Required]
        [StringLength(7, MinimumLength = 7)] // Hexadecimal value
        public string LineColor { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the second point in the route line.
        /// </summary>
        [Required]
        public double SecondLat { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the second point in the route line.
        /// </summary>
        [Required]
        public double SecondLong { get; set; }
    }
}
