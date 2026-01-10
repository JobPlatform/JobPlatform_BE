using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Models;
using MediatR;

namespace JobPlatform.Application.Applications.Queries;

public sealed record GetApplicationsByJobQuery(
    Guid JobId,
    int? Status,      
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ApplicationListItemDto>>;