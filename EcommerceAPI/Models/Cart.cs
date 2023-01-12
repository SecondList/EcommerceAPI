using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set;}
        
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [Range(1, 99999, ErrorMessage = "Quantity must be a positive value and cannot order more than 99999 items")]
        [DisplayName("Order Quantity")]
        public int OrderQty { get; set; }
        public DateTime CreatedAt { get; set;}
        public DateTime ModifiedAt { get; set; }
        public User User { get; set; } = null!;
        public Product Product { get; set; } = null!;

    }
}
