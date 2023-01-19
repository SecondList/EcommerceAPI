using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class UserDetailDto
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
