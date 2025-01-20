
namespace UTAPI.Requests.Route
{
    public class ListRoute
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid EntityId { get; set; }
        public bool Active { get; set; }
        public Guid RegionId { get; set; }

    }
}
