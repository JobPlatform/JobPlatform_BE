using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Profiles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Candidates.Commands;

public sealed class UpsertMyCandidateProfileHandler : IRequestHandler<UpsertMyCandidateProfileCommand, CandidateProfileDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpsertMyCandidateProfileHandler(IAppDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<CandidateProfileDto> Handle(UpsertMyCandidateProfileCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        if (string.IsNullOrWhiteSpace(request.Dto.FullName))
            throw new BadRequestException("FullName is required");

        if (request.Dto.ExpectedSalaryMin.HasValue && request.Dto.ExpectedSalaryMax.HasValue
            && request.Dto.ExpectedSalaryMin.Value > request.Dto.ExpectedSalaryMax.Value)
            throw new BadRequestException("ExpectedSalaryMin must be <= ExpectedSalaryMax");

        var profile = await _db.CandidateProfiles
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null)
        {
            profile = new CandidateProfile
            {
                UserId = userId,
                FullName = request.Dto.FullName.Trim(),
                Headline = request.Dto.Headline?.Trim(),
                Location = request.Dto.Location?.Trim(),
                ExpectedSalaryMin = request.Dto.ExpectedSalaryMin,
                ExpectedSalaryMax = request.Dto.ExpectedSalaryMax,
                PreferRemote = request.Dto.PreferRemote
            };
            _db.CandidateProfiles.Add(profile);
        }
        else
        {
            profile.FullName = request.Dto.FullName.Trim();
            profile.Headline = request.Dto.Headline?.Trim();
            profile.Location = request.Dto.Location?.Trim();
            profile.ExpectedSalaryMin = request.Dto.ExpectedSalaryMin;
            profile.ExpectedSalaryMax = request.Dto.ExpectedSalaryMax;
            profile.PreferRemote = request.Dto.PreferRemote;
            profile.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        // reload nav for dto
        var saved = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstAsync(p => p.UserId == userId, ct);

        return _mapper.Map<CandidateProfileDto>(saved);
    }
}
