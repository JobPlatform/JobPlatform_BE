using AutoMapper;
using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Domain.Entities.Profiles;

namespace JobPlatform.Application.Candidates.Mapping;

public class CandidateMappingProfile : Profile
{
    public CandidateMappingProfile()
    {
        CreateMap<CandidateSkill, CandidateSkillDto>()
            .ForMember(d => d.SkillCode, opt => opt.MapFrom(s => s.Skill.Code))
            .ForMember(d => d.SkillName, opt => opt.MapFrom(s => s.Skill.Name));

        CreateMap<CandidateProfile, CandidateProfileDto>()
            .ForMember(d => d.Skills, opt => opt.MapFrom(s => s.Skills));
    }
}