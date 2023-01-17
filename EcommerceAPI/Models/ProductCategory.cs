using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Models
{
    public class ProductCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(160)]
        public string CategoryName { get; set; } = null!;
        [Required]
        public bool ActiveStatus { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}