using JobPlatform.Application.Chat.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Chats;

using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Chat.Commands;

public sealed class SendChatMessageHandler : IRequestHandler<SendChatMessageCommand, MessageDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _current;
    private readonly IChatHubNotifier _hub; // infrastructure wrapper

    public SendChatMessageHandler(IAppDbContext db, ICurrentUserService current, IChatHubNotifier hub)
    {
        _db = db;
        _current = current;
        _hub = hub;
    }

    public async Task<MessageDto> Handle(SendChatMessageCommand request, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new ForbiddenException("Unauthorized");

        var content = request.Content?.Trim() ?? "";
        if (content.Length == 0) throw new BadRequestException("Content is required");
        if (content.Length > 2000) throw new BadRequestException("Content too long (max 2000)");

        // lấy 2 user id (1 query) + validate
        var appInfo = await _db.JobApplications.AsNoTracking()
            .Where(a => a.Id == request.JobApplicationId)
            .Select(a => new
            {
                ApplicationId = a.Id,
                CandidateUserId = a.CandidateProfile.UserId,
                EmployerUserId = a.JobPost.EmployerProfile.UserId
            })
            .FirstOrDefaultAsync(ct);

        if (appInfo is null) throw new NotFoundException("Application not found");
        if (appInfo.CandidateUserId != userId && appInfo.EmployerUserId != userId) throw new ForbiddenException();

        var convId = request.JobApplicationId;

        // ensure conversation exists + active
        var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == convId, ct);
        if (conv is null)
            throw new NotFoundException("Conversation not found. Call GET /job-applications/{id}/chat first.");

        if (!conv.IsActive) throw new BadRequestException("Conversation is not active");

        // sender & receiver
        var receiverUserId = (userId == appInfo.CandidateUserId) ? appInfo.EmployerUserId : appInfo.CandidateUserId;

        // tạo messageId chủ động để không cần SaveChanges sớm
        var msgId = Guid.NewGuid();
        var sentAt = DateTimeOffset.UtcNow;

        _db.Messages.Add(new Message
        {
            Id = msgId, // BaseEntity cần settable Id. Nếu không settable -> bỏ dòng này, SaveChanges trước rồi lấy msg.Id
            ConversationId = convId,
            SenderUserId = userId,
            Content = content,
            SentAt = sentAt
        });

        // unread++ cho receiver
        await _db.ConversationMembers
            .Where(m => m.ConversationId == convId && m.UserId == receiverUserId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.UnreadCount, x => x.UnreadCount + 1), ct);

        // sender: unread=0 + lastread = msg vừa gửi
        await _db.ConversationMembers
            .Where(m => m.ConversationId == convId && m.UserId == userId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.UnreadCount, 0)
                .SetProperty(x => x.LastReadMessageId, msgId)
                .SetProperty(x => x.LastReadAt, sentAt), ct);

        await _db.SaveChangesAsync(ct);

        var dto = new MessageDto(msgId, convId, userId, content, sentAt);

        // push realtime tới group application
        await _hub.PushNewMessageAsync(request.JobApplicationId, dto, ct);

        // optional: push unread delta tới user group (không query thêm)
        await _hub.PushUnreadDeltaAsync(receiverUserId, request.JobApplicationId, +1, ct);

        return dto;
    }
}
