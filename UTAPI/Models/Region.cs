using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a region entity.
    /// </summary>
    public class Region
    {
        /// <summary>
        /// Gets or sets the unique identifier for the region.
        /// </summary>
        [Key]
        public Guid Id { get; set; }  // Identificador único da região

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        [StringLength(40)]
        public string Name { get; set; }  // Nome da região
    }
}
