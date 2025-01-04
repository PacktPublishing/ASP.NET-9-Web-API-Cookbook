namespace DataAnnotations.Models
{
    public class EventRegistration
    {
        public int Id { get; set; }

        public Guid GUID { get; set; }

        public required string FullName { get; set; }

        public required string Email { get; set; }

        public required string EventName { get; set; }

        public DateTime EventDate { get; set; }

        public int DaysAttending { get; set; }

        public string? Notes { get; set; }
    }
}

