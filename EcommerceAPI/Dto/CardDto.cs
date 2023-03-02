using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Dto
{
    public class CardDto
    {
        [CreditCard]
        public string CardNumber { get; set; }

        [Required]
        public string CardName { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Please enter a valid year.")]
        public string ExpYear { get; set; }

        [Range(1, 12)]
        public int ExpMonth { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Only allow 3 or 4 digits.")]
        public string Cvc { get; set; }

    }
}
