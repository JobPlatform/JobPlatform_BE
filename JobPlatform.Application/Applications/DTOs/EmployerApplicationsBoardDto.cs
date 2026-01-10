using JobPlatform.Domain.Entities;

namespace JobPlatform.Application.Applications.DTOs;

public sealed record EmployerBoardApplicationItemDto(
    Guid Id,
    Guid JobPostId,
    string JobTitle,
    Guid CandidateProfileId,
    string CandidateName,
    string? CandidateHeadline,
    ApplicationStatus Status,
    DateTimeOffset AppliedAt,
    DateTimeOffset? StatusChangedAt,
    List<ApplicationStatus> AllowedNextStatuses
);

public sealed record BoardColumnDto(
    int TotalCount,
    List<EmployerBoardApplicationItemDto> Items
);

public sealed record EmployerApplicationsBoardDto(
    BoardColumnDto Applied,
    BoardColumnDto Reviewed,
    BoardColumnDto Interview,
    BoardColumnDto Offer,
    BoardColumnDto Rejected
);