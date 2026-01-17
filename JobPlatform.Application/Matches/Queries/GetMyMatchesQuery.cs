using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Matches.DTOs;
using MediatR;

namespace JobPlatform.Application.Matches.Queries;

public sealed record GetMyMatchesQuery(
    Guid? JobId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CandidateJobMatchDto>>;