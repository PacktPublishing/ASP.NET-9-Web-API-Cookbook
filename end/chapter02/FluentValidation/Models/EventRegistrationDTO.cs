namespace events.Models;

public class EventRegistrationDTO
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string EventName { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string ConfirmEmail { get; set; } = string.Empty;

    public int DaysAttending { get; set; }

}
