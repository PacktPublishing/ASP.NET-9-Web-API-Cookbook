using AutoMapper;

namespace events.Models;

public class EventProfile : Profile
{
    public EventProfile()
    {
        CreateMap<EventRegistrationDTO, EventRegistration>()
            .ForMember(dest => dest.AdditionalContact, opt => opt.MapFrom(src => src.AdditionalContact));
        CreateMap<AdditionalContactInfoDTO, AdditionalContactInfo>();
        CreateMap<EventRegistration, EventRegistrationDTO>()
            .ForMember(dest => dest.AdditionalContact, opt => opt.MapFrom(src => src.AdditionalContact));
        CreateMap<AdditionalContactInfo, AdditionalContactInfoDTO>();
    }
}
