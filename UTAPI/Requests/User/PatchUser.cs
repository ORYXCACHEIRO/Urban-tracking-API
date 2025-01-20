using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.User
{
    public class PatchUser
    {
        [StringLength(50, MinimumLength = 6)]
        public string Name { get; set; }
        [StringLength(100, MinimumLength = 15)]
        public string Email { get; set; }
        [StringLength(30, MinimumLength = 6)]
        public string? Password { get; set; }
        [StringLength(1, MinimumLength = 1)]
        public string Role { get; set; }
        public bool Active { get; set; }
    }
}
