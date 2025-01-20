using System.ComponentModel.DataAnnotations;

namespace UTAPI.Requests.Auth
{
    public class Login
    {
        [Required]
        [StringLength(100, MinimumLength = 15)]
        public string email { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string password { get; set; }
    }
}
