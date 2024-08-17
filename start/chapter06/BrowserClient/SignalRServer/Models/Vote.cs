public class Vote
{
    public int Id { get; set; }
    public int Choice { get; set; }
    public DateTime Timestamp { get; set; }
    public string ConnectionId { get; set; } = String.Empty;
}
