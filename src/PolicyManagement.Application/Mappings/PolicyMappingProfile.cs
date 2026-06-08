using AutoMapper;
using PolicyManagement.Application.DTOs;
using PolicyManagement.Domain.Entities;

namespace PolicyManagement.Application.Mappings;

public class PolicyMappingProfile : Profile
{
    public PolicyMappingProfile()
    {
        CreateMap<Policy, PolicyDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.LineOfBusiness,
                opt => opt.MapFrom(src => src.LineOfBusiness.ToString()));
    }
}
