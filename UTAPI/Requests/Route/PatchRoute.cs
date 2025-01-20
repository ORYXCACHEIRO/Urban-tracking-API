using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.Route
{
    public class PatchRoute
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public Guid? EntityId { get; set; }

        public bool? Active { get; set; }

        public Guid? RegionId { get; set; }

    }
}
