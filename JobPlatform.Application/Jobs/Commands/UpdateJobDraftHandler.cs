using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Jobs.Commands;

public sealed class UpdateJobDraftHandler : IRequestHandler<UpdateJobDraftCommand, JobDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateJobDraftHandler(IAppDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<JobDetailDto> Handle(UpdateJobDraftCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var job = await _db.JobPosts
            .Include(j => j.EmployerProfile)
            .Include(j => j.SkillRequirements).ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, ct);

        if (job is null) throw new KeyNotFoundException("Job not found");
        if (job.EmployerProfile.UserId != userId) throw new UnauthorizedAccessException("Forbidden");
        if (job.Status != JobStatus.Draft) throw new InvalidOperationException("Only Draft job can be updated");

        job.Title = request.Dto.Title;
        job.Description = request.Dto.Description ?? "";
        job.Location = request.Dto.Location;
        job.WorkMode = (WorkMode)request.Dto.WorkMode;
        job.SalaryMin = request.Dto.SalaryMin;
        job.SalaryMax = request.Dto.SalaryMax;
        job.UpdatedAt = DateTimeOffset.UtcNow;

        // replace requirements
        job.SkillRequirements.Clear();
        await ReplaceRequirementsAsync(job, request.Dto.Requirements, ct);

        await _db.SaveChangesAsync(ct);

        // reload requirements -> skill
        var updated = await _db.JobPosts
            .Include(j => j.EmployerProfile)
            .Include(j => j.SkillRequirements).ThenInclude(r => r.Skill)
            .FirstAsync(j => j.Id == request.JobId, ct);

        return _mapper.Map<JobDetailDto>(updated);
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
                throw new NotFoundException($"SkillId '{r.SkillId}' is invalid or inactive.");

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

