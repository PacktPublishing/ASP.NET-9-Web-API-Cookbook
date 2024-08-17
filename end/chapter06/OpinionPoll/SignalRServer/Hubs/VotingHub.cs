using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class VotingHub : Hub
{
    private readonly VoteDbContext _context;

    public VotingHub(VoteDbContext context)
    {
        _context = context;
    }

    public async Task Vote(int choice)
    {
        if (choice != 1 && choice != 2)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "Invalid choice. Please vote 1 for Super Nintendo or 2 for Sega Genesis.");
            return;
        }

        var connectionId = Context.ConnectionId;

        if (await _context.Votes.AnyAsync(v => v.ConnectionId == connectionId))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "You have already voted.");
            return;
        }

        try
        {
            var vote = new Vote { Choice = choice, Timestamp = DateTime.UtcNow, ConnectionId = connectionId };
            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            var results = await GetVoteResults();

            await Clients.All.SendAsync("ReceiveVoteResults", results);
            await Clients.Caller.SendAsync("ReceiveMessage", $"Your vote for option {choice} has been recorded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recording vote: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveMessage", "An error occurred while recording your vote. Please try again.");
        }
    }

    public async Task GetCurrentResults()
    {
        var results = await GetVoteResults();
        await Clients.Caller.SendAsync("ReceiveVoteResults", results);
    }

    private async Task<object> GetVoteResults()
    {
        return await _context.Votes
            .GroupBy(v => v.Choice)
            .Select(g => new { Choice = g.Key, Count = g.Count() })
            .ToListAsync();
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}
