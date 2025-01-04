using System.ComponentModel.DataAnnotations;

namespace DataAnnotations.Models;

public class EventRegistrationDTO
{
    public int Id { get; set; }

    [Required]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string EventName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; }

    [Required]
    [Compare("Email", ErrorMessage = "Email addresses do not match.")]
    public required string ConfirmEmail { get; set; }

    [Required]
    [Range(1, 7, ErrorMessage = "Number of days attending must be between 1 and 7.")]
    public int DaysAttending { get; set; }
}
