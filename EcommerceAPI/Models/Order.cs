using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class Order
    {
        [Key]
        [Range(1, int.MaxValue, ErrorMessage = "Order Id must be positive value.")]
        public int OrderId { get; set; }

        [ForeignKey("User")]
        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int UserId { get; set; }

        [Range(0.00, Double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal TotalPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Invalid order status.")]
        public int OrderStatus { get; set; } = 0;

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Payment Payment { get; set; } = null!;
        public virtual Shipment Shipment { get; set; } = null!;
    }
}
