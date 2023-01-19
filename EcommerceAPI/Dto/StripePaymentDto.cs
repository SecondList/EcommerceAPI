using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class StripePaymentDto
    {
        public string ReceiptEmail { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = "MYR";

        [Range(200, 99999999)]
        public Int64 Amount { get; set; }

    }
}
