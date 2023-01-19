using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace EcommerceAPI.Dto
{
    public class OrderDetailDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Order Id must be positive value.")]
        public int OrderId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be positive value.")]
        public int ProductId { get; set; }

        [Range(1, 99999, ErrorMessage = "Quantity must be a positive value and cannot order more than 99999 items")]
        [DisplayName("Order Quantity")]
        public int OrderQty { get; set; }

        [Range(0.00, Double.MaxValue, ErrorMessage = "Price cannot be negative")]
        public decimal Price { get; set; }

        public virtual ProductDto Product { get; set; } = null!;
    }
}
