using Microsoft.EntityFrameworkCore;
using events.Data;
using events.Models;

namespace events.Repositories;

public class EFCoreRepository : IEFCoreRepository
{
    private readonly AppDbContext _context;

    public EFCoreRepository(AppDbContext context)
    {
        if (context == null) {
            throw new ArgumentNullException("DB context is null"); 
        }
        _context = context;
    }

    public async Task<(IReadOnlyCollection<EventRegistration> Items, bool HasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId)
    {
        if (_context.EventRegistrations == null)
        {
                throw new InvalidOperationException("EventRegistrations DbSet is null");
        }

        var query = _context.EventRegistrations.Where(e => e.Id > lastId) .OrderBy(e => e.Id)
            .Take(pageSize + 1); 

        var result = await query.ToListAsync()!;

        var items = result.Take(pageSize).ToList().AsReadOnly();
        var hasNextPage = result.Count > pageSize;

        return (items, hasNextPage);
    }

    public async Task<EventRegistration?> GetEventRegistrationByIdAsync(int id)
    {
        if (_context.EventRegistrations == null)
        {
                throw new InvalidOperationException("EventRegistrations DbSet is null");
        } 

        return await _context.EventRegistrations.FindAsync(id);
    }

    public async Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration)
    {
        if (_context.EventRegistrations == null)
        {
                throw new InvalidOperationException("EventRegistrations DbSet is null");
        } 

        _context.EventRegistrations.Add(eventRegistration);
        await _context.SaveChangesAsync();
        return eventRegistration;
    }

     public async Task UpdateEventRegistrationAsync(EventRegistration eventRegistration)
    {
        if (_context.EventRegistrations == null)
        {
                throw new InvalidOperationException("EventRegistrations DbSet is null");
        } 

        _context.EventRegistrations.Update(eventRegistration);
        await _context.SaveChangesAsync();
    }
}
