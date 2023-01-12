using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Range(0.00, Double.MaxValue)]
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        [Required]
        public int OrderStatus { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = null!;
        public User Buyer { get; set; } = null!;
    }
}
