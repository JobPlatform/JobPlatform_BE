using System.Security.Claims;

namespace JobPlatform.Application.Common.Auth;

public record TokenSubjectDto(
    Guid UserId,
    string? Email,
    string UserName,
    string[] Roles,
    IReadOnlyCollection<Claim>? ExtraClaims = null
);