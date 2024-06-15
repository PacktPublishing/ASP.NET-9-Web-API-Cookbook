using System.ComponentModel.DataAnnotations;

namespace events.Models;

public class AllowedValuesAttribute : ValidationAttribute
{
    private readonly List<string> _allowedValues;

    public AllowedValuesAttribute(params string[] allowedValues)
    {
        _allowedValues = allowedValues?.ToList() ?? new List<string>();
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null || !_allowedValues.Contains(value.ToString()))
        {
            return new ValidationResult($"The field {validationContext.DisplayName} must be one of the following values: {string.Join(", ", _allowedValues)}.");
        }
        return ValidationResult.Success;
    }
}
