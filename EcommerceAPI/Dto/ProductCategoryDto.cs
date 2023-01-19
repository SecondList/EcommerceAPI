using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class ProductCategoryDto
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "A product category name is required.")]
        [StringLength(160, ErrorMessage = "Please do not enter values over 160 characters.")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "The active status of this product category needed to be defined.")]
        public bool ActiveStatus { get; set; } = true;
        public virtual ICollection<ProductDto> Products { get; set; } = null!;
    }
}
