using IndiamojoBackend.BuildingBlocks.Domain.Common;

namespace IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

public sealed class User : BaseEntity
{
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public List<RefreshToken> RefreshTokens { get; private set; } = [];

    private User() { }

    public User(string fullName, string email, string passwordHash, UserRole role)
    {
        FullName = fullName;
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
    }

    public void UpdatePassword(string passwordHash) => PasswordHash = passwordHash;
}
