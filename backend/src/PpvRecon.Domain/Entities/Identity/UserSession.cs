namespace PpvRecon.Domain.Entities.Identity;

public sealed class UserSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string? JwtId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceName { get; set; }
    public DateTime? LastUsedAtUtc { get; set; }
    public string? LastUsedIp { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public int? RevokedByUserId { get; set; }
    public string? RevokeReason { get; set; }
    public int? ReplacedBySessionId { get; set; }
}
