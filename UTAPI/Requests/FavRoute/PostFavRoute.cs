using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.FavRoute
{
    public class PostFavRoute
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid RouteId { get; set; }  // Collection of Stop IDs
    }
}
