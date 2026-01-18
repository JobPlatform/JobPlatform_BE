using JobPlatform.Application.Chat.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Chats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Chat.Queries;

public sealed class GetOrCreateChatSummaryHandler
    : IRequestHandler<GetOrCreateChatSummaryQuery, ChatSummaryDto>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _current;

    public GetOrCreateChatSummaryHandler(IAppDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<ChatSummaryDto> Handle(GetOrCreateChatSummaryQuery request, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new ForbiddenException("Unauthorized");

        
        var appInfo = await _db.JobApplications.AsNoTracking()
            .Where(a => a.Id == request.JobApplicationId)
            .Select(a => new
            {
                ApplicationId = a.Id,
                CandidateUserId = a.CandidateProfile.UserId,
                CandidateName = a.CandidateProfile.FullName,
                EmployerUserId = a.JobPost.EmployerProfile.UserId,
                EmployerName = a.JobPost.EmployerProfile.CompanyName
            })
            .FirstOrDefaultAsync(ct);

        if (appInfo is null) throw new NotFoundException("Application not found");

        var isCandidate = appInfo.CandidateUserId == userId;
        var isEmployer = appInfo.EmployerUserId == userId;
        if (!isCandidate && !isEmployer) throw new ForbiddenException();

        var otherUserId = isCandidate ? appInfo.EmployerUserId : appInfo.CandidateUserId;
        var otherName = isCandidate ? appInfo.EmployerName : appInfo.CandidateName;

       
        var convId = request.JobApplicationId;

        // Create if not exists (race-safe)
        var exists = await _db.Conversations.AsNoTracking().AnyAsync(c => c.Id == convId, ct);
        if (!exists)
        {
            _db.Conversations.Add(new Conversation
            {
                Id = convId,
                JobApplicationId = request.JobApplicationId,
                IsActive = true
            });

            _db.ConversationMembers.AddRange(
                new ConversationMember { ConversationId = convId, UserId = appInfo.CandidateUserId },
                new ConversationMember { ConversationId = convId, UserId = appInfo.EmployerUserId }
            );

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
               
            }
        }

        var conv = await _db.Conversations.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == convId, ct);
        if (conv is null) throw new NotFoundException("Conversation not found");

        var unread = await _db.ConversationMembers.AsNoTracking()
            .Where(m => m.ConversationId == convId && m.UserId == userId)
            .Select(m => m.UnreadCount)
            .FirstAsync(ct);

        var lastMsg = await _db.Messages.AsNoTracking()
            .Where(m => m.ConversationId == convId)
            .OrderByDescending(m => m.SentAt).ThenByDescending(m => m.Id)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderUserId, m.Content, m.SentAt))
            .FirstOrDefaultAsync(ct);

        return new ChatSummaryDto(
            ConversationId: convId,
            JobApplicationId: request.JobApplicationId,
            OtherUserId: otherUserId,
            OtherDisplayName: otherName,
            UnreadCount: unread,
            IsActive: conv.IsActive,
            LastMessage: lastMsg
        );
    }
}
