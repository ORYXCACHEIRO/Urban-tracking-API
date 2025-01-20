using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents a user's favorite route.
    /// </summary>
    public class FavRoute
    {
        /// <summary>
        /// Gets or sets the unique identifier for the FavRoute record.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the user who favorited the route.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the route that was favorited by the user.
        /// </summary>
        [Required]
        public Guid RouteId { get; set; }

        /// <summary>
        /// Navigation property to the associated User who favorited the route.
        /// </summary>
        public virtual User User { get; set; }

        /// <summary>
        /// Navigation property to the associated Route that was favorited.
        /// </summary>
        public virtual Route Route { get; set; }
    }
}
