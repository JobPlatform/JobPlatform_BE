namespace JobPlatform.Application.Common.Interfaces;

public interface IUserEmailProvider
{
    Task<string?> GetEmailAsync(Guid userId, CancellationToken ct);
}