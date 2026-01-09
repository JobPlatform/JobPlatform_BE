namespace JobPlatform.Application.Common.Auth;

public sealed record TokenSubjectDto(
    Guid UserId,
    string? Email,
    string UserName,
    IReadOnlyCollection<string> Roles
);