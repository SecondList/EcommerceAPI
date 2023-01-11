using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        
        [Required]
        [Range(1, 99999, ErrorMessage = "Quantity must be a positive value and cannot order more than 99999 items")]
        [DisplayName("Order Quantity")]
        public int OrderQty { get; set; }
        public Product Product { get; set; } = null!;
    }
}
