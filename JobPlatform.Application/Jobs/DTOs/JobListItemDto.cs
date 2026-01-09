namespace JobPlatform.Application.Jobs.DTOs;

public sealed record JobListItemDto(
    Guid Id,
    string Title,
    string? Location,
    int WorkMode,
    decimal? SalaryMin,
    decimal? SalaryMax,
    DateTimeOffset? PublishedAt,
    string CompanyName
);