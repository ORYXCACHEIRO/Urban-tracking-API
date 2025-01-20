namespace UTAPI.Requests.User
{
    public class OneUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}
