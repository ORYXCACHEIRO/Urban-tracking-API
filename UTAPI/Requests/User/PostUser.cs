using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.User
{
    public class PostUser
    {
        [Required]
        [StringLength(50, MinimumLength = 6)]
        public string Name { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 15)]
        public string Email { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }
        [Required]
        [StringLength(1, MinimumLength = 1)]
        public string Role { get; set; }
    }
}
