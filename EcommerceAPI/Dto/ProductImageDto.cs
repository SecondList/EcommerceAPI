using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Dto
{
    public class ProductImageDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Product Id must be positive value.")]
        public int ProductId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public IFormFile Image { get; set; }

        public string? ImagePath { get; set; }
    }
}
