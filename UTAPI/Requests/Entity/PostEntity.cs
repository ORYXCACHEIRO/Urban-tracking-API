namespace UTAPI.Requests.Entity
{
    public class PostEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool Active { get; set; }
        public string About { get; set; }
        public string WorkHours { get; set; }
        public Guid RegionId { get; set; }
    }
}
