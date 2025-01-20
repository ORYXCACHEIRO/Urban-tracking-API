using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a user in the system, inheriting from IdentityUser to include identity properties.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the user. The name must be between 6 and 50 characters.
        /// </summary>
        [StringLength(50, MinimumLength = 6)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email of the user. The email must be between 15 and 100 characters.
        /// </summary>
        [StringLength(100, MinimumLength = 15)]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the password of the user. The password must be between 6 and 100 characters.
        /// </summary>
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the role of the user. The role is a single character (e.g., '1' for driver, '2' for admin).
        /// </summary>
        [StringLength(1, MinimumLength = 1)]
        public string Role { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is active or inactive.
        /// </summary>
        public bool Active { get; set; }
    }
}
