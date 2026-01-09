using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Candidates.DTOs;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Profiles;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Candidates.Commands;

public sealed class UpdateMyCandidateSkillsHandler : IRequestHandler<UpdateMyCandidateSkillsCommand, CandidateProfileDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateMyCandidateSkillsHandler(IAppDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<CandidateProfileDto> Handle(UpdateMyCandidateSkillsCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var profile = await _db.CandidateProfiles
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) throw new NotFoundException("Candidate profile not found");

        if (request.Dto.Skills is null)
            throw new BadRequestException("Skills is required");

        // basic validation + distinct
        var items = request.Dto.Skills
            .GroupBy(x => x.SkillId)
            .Select(g => g.First())
            .ToList();

        foreach (var s in items)
        {
            if (s.Level is < 1 or > 5)
                throw new BadRequestException("Skill level must be between 1 and 5");
            if (s.Years < 0 || s.Years > 60)
                throw new BadRequestException("Years must be between 0 and 60");
        }

        // Validate skill ids exist & active
        var skillIds = items.Select(x => x.SkillId).Distinct().ToList();

        var activeSkills = await _db.Skills
            .AsNoTracking()
            .Where(x => x.IsActive && skillIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ct);

        if (activeSkills.Count != skillIds.Count)
            throw new BadRequestException("One or more SkillId is invalid or inactive.");

        // Replace all skills
        profile.Skills.Clear();
        foreach (var s in items)
        {
            profile.Skills.Add(new CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId = s.SkillId,
                Level = s.Level,
                Years = s.Years
            });
        }

        profile.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var saved = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstAsync(p => p.UserId == userId, ct);

        return _mapper.Map<CandidateProfileDto>(saved);
    }
}
