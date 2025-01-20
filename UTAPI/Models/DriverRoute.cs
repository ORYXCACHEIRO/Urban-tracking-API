using System;
using System.ComponentModel.DataAnnotations;

namespace UTAPI.Models
{
    /// <summary>
    /// Represents the assignment of a driver to a specific route, including work schedule details.
    /// </summary>
    public class DriverRoute
    {
        /// <summary>
        /// Gets or sets the unique identifier for the DriverRoute record.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user (driver) assigned to the route.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the route to which the driver is assigned.
        /// </summary>
        public Guid RouteId { get; set; }

        /// <summary>
        /// Gets or sets the start time of the route for the driver.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the route for the driver.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the days the driver works on this route, represented as a comma-separated string (e.g., "Mon,Wed,Fri").
        /// </summary>
        [StringLength(7, MinimumLength = 7)]
        public string WorkDays { get; set; }

        /// <summary>
        /// Navigation property to the associated User (driver).
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Navigation property to the associated Route.
        /// </summary>
        public Route Route { get; set; }
    }
}
