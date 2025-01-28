namespace SignalRServer.Models;

public record LoginDTO
(
    string Username,
    string Password,
    string Locale = "en-US"
);
