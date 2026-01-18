using MediatR;

namespace JobPlatform.Application.Chat.Commands;

public sealed record MarkChatReadCommand(Guid JobApplicationId, Guid? LastMessageId) : IRequest;