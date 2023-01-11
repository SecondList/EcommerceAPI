using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class ProductCategory
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "A product category is required")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string CategoryName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}