using MediatR;

namespace JobPlatform.Application.Interviews.Commands;

public sealed record CancelInterviewCommand(Guid InterviewId, string? Reason) : IRequest;
