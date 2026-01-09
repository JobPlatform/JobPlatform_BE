namespace JobPlatform.Application.Jobs.DTOs;

public sealed record UpdateJobDraftDto(
    string Title,
    string? Description,
    string? Location,
    int WorkMode,
    decimal? SalaryMin,
    decimal? SalaryMax,
    List<JobRequirementDto>? Requirements
);