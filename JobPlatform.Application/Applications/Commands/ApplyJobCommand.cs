using JobPlatform.Application.Applications.DTOs;
using MediatR;

namespace JobPlatform.Application.Applications.Commands;

public sealed record ApplyJobCommand(Guid JobId, ApplyJobDto Dto) : IRequest<Guid>; // return ApplicationId