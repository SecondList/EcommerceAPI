using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class UserRegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [MinLength(8, ErrorMessage = "The minimum length for password is 8 characters.")]
        public string Password { get; set; } = null!;
        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
