using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Applications.Queries;

public sealed record GetMyApplicationsQuery(
    int? Status,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<MyApplicationListItemDto>>;