using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.RouteStop
{
    public class PostRouteStop
    {

        [Required]
        public Guid RouteId { get; set; }

        [Required]
        public Guid StopId { get; set; }
    }
}
