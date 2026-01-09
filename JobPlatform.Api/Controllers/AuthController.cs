using System.Security.Claims;
using JobPlatform.Application.Common.Auth;
using JobPlatform.Application.Common.Interfaces;
using JobPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobPlatform.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    public record RegisterRequest(string Email, string Password, string Role); // Employer/Candidate
    public record LoginRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        if (req.Role is not ("Employer" or "Candidate"))
            return BadRequest(new { message = "Role must be Employer or Candidate" });

        // ensure role exists
        if (!await _roleManager.RoleExistsAsync(req.Role))
        {
            var createRole = await _roleManager.CreateAsync(new IdentityRole<Guid>(req.Role));
            if (!createRole.Succeeded) return BadRequest(createRole.Errors);
        }

        var user = new ApplicationUser { UserName = req.Email, Email = req.Email };
        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        var addRole = await _userManager.AddToRoleAsync(user, req.Role);
        if (!addRole.Succeeded) return BadRequest(addRole.Errors);

        var subject = await BuildSubjectAsync(user);
        var tokens = await _tokenService.CreateTokensAsync(subject, ct);

        return Ok(new { accessToken = tokens.accessToken, refreshToken = tokens.refreshToken });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(req.Email);
        if (user is null) return Unauthorized();

        var ok = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!ok) return Unauthorized();

        var subject = await BuildSubjectAsync(user);
        var tokens = await _tokenService.CreateTokensAsync(subject, ct);

        return Ok(new { accessToken = tokens.accessToken, refreshToken = tokens.refreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var result = await _tokenService.RefreshAsync(req.RefreshToken, ct);
        if (result is null) return Unauthorized();

        return Ok(new { accessToken = result.Value.accessToken, refreshToken = result.Value.refreshToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
    {
        await _tokenService.RevokeAsync(req.RefreshToken, ct);
        return NoContent();
    }

    private async Task<TokenSubjectDto> BuildSubjectAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var extraClaims = new List<Claim>
        {
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? ""),
            new Claim("emailConfirmed", user.EmailConfirmed ? "true" : "false"),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("phoneConfirmed", user.PhoneNumberConfirmed ? "true" : "false"),
        };

        
        
        return  new TokenSubjectDto(
            UserId: user.Id,
            Email: user.Email,
            UserName: user.UserName ?? user.Email ?? user.Id.ToString(),
            Roles: roles.ToArray(),
            ExtraClaims: extraClaims
        );
    }
}
