using CustomAnnotations.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomAnnotations.Services;

public interface IEFCoreService
{
    Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(int pageSize, int lastId, IUrlHelper urlHelper);

    Task<EventRegistrationDTO?> GetEventRegistrationByIdAsync(int id);

    Task<EventRegistrationDTO> CreateEventRegistrationAsync(EventRegistrationDTO eventRegistrationDTO);

}

