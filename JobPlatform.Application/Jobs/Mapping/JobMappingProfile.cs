using AutoMapper;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities.Jobs;

namespace JobPlatform.Application.Jobs.Mapping;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<JobSkillRequirement, JobRequirementDto>();
            

        CreateMap<JobPost, JobListItemDto>()
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.EmployerProfile.CompanyName))
            .ForMember(d => d.WorkMode, opt => opt.MapFrom(s => (int)s.WorkMode));

        CreateMap<JobPost, JobDetailDto>()
            .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.EmployerProfile.CompanyName))
            .ForMember(d => d.WorkMode, opt => opt.MapFrom(s => (int)s.WorkMode))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Requirements, opt => opt.MapFrom(s => s.SkillRequirements));
    }
}