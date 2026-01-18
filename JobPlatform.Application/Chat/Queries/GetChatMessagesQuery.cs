using JobPlatform.Application.Chat.DTOs;
using MediatR;

namespace JobPlatform.Application.Chat.Queries;

public sealed record GetChatMessagesQuery(
    Guid JobApplicationId,
    DateTimeOffset? CursorSentAt,
    Guid? CursorId,
    int PageSize = 50
) : IRequest<CursorPagedResult<MessageDto>>;