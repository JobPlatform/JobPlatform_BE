using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Jobs.Commands;

public sealed class PublishJobHandler : IRequestHandler<PublishJobCommand, JobDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public PublishJobHandler(IAppDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<JobDetailDto> Handle(PublishJobCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("Unauthorized");

        var job = await _db.JobPosts
            .Include(j => j.EmployerProfile)
            .Include(j => j.SkillRequirements).ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(j => j.Id == request.JobId, ct);

        if (job is null) throw new NotFoundException("Job not found");
        if (job.EmployerProfile.UserId != userId) throw new UnauthorizedAccessException("Forbidden");
        if (job.Status != JobStatus.Draft) throw new InvalidOperationException("Only Draft job can be published");

        // publish rules (MVP)
        if (string.IsNullOrWhiteSpace(job.Title)) throw new InvalidOperationException("Title is required");
        if (string.IsNullOrWhiteSpace(job.Description)) throw new InvalidOperationException("Description is required");

        job.Status = JobStatus.Published;
        job.PublishedAt = DateTimeOffset.UtcNow;
        job.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<JobDetailDto>(job);
    }
}