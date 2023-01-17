using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; } = null!;
        [ForeignKey("Shipment")]
        public int ShipmentId { get; set; }
        public int Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime ModifiedAt { get; set; }
        public Shipment Shipment { get; set; } = null!;
    }
}
