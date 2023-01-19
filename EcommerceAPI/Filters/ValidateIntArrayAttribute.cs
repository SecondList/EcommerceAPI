using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.Filters
{
    public class ValidateIntArrayAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object ints, ValidationContext validationContext)
        {
            int[]? arrInts = ints as int[];
            if (arrInts == null || arrInts.Length == 0)
            {
                return new ValidationResult("At least one element is required");
            }

            // Validation for negative numbers
            bool allPositive = arrInts.All(s => s > 0);
            if (!allPositive)
            {
                return new ValidationResult("Enter only positive values");
            }

            // Validation for Min and Max value of the SubType array
            var min = arrInts.Min();
            var max = arrInts.Max();
            if (min < 1 || max > int.MaxValue)
            {
                return new ValidationResult("Enter only valid integer");
            }

            return ValidationResult.Success;
        }
    }
}
