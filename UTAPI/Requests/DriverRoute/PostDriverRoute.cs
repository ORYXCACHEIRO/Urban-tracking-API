using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.DriverRoute
{
    public class PostDriverRoute
    {
        public Guid UserId { get; set; }         // Driver ID (User)
        public Guid RouteId { get; set; }        // Route ID
        public DateTime StartTime { get; set; }  // Start time of the route
        public DateTime EndTime { get; set; }    // End time of the route
        [StringLength(7, MinimumLength = 7)]
        public string WorkDays { get; set; }     // Days the driver works on this route
    }
}
