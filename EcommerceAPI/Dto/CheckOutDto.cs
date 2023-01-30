using EcommerceAPI.Filters;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class CheckOutDto
    {
        [ValidateIntArray]
        public int[] ProductIds { get; set; } = Array.Empty<int>();

        // Shipment details
        [Required(ErrorMessage = "A first name is required")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "A last name is required")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "A address is required")]
        [StringLength(100, ErrorMessage = "Please do not enter values over 100 characters")]
        public string Address1 { get; set; } = null!;

        [Required(ErrorMessage = "A city is required")]
        [StringLength(50, ErrorMessage = "Please do not enter values over 50 characters")]
        public string City { get; set; } = null!;

        [Required(ErrorMessage = "A state is required")]
        [StringLength(50, ErrorMessage = "Please do not enter values over 50 characters")]
        public string State { get; set; } = null!;

        [Required(ErrorMessage = "A postal code is required")]
        [StringLength(16, ErrorMessage = "Please do not enter values over 16 characters")]
        public string PostalCode { get; set; } = null!;

        [Required(ErrorMessage = "A country is required")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string Country { get; set; } = null!;

        [EmailAddress]
        public string ReceiptEmail { get; set; } = null!;

        [Required(ErrorMessage = "A carrier is required")]
        [StringLength(50, ErrorMessage = "Please do not enter values over 50 characters")]
        public string Carrier { get; set; } = null!;

        // Payment details
        public string Currency { get; set; } = "MYR";

        // Card Detail start
        [Required]
        public CardDto Card { get; set; }
    }
}
