using System.Security.Claims;
using JobPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Api.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    private readonly AppDbContext _db;

    public ChatHub(AppDbContext db) => _db = db;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        await base.OnConnectedAsync();
    }

    public Task JoinApplicationChat(Guid applicationId) => JoinInternal(applicationId);

    public Task LeaveApplicationChat(Guid applicationId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, AppGroup(applicationId));

    private async Task JoinInternal(Guid applicationId)
    {
        var userId = GetUserId();

        // validate membership theo application (1 query projection)
        var ok = await _db.JobApplications.AsNoTracking()
            .Where(a => a.Id == applicationId)
            .Select(a => new { CandidateUserId = a.CandidateProfile.UserId, EmployerUserId = a.JobPost.EmployerProfile.UserId })
            .AnyAsync(x => x.CandidateUserId == userId || x.EmployerUserId == userId);

        if (!ok) throw new HubException("Forbidden");

        await Groups.AddToGroupAsync(Context.ConnectionId, AppGroup(applicationId));
    }

    private Guid GetUserId()
    {
        var raw = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? Context.User?.FindFirstValue("sub");

        if (raw is null || !Guid.TryParse(raw, out var id))
            throw new HubException("Unauthorized");

        return id;
    }

    public static string AppGroup(Guid applicationId) => $"chat:app:{applicationId}";
    public static string UserGroup(Guid userId) => $"chat:user:{userId}";
}