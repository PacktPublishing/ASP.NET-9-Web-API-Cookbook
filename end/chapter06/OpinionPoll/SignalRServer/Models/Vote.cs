public record Vote
{
    public int Id { get; set; }
    public int Choice { get; set; }
    public DateTime Timestamp { get; set; }
    public required string ConnectionId { get; set; }
}
