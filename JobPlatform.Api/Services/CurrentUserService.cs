using System.Security.Claims;
using JobPlatform.Application.Common.Interfaces;

namespace JobPlatform.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;
    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var sub = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name)
                      ?? _accessor.HttpContext?.User?.FindFirstValue("sub");

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }
}