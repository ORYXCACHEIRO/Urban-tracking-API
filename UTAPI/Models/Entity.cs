using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents an entity, which could be a business, organization, or other entity with specific attributes.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email address of the entity.
        /// </summary>
        /// <remarks>
        /// The email address must be in a valid email format.
        /// </remarks>
        [StringLength(100)]
        [EmailAddress] // Email validation attribute
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the entity.
        /// </summary>
        [StringLength(9)]
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a description about the entity.
        /// </summary>
        /// <remarks>
        /// The description can provide additional details such as history, mission, etc.
        /// </remarks>
        [StringLength(500)] // Limit for the "about" field
        public string About { get; set; }

        /// <summary>
        /// Gets or sets the working hours of the entity.
        /// </summary>
        [StringLength(100)] // Limit for the "workHours" field
        public string WorkHours { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the region associated with the entity.
        /// </summary>
        public Guid RegionId { get; set; }

        /// <summary>
        /// Gets or sets the associated region for the entity.
        /// </summary>
        public virtual Region Region { get; set; }  // Relationship with the Region table
    }
}
