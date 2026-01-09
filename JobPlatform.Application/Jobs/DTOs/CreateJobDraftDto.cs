namespace JobPlatform.Application.Jobs.DTOs;

public sealed record CreateJobDraftDto(
    string CompanyName,              // để auto tạo EmployerProfile nếu chưa có
    string Title,
    string? Description,
    string? Location,
    int WorkMode,                    // map enum WorkMode
    decimal? SalaryMin,
    decimal? SalaryMax,
    List<JobRequirementDto>? Requirements
);