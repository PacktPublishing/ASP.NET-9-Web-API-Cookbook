using FluentExample.Models;

namespace FluentExample.Repositories;

public interface IEFCoreRepository
{
    Task<(IReadOnlyCollection<EventRegistration> Items, bool HasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId);

    Task<EventRegistration?> GetEventRegistrationByIdAsync(int id);

    Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration);
}

