using events.Repositories;
using events.Models;
using Microsoft.AspNetCore.Mvc;

namespace events.Services;

public class DapperService : IDapperService
{
    private readonly IDapperRepository _repository;

    public DapperService(IDapperRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(int pageSize, int lastId, IUrlHelper urlHelper)
    {
        var (eventRegistrations, hasNextPage) = await _repository.GetEventRegistrationsAsync(pageSize, lastId);

        var items = eventRegistrations.Select(e => new EventRegistrationDTO
        {
            Id = e.Id,
            FullName = e.FullName,
            Email = e.Email,
            EventName = e.EventName,
            EventDate = e.EventDate,
            ConfirmEmail = e.Email,
            DaysAttending = e.DaysAttending
        }).ToList().AsReadOnly();

        var hasPreviousPage = lastId > 0;
        var previousPageUrl = hasPreviousPage
            ? urlHelper.Action("GetEventRegistrations", new { pageSize, lastId = items.First().Id })
            : null;
        var nextPageUrl = hasNextPage
            ? urlHelper.Action("GetEventRegistrations", new { pageSize, lastId = items.Last().Id })
            : null;

        return new PagedResult<EventRegistrationDTO>
        {
            Items = items,
            HasPreviousPage = hasPreviousPage,
            HasNextPage = hasNextPage,
            PreviousPageUrl = previousPageUrl,
            NextPageUrl = nextPageUrl,
            PageSize = pageSize
        };
    }

    public async Task<EventRegistrationDTO?> GetEventRegistrationByIdAsync(int id)
    {
        var eventRegistration = await _repository.GetEventRegistrationByIdAsync(id);
        if (eventRegistration == null) return null;

        return new EventRegistrationDTO
        {
            Id = eventRegistration.Id,
            FullName = eventRegistration.FullName,
            Email = eventRegistration.Email,
            EventName = eventRegistration.EventName,
            EventDate = eventRegistration.EventDate,
            ConfirmEmail = eventRegistration.Email,
            DaysAttending = eventRegistration.DaysAttending,
        };
    }
}
