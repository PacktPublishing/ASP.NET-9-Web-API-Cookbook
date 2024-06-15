namespace events.Models
{
    public class EventRegistration
    {
        public int Id { get; set; }

        public Guid GUID { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public int DaysAttending { get; set; }

        public string Notes { get; set; } = string.Empty;

        public AdditionalContactInfo AdditionalContact { get; set; } = new AdditionalContactInfo();
    }
}

