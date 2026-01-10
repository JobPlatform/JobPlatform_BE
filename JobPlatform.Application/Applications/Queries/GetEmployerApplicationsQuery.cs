using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Applications.Queries;

public sealed record GetEmployerApplicationsQuery(
    int? Status,
    string? Sort,   // "applied_desc" | "applied_asc" | "status_changed_desc"
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<EmployerApplicationKanbanItemDto>>;