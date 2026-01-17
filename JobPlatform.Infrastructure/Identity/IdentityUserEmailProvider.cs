using JobPlatform.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace JobPlatform.Infrastructure.Identity;

public sealed class IdentityUserEmailProvider<TUser> : IUserEmailProvider
    where TUser : class
{
    private readonly UserManager<TUser> _userManager;

    public IdentityUserEmailProvider(UserManager<TUser> userManager)
        => _userManager = userManager;

    public async Task<string?> GetEmailAsync(Guid userId, CancellationToken ct)
    {
        
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await _userManager.GetEmailAsync(user);
    }
}