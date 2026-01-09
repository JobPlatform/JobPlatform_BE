using MediatR;

namespace JobPlatform.Application.Candidates.DTOs;

public sealed record UploadMyCvCommand(
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<CandidateProfileDto>;