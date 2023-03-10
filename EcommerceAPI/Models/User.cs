using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace EcommerceAPI.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = new byte[32];
        public byte[] PasswordSalt { get; set; } = new byte[32];

        [ForeignKey("UserRole")]
        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int RoleId { get; set; } = 2;
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<Cart> Carts { get; set; } = null!;
        public virtual ICollection<Order> Orders { get; set; } = null!;
        public virtual UserRole UserRole { get; set; } = null!;
    }
}
