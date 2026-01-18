using JobPlatform.Application.Interviews.DTOs;
using MediatR;

namespace JobPlatform.Application.Interviews.Queries;

public sealed record GetCandidateInterviewDetailQuery(Guid InterviewId) : IRequest<InterviewDetailDto>;