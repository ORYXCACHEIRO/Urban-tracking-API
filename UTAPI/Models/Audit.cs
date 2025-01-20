using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents an audit log entry for tracking changes or actions taken by users.
    /// </summary>
    public class Audit
    {
        /// <summary>
        /// Gets or sets the unique identifier of the audit entry.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who performed the action.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the resource being acted upon.
        /// </summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the action performed by the user (e.g., "Create", "Update", "Delete").
        /// </summary>
        [StringLength(8)]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource that was acted upon.
        /// </summary>
        [StringLength(20)]
        public string Resource { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the action was logged.
        /// </summary>
        public DateTime LogDate { get; set; }
    }
}
