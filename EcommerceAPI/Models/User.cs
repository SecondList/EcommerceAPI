using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

namespace EcommerceAPI.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];
        public DateTime CreatedAt { get; set; }
        public ICollection<Cart> Carts { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = null!;
    }
}
