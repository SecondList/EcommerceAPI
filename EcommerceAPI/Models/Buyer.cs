using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class Buyer
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string Password { get; set; }
        public ICollection<Cart> Carts { get; set; } = null!;
    }
}
