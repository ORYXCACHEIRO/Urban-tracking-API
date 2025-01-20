using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.EntityDriver
{
    public class ListEntityDriver
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public Guid EntityId { get; set; }  // Collection of Stop IDs

        public string? Username { get; set; }
    }
}
