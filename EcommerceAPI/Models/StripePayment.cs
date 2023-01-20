namespace EcommerceAPI.Models
{
    public class StripePayment
    {
        public string ReceiptEmail { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public Int64 Amount { get; set; }

        public string PaymentId { get; set; } = null!;

        public string Response { get; set; } = null!;
    }
}
