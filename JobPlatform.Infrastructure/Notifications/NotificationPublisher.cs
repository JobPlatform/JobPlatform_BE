using System.Text.Json;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Application.Common.Models;
using JobPlatform.Domain.Entities;
using JobPlatform.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobPlatform.Infrastructure.Notifications;

public sealed class NotificationPublisher : INotificationPublisher
{
    private readonly IAppDbContext _db;
    private readonly IUserEmailProvider _emailProvider;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationPublisher> _logger;
    private readonly EmailSettings _settings;

    
    public NotificationPublisher(
        IAppDbContext db,
        IUserEmailProvider emailProvider,
        IEmailSender emailSender,
        IOptions<EmailSettings> options,
        ILogger<NotificationPublisher> logger)
    {
        _db = db;
        _emailProvider = emailProvider;
        _emailSender = emailSender;
        _logger = logger;
        _settings = options.Value;
    }
    


    public async Task NotifyAsync(
        Guid userId,
        string type,
        string title,
        string body,
        object? data,
        string? targetUrl,
        bool sendEmail,
        CancellationToken ct)
    {
        var noti = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            TargetUrl = string.IsNullOrWhiteSpace(targetUrl) ? null : targetUrl.Trim(),
            DataJson = data is null ? null : JsonSerializer.Serialize(data),
            IsRead = false
        };

        _db.Notifications.Add(noti);
        await _db.SaveChangesAsync(ct);

        if (!sendEmail) return;

        var to = await _emailProvider.GetEmailAsync(userId, ct);
        if (string.IsNullOrWhiteSpace(to)) return;

        var now = DateTimeOffset.UtcNow;
        var startOfDay = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

        var sentToday = await _db.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == userId && n.EmailSentAt != null && n.EmailSentAt >= startOfDay, ct);
        
        if (sentToday >= _settings.DailyLimitPerUser)
        {
            // quá hạn: không gửi email, vẫn lưu notification
            _logger.LogInformation("Email throttled for user {UserId}. SentToday={SentToday}", userId, sentToday);
            return;
        }

        try
        {
            await _emailSender.SendAsync(to, title, body, ct);

            // mark email sent
            var justSaved = await _db.Notifications.FirstAsync(x => x.Id == noti.Id, ct);
            justSaved.EmailSentAt = now;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            
            _logger.LogWarning(ex, "Failed to send email to {Email} for notification {Type}", to, type);
        }
    }
}