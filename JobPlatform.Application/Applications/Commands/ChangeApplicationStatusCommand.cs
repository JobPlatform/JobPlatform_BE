using JobPlatform.Application.Applications.DTOs;
using MediatR;

namespace JobPlatform.Application.Applications.Commands;

public sealed record ChangeApplicationStatusCommand(Guid ApplicationId, ChangeApplicationStatusDto Dto) : IRequest;