using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Auditing;

public sealed class AuditLogEntry
{
    public int? UserId { get; set; }
    public string Module { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public AuditAction Action { get; set; }
    public object? Before { get; set; }
    public object? After { get; set; }
}

public sealed class AuditLogDto
{
    public int Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public int? UserId { get; set; }
    public string? UserEmailSnapshot { get; set; }
    public string? UserRoleSnapshot { get; set; }
    public string Module { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public AuditAction Action { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
}

public interface IAuditService
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
