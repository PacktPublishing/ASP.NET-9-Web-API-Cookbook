using events.Models;

namespace events.Repositories;

public interface IEventsRepository
{
    Task<(IReadOnlyCollection<EventRegistration> Items, bool HasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId);

    Task<EventRegistration?> GetEventRegistrationByIdAsync(int id);

    Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration);
}

