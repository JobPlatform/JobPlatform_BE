using JobPlatform.Application.Common.Models;
using JobPlatform.Application.Interviews.DTOs;
using MediatR;

namespace JobPlatform.Application.Interviews.Queries;

public sealed record GetMyInterviewsQuery(
    DateTimeOffset? From,
    DateTimeOffset? To,
    bool UpcomingOnly = true,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CandidateInterviewListItemDto>>;