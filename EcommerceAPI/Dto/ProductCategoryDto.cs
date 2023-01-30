using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public virtual ICollection<ProductDto> Products { get; set; } = null!;
    }
}
