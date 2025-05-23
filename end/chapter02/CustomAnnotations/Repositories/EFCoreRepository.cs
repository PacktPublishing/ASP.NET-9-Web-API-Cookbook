using Microsoft.EntityFrameworkCore;
using CustomAnnotations.Data;
using CustomAnnotations.Models;

namespace CustomAnnotations.Repositories;

public class EFCoreRepository : IEFCoreRepository
{
    private readonly AppDbContext _context;

    public EFCoreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyCollection<EventRegistration> Items, bool HasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId)
    {
        var query = _context.EventRegistrations .Where(e => e.Id > lastId) .OrderBy(e => e.Id)
            .Take(pageSize + 1); // Fetch one more record to determine HasNextPage

        var result = await query.ToListAsync();

        var items = result.Take(pageSize).ToList().AsReadOnly();
        var hasNextPage = result.Count > pageSize;

        return (items, hasNextPage);
    }

    public async Task<EventRegistration?> GetEventRegistrationByIdAsync(int id)
    {
        return await _context.EventRegistrations.FindAsync(id);
    }

    public async Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration)
    {
        _context.EventRegistrations.Add(eventRegistration);
        await _context.SaveChangesAsync();
        return eventRegistration;
    }
}
