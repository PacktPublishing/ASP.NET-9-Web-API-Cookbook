using events.Models;
using Microsoft.AspNetCore.Mvc;

namespace events.Services;

public interface IEFCoreService
{
    Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(int pageSize, int lastId, IUrlHelper urlHelper);

    Task<EventRegistrationDTO?> GetEventRegistrationByIdAsync(int id);

}

