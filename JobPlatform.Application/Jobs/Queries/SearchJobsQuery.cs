using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Jobs.DTOs;
using MediatR;

namespace JobPlatform.Application.Jobs.Queries;

public sealed record SearchJobsQuery(
    string? Q,
    string? Location,
    int? WorkMode,
    decimal? SalaryMin,
    decimal? SalaryMax,
    List<string>? Skills,
    string? Sort,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<JobListItemDto>>;