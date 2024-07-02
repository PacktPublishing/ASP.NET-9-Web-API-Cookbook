using events.Repositories;
using events.Models;
using Microsoft.AspNetCore.Mvc;

namespace events.Services;

public class EventsService : IEventsService
{
    private readonly IEventsRepository _repository;

    public EventsService(IEventsRepository repository)
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

    public async Task<EventRegistrationDTO> CreateEventRegistrationAsync(EventRegistrationDTO eventRegistrationDTO) 
    {
        var eventRegistration = new EventRegistration
        {
            FullName = eventRegistrationDTO.FullName,
            Email = eventRegistrationDTO.Email,
            EventName = eventRegistrationDTO.EventName,
            EventDate = eventRegistrationDTO.EventDate,
            DaysAttending = eventRegistrationDTO.DaysAttending
        };

        var result = await _repository.CreateEventRegistrationAsync(eventRegistration);

        return new EventRegistrationDTO
        {
            Id = result.Id,
            FullName = result.FullName,
            Email = result.Email,
            EventName = result.EventName,
            EventDate = result.EventDate,
            ConfirmEmail = result.Email,
            DaysAttending = result.DaysAttending
        };
    }


}

