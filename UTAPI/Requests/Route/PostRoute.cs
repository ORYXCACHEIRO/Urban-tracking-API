using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.Route
{
    public class PostRoute
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public Guid RegionId { get; set; }

  
    }
}
