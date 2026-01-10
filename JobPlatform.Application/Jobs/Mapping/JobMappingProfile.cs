using AutoMapper;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities.Jobs;

namespace JobPlatform.Application.Jobs.Mapping;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<JobSkillRequirement, JobRequirementDetailDto>()
            .ForCtorParam("SkillId", opt => opt.MapFrom(s => s.SkillId))
            .ForCtorParam("SkillName", opt => opt.MapFrom(s => s.Skill.Name))
            .ForCtorParam("CategoryName", opt => opt.MapFrom(s => s.Skill.Category.Name))
            
            .ForCtorParam("RequiredLevel", opt => opt.MapFrom(s => s.RequiredLevel))
            .ForCtorParam("IsMustHave", opt => opt.MapFrom(s => s.IsMustHave))
            .ForCtorParam("Weight", opt => opt.MapFrom(s => s.Weight))
            .ForCtorParam("RequirementDescription", opt => opt.MapFrom(s => s.RequirementDescription));

        CreateMap<JobPost, JobDetailDto>()
            // các field map thẳng thường AutoMapper tự hiểu, nhưng cứ rõ ràng cho chắc
            .ForCtorParam("Id", opt => opt.MapFrom(s => s.Id))
            .ForCtorParam("Title", opt => opt.MapFrom(s => s.Title))
            .ForCtorParam("Description", opt => opt.MapFrom(s => s.Description))
            .ForCtorParam("Location", opt => opt.MapFrom(s => s.Location))

            // WorkMode (nếu entity là enum) -> dto int
            .ForCtorParam("WorkMode", opt => opt.MapFrom(s => (int)s.WorkMode))

            .ForCtorParam("SalaryMin", opt => opt.MapFrom(s => s.SalaryMin))
            .ForCtorParam("SalaryMax", opt => opt.MapFrom(s => s.SalaryMax))

            // Status (nếu entity là enum) -> dto string
            .ForCtorParam("Status", opt => opt.MapFrom(s => s.Status.ToString()))

            .ForCtorParam("PublishedAt", opt => opt.MapFrom(s => s.PublishedAt))

            // CompanyName thường là navigation
            .ForCtorParam("CompanyName", opt => opt.MapFrom(s => s.EmployerProfile.CompanyName))

            // Requirements navigation -> List<JobRequirementDto>
            .ForCtorParam("Requirements", opt => opt.MapFrom(s => s.SkillRequirements)).ReverseMap();
        
        
            

        CreateMap<JobPost, JobListItemDto>()
    .ForCtorParam("Id", opt => opt.MapFrom(s => s.Id))
    .ForCtorParam("Title", opt => opt.MapFrom(s => s.Title))
    .ForCtorParam("Location", opt => opt.MapFrom(s => s.Location))
    .ForCtorParam("WorkMode", opt => opt.MapFrom(s => (int)s.WorkMode))
    .ForCtorParam("SalaryMin", opt => opt.MapFrom(s => s.SalaryMin))
    .ForCtorParam("SalaryMax", opt => opt.MapFrom(s => s.SalaryMax))
    .ForCtorParam("PublishedAt", opt => opt.MapFrom(s => s.PublishedAt))
    .ForCtorParam("CompanyName", opt => opt.MapFrom(s => s.EmployerProfile.CompanyName));

        // CreateMap<JobPost, JobDetailDto>()
        //     .ForMember(d => d.CompanyName, opt => opt.MapFrom(s => s.EmployerProfile.CompanyName))
        //     .ForMember(d => d.WorkMode, opt => opt.MapFrom(s => (int)s.WorkMode))
        //     .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
        //     .ForMember(d => d.Requirements, opt => opt.MapFrom(s => s.SkillRequirements));
    }
}