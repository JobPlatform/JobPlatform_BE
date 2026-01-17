using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Interviews.DTOs;
using MediatR;

namespace JobPlatform.Application.Interviews.Queries;

public sealed record GetEmployerInterviewsQuery(
    DateTimeOffset From,
    DateTimeOffset To,
    bool IncludeCancelled = false,
    int Page = 1,
    int PageSize = 50
) : IRequest<PagedResult<EmployerInterviewListItemDto>>;