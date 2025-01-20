using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.RouteHistory
{
    public class PostRouteHistory
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public Guid RouteId { get; set; }
    }
}
