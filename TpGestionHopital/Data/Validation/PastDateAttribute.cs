using System.ComponentModel.DataAnnotations;

namespace TpGestionHopital.Data.Validation;

public class PastDateAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateValue)
        {
            if (dateValue >= DateTime.Now)
            {
                return new ValidationResult("The date must be in the past.");
            }
        }
        return ValidationResult.Success;
    }
}
