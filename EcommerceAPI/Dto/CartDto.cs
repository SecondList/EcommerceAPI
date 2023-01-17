using EcommerceAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Dto
{
    public class CartDto
    {
        [Key]
        public int CartId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be positive value.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "User Id must be positive value.")]
        public int UserId { get; set; }

        [Range(1, 99999, ErrorMessage = "Quantity must be a positive value and cannot order more than 99999 items")]
        [DisplayName("Order Quantity")]
        public int OrderQty { get; set; }
    }
}
