using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class Shipment 
    {
        [Key]
        public int ShipmentId { get; set; }

        [Required]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string LastName { get; set; } = null!;

    }
}