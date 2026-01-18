using JobPlatform.Application.Chat.DTOs;
using MediatR;

namespace JobPlatform.Application.Chat.Queries;

public sealed record GetOrCreateChatSummaryQuery(Guid JobApplicationId) : IRequest<ChatSummaryDto>;