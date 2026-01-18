using JobPlatform.Application.Chat.DTOs;
using JobPlatform.Application.Common.Exceptions;
using JobPlatform.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Application.Chat.Queries;

public sealed class GetChatMessagesHandler
    : IRequestHandler<GetChatMessagesQuery, CursorPagedResult<MessageDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _current;

    public GetChatMessagesHandler(IAppDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    public async Task<CursorPagedResult<MessageDto>> Handle(GetChatMessagesQuery request, CancellationToken ct)
    {
        var userId = _current.UserId ?? throw new ForbiddenException("Unauthorized");

        
        var appUsers = await _db.JobApplications.AsNoTracking()
            .Where(a => a.Id == request.JobApplicationId)
            .Select(a => new { a.CandidateProfile.UserId, EmployerUserId = a.JobPost.EmployerProfile.UserId })
            .FirstOrDefaultAsync(ct);

        if (appUsers is null) throw new NotFoundException("Application not found");
        if (appUsers.UserId != userId && appUsers.EmployerUserId != userId) throw new ForbiddenException();

        var pageSize = request.PageSize is < 1 or > 100 ? 50 : request.PageSize;
        var convId = request.JobApplicationId;

        var q = _db.Messages.AsNoTracking()
            .Where(m => m.ConversationId == convId);

        
        if (request.CursorSentAt.HasValue && request.CursorId.HasValue)
        {
            var t = request.CursorSentAt.Value;
            var id = request.CursorId.Value;

            q = q.Where(m => m.SentAt < t || (m.SentAt == t && m.Id.CompareTo(id) < 0));
        }

        var items = await q
            .OrderByDescending(m => m.SentAt).ThenByDescending(m => m.Id)
            .Take(pageSize)
            .Select(m => new MessageDto(m.Id, m.ConversationId, m.SenderUserId, m.Content, m.SentAt))
            .ToListAsync(ct);

        DateTimeOffset? nextT = null;
        Guid? nextId = null;

        if (items.Count > 0)
        {
            var last = items[^1]; // item cuối là cũ nhất trong batch
            nextT = last.SentAt;
            nextId = last.Id;
        }

        return new CursorPagedResult<MessageDto>(items, nextT, nextId);
    }
}
