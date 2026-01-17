using MediatR;

namespace JobPlatform.Application.Interviews.Commands;

public sealed record ConfirmInterviewCommand(Guid InterviewId) : IRequest;
