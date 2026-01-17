using MediatR;

namespace JobPlatform.Application.Interviews.Commands;

using JobPlatform.Application.Interviews.DTOs;
public sealed record RescheduleInterviewCommand(Guid InterviewId, RescheduleInterviewDto Dto) : IRequest;
