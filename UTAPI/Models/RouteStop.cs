using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a stop associated with a specific route.
    /// </summary>
    public class RouteStop
    {
        /// <summary>
        /// Gets or sets the unique identifier for the route stop.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the route associated with the stop.
        /// </summary>
        [Required]
        public Guid RouteId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the stop.
        /// </summary>
        [Required]
        public Guid StopId { get; set; }
        public virtual Models.Route Route { get; set; }
        public virtual Models.Stop Stop { get; set; }
    }
}
