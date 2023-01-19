using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class UserDto
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public ICollection<CartDto> Carts { get; set; } = null!;
        public ICollection<OrderDto> Orders { get; set; } = null!;
    }
}
