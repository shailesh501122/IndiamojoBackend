using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

namespace IndiamojoBackend.BuildingBlocks.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public AuthResponse Generate(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, expires: expiresAt, signingCredentials: credentials);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), expiresAt, user.Id, user.Email, user.Role.ToString());
    }
}

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? UserId => Guid.TryParse(
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub),
        out var id) ? id : null;
    public string? Role => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
}
