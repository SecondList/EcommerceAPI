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

        // Shipping start
        // Address start
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Address1 { get; set; } = null!;
        public string Address2 { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string State { get; set; } = null!;


        // Address end

        [Required]
        public string RecipientName { get; set; }
        public string Carrier { get; set; }
        public string Phone { get; set; }
        public string TrackingNumber { get; set; }
        // Shipping end

        // Card Detail start
        [Required]
        public CardDto Card { get; set; }
        // Card Detail end
    }
}
