using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a relationship between a user (driver) and an entity.
    /// </summary>
    public class EntityDriver
    {
        /// <summary>
        /// Gets or sets the unique identifier for the EntityDriver record.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user (driver) associated with the entity.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the entity associated with the driver.
        /// </summary>
        [Required]
        public Guid EntityId { get; set; }

        /// <summary>
        /// Navigation property to the associated User (driver).
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Navigation property to the associated Entity (company or organization).
        /// </summary>
        public virtual Entity Entity { get; set; }
    }
}
