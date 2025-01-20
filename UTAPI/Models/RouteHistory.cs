using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a record of a user's interaction with a route, stored in the route history.
    /// </summary>
    public class RouteHistory
    {
        /// <summary>
        /// Gets or sets the unique identifier for the route history record.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the unique identifier of the user associated with the route history record.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the route associated with the route history record.
        /// </summary>
        [Required]
        public Guid RouteId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the route history record was created.
        /// The value is set to the current UTC time by default.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
