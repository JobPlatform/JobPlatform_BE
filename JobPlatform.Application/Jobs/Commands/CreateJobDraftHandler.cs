using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Jobs;
using JobPlatform.Domain.Entities.Profiles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Jobs.Commands;

public class CreateJobDraftHandler : IRequestHandler<CreateJobDraftCommand,JobDetailDto>
{

    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    
    public CreateJobDraftHandler(IAppDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }


    public async Task<JobDetailDto> Handle(CreateJobDraftCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        var employer = await _db.EmployerProfiles.FirstOrDefaultAsync(x => x.UserId == userId);
        if (employer is null)
        {
            employer = new EmployerProfile{UserId = userId, CompanyName = request.Dto.CompanyName};
            _db.EmployerProfiles.Add(employer);
        }
        var job = new JobPost
        {
            EmployerProfile = employer,
            Title = request.Dto.Title,
            Description = request.Dto.Description ?? "",
            Location = request.Dto.Location,
            WorkMode = (WorkMode)request.Dto.WorkMode,
            SalaryMin = request.Dto.SalaryMin,
            SalaryMax = request.Dto.SalaryMax,
            Status = JobStatus.Draft
        };
        await ReplaceRequirementsAsync(job, request.Dto.Requirements, ct);
        _db.JobPosts.Add(job);
        await _db.SaveChangesAsync(ct);

        // load nav for mapping
        var created = await _db.JobPosts
            .Include(j => j.EmployerProfile)
            .Include(j => j.SkillRequirements).ThenInclude(r => r.Skill)
            .FirstAsync(j => j.Id == job.Id, ct);

        return _mapper.Map<JobDetailDto>(created);
        
        
        
    }
    
    private async Task ReplaceRequirementsAsync(JobPost job, List<JobRequirementDto>? reqs, CancellationToken ct)
    {
        job.SkillRequirements.Clear();
        if (reqs is null || reqs.Count == 0) return;

        // distinct theo SkillId để tránh trùng line
        foreach (var r in reqs.GroupBy(x => x.SkillId).Select(g => g.First()))
        {
            var skill = await _db.Skills
                .FirstOrDefaultAsync(s => s.Id == r.SkillId && s.IsActive, ct);

            if (skill is null)
                throw new BadRequestException($"SkillId '{r.SkillId}' is invalid or inactive.");

            job.SkillRequirements.Add(new JobSkillRequirement
            {
                SkillId = skill.Id,
                RequiredLevel = r.RequiredLevel,
                IsMustHave = r.IsMustHave,
                Weight = r.Weight <= 0 ? 1 : r.Weight,
                RequirementDescription = string.IsNullOrWhiteSpace(r.RequirementDescription)
                    ? null
                    : r.RequirementDescription.Trim()
            });
        }
    }
}