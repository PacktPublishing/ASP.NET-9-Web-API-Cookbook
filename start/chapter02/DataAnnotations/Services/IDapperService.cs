using DataAnnotations.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataAnnotations.Services;

public interface IDapperService
{
    Task<PagedResult<EventRegistrationDTO>> GetEventRegistrationsAsync(int pageSize, int lastId, IUrlHelper urlHelper);

    Task<EventRegistrationDTO?> GetEventRegistrationByIdAsync(int id);
}
