using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class OrderDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Order Id must be positive value.")]
        public int OrderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int UserId { get; set; }

        [Range(0.00, Double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal TotalPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Invalid order status.")]
        public int OrderStatus { get; set; }

        public virtual ICollection<OrderDetailDto> OrderDetails { get; set; } = null!;
        public virtual UserDto User { get; set; } = null!;
        public virtual PaymentDto Payment { get; set; } = null!;
        public virtual ShipmentDto Shipment { get; set; } = null!;
    }
}