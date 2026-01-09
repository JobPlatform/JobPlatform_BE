using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Jobs.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Jobs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Jobs.Queries;

public sealed class GetPublishedJobByIdHandler : IRequestHandler<GetPublishedJobByIdQuery, JobDetailDto>
{
    private readonly IAppDbContext _db;
    private readonly IMapper _mapper;

    public GetPublishedJobByIdHandler(IAppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<JobDetailDto> Handle(GetPublishedJobByIdQuery request, CancellationToken ct)
    {
        var job = await _db.JobPosts.AsNoTracking()
            .Include(j => j.EmployerProfile)
            .Include(j => j.SkillRequirements).ThenInclude(r => r.Skill)
            .FirstOrDefaultAsync(j => j.Id == request.JobId && j.Status == JobStatus.Published, ct);

        if (job is null) throw new NotFoundException("Job not found");
        return _mapper.Map<JobDetailDto>(job);
    }
}