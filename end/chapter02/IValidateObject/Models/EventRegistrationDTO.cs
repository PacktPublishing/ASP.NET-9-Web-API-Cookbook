using System.ComponentModel.DataAnnotations;

namespace events.Models;

public class EventRegistrationDTO : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [AllowedValues("C# Conference", "WebAPI Workshop", ".NET Hangout")]
    public string EventName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; }

    [Required]
    [Compare("Email", ErrorMessage = "Email addresses do not match.")]
    public string ConfirmEmail { get; set; } = string.Empty;

    [Required]
    [Range(1, 7, ErrorMessage = "Number of days attending must be between 1 and 7.")]
    public int DaysAttending { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EventDate < DateTime.Now)
        {
            yield return new ValidationResult(
                "Event date must be in the future.",
                new[] { nameof(EventDate) });

        }

        if ((FullName.Contains("Garry") || FullName.Contains("Luke")) && EventName == "C# Conference")
            {
                yield return new ValidationResult(
                    $"{FullName} is banned from {EventName}.",
                    new[] { nameof(FullName), nameof(EventName) });
            }

    }
}
