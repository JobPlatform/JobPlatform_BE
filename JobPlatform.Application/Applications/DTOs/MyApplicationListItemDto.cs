namespace JobPlatform.Application.Applications.DTOs;

public sealed record MyApplicationListItemDto(
    Guid Id,
    Guid JobPostId,
    string JobTitle,
    string CompanyName,
    ApplicationStatus Status,
    DateTimeOffset AppliedAt,
    DateTimeOffset? StatusChangedAt
);