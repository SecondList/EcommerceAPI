using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Dto
{
    public class PaymentDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Payment Id must be positive value.")]
        public int PaymentId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Order Id must be positive value.")]
        public int OrderId { get; set; }

        [Range(0.00, Double.MaxValue, ErrorMessage = "Amount must be positive")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "A payment method is required")]
        [StringLength(50, ErrorMessage = "Please do not enter values over 50 characters")]
        public string PaymentMethod { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Invalid payment status.")]
        public int Status { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;


        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual OrderDto Order { get; set; } = null!;
    }
}
