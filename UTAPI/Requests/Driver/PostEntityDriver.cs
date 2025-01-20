using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.EntityDriver
{
    public class PostEntityDriver
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid EntityId { get; set; }  // Collection of Stop IDs
    }
}
