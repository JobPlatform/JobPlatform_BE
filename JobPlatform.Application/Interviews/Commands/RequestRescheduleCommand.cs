using MediatR;

namespace JobPlatform.Application.Interviews.Commands;

using JobPlatform.Application.Interviews.DTOs;

public sealed record RequestRescheduleCommand(Guid InterviewId, RequestRescheduleDto Dto) : IRequest;
