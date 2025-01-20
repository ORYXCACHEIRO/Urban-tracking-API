using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.User
{
    public class ListUser
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}
