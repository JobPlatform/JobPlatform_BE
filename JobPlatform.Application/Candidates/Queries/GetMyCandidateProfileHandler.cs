using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Candidates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Candidates.Queries;

public sealed class GetMyCandidateProfileHandler : IRequestHandler<GetMyCandidateProfileQuery, CandidateProfileDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public GetMyCandidateProfileHandler(IAppDbContext db, ICurrentUserService currentUser, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<CandidateProfileDto> Handle(GetMyCandidateProfileQuery request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        var profile = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) throw new NotFoundException("Candidate profile not found");

        return _mapper.Map<CandidateProfileDto>(profile);
    }
}