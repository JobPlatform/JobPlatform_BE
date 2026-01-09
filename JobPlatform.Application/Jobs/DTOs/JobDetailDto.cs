namespace JobPlatform.Application.Jobs.DTOs;

public sealed record JobDetailDto(
    Guid Id,
    string Title,
    string Description,
    string? Location,
    int WorkMode,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string Status,
    DateTimeOffset? PublishedAt,
    string CompanyName,
    List<JobRequirementDto> Requirements
);