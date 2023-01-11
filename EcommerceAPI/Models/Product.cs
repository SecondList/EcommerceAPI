using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceAPI.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "A product title is required")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "A product description is required")]
        public string Description { get; set; } = null!;

        public int CategoryId { get; set; }

        [Column("Price", TypeName = "decimal(7, 2)")]
        [Required(ErrorMessage = "A product price is required")]
        [Range(0.01, Double.MaxValue, ErrorMessage = "Price must be positive")]
        public decimal Price { get; set; }

        [DisplayName("Product Image URL")]
        [StringLength(1024, ErrorMessage = "Please do not enter values over 1024 characters")]
        public string ImageUrl { get; set; } = null!;
        public ProductCategory ProductCategory { get; set; }

    }
}
