namespace SignalRServer.Models;

public record User(
    int Id,
    string Username,
    string PasswordHash,
    bool IsAdmin
);
