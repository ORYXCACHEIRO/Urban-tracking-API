using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a stop location with details such as name, coordinates, and status.
    /// </summary>
    public class Stop
    {
        /// <summary>
        /// Gets or sets the unique identifier for the stop.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the stop. The name must be between 3 and 50 characters.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the stop. Latitude should be between -90 and 90 degrees.
        /// </summary>
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the stop. Longitude should be between -180 and 180 degrees.
        /// </summary>
        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the stop is active or inactive.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the stop. The default value is set to the current UTC time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
