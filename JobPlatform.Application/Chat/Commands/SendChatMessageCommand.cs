using JobPlatform.Application.Chat.DTOs;
using MediatR;

namespace JobPlatform.Application.Chat.Commands;

public sealed record SendChatMessageCommand(Guid JobApplicationId, string Content) : IRequest<MessageDto>;