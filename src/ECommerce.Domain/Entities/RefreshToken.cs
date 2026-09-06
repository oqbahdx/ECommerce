using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public User User { get; set; } = null!;
}