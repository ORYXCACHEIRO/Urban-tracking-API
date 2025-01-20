using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a session for a user, including the token and session details.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Gets or sets the unique identifier for the session.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user associated with the session.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the token associated with the session.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the session was created or started.
        /// The session date should be between 15 and 100 characters.
        /// </summary>
        [StringLength(100, MinimumLength = 15)]
        public DateTime SessionDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the session is active.
        /// </summary>
        public bool Active { get; set; }
    }
}
