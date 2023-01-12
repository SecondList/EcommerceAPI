using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class UserLogin
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
    }
}
