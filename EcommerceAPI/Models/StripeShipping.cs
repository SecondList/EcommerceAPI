using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class StripeShipping
    {
        [Required]
        public string Address { get; set; } = null!;
        [Required]
        public string RecipientName{ get; set; }
        public string Carrier { get; set; }
        public string Phone { get; set; }
        public string TrackingNumber { get; set; }
    }
}
