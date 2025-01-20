using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a route entity.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Gets or sets the unique identifier for the route.
        /// </summary>
        [Key]
        public Guid Id { get; set; } // Identificador único da entidade

        /// <summary>
        /// Gets or sets the name of the route. The name must be between 3 and 50 characters long.
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the entity associated with the route.
        /// </summary>
        [Required]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets whether the route is active or not.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the region associated with the route.
        /// </summary>
        [Required]
        public Guid RegionId { get; set; }

    }
}
