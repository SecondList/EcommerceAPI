using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class ProductUpdateDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be positive value.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "A product title is required")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "A product description is required")]
        public string Description { get; set; } = null!;

        [Range(1, int.MaxValue, ErrorMessage = "Category Id must be positive value.")]
        public int CategoryId { get; set; }

        [Range(0.01, Double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }
    }
}
