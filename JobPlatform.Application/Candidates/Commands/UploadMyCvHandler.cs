using AutoMapper;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Candidates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Candidates.Commands;

public sealed class UploadMyCvHandler : IRequestHandler<UploadMyCvCommand, CandidateProfileDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorage _storage;
    private readonly IMapper _mapper;

    public UploadMyCvHandler(IAppDbContext db, ICurrentUserService currentUser, IFileStorage storage, IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _storage = storage;
        _mapper = mapper;
    }

    public async Task<CandidateProfileDto> Handle(UploadMyCvCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("Unauthorized");

        // basic validation
        if (string.IsNullOrWhiteSpace(request.FileName))
            throw new BadRequestException("FileName is required");

        var allowed = new[] { "application/pdf" };
        if (!allowed.Contains(request.ContentType))
            throw new BadRequestException("Only PDF is allowed");

        var profile = await _db.CandidateProfiles
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) throw new NotFoundException("Candidate profile not found");

        var (path, originalName, contentType, size) =
            await _storage.SaveCandidateCvAsync(userId, request.FileStream, request.FileName, request.ContentType, ct);

        profile.CvStoragePath = path;
        profile.CvFileName = originalName;
        profile.CvContentType = contentType;
        profile.CvFileSize = size;
        profile.CvUploadedAt = DateTimeOffset.UtcNow;
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        var saved = await _db.CandidateProfiles
            .AsNoTracking()
            .Include(p => p.Skills).ThenInclude(s => s.Skill)
            .FirstAsync(p => p.UserId == userId, ct);

        return _mapper.Map<CandidateProfileDto>(saved);
    }
}
