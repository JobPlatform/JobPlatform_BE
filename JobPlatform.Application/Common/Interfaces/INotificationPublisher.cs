public interface INotificationPublisher
{
    Task NotifyAsync(
        Guid userId,
        string type,
        string title,
        string body,
        object? data,
        string? targetUrl, 
        bool sendEmail,
        CancellationToken ct);
}