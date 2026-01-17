using JobPlatform.Application.Applications.DTOs;
using JobPlatform.Application.Common.Interfaces;
using MediatR;

namespace JobPlatform.Application.Applications.Commands;

public sealed record ChangeApplicationStatusCommand(Guid ApplicationId, ChangeApplicationStatusDto Dto) : IRequest,ITransactionalRequest;