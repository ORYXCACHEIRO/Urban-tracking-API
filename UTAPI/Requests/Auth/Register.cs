using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.Auth
{
    public class Register
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
    }
}
