namespace JobPlatform.Application.Interviews.Commands;

using MediatR;
using JobPlatform.Application.Interviews.DTOs;

public sealed record CreateInterviewCommand(Guid ApplicationId, CreateInterviewDto Dto) : IRequest<Guid>;
