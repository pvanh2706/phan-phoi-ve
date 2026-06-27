using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using PpvRecon.Application.Auditing;
using PpvRecon.Domain.Entities.Auditing;
using PpvRecon.Infrastructure.Persistence;

namespace PpvRecon.Api.Services;

public sealed class AuditService(
    PpvReconDbContext dbContext,
    IHttpContextAccessor httpContextAccessor) : IAuditService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
    };

    private static readonly string[] SensitiveNameParts =
    [
        "password",
        "token",
        "secret",
        "hash",
        "signingkey",
        "smtpPassword",
    ];

    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var user = httpContext?.User;
        var userId = entry.UserId ?? GetUserId(user);

        var auditLog = new AuditLog
        {
            OccurredAtUtc = DateTime.UtcNow,
            UserId = userId,
            UserEmailSnapshot = user?.FindFirstValue(ClaimTypes.Email),
            UserRoleSnapshot = user?.FindFirstValue(ClaimTypes.Role),
            Module = entry.Module,
            EntityName = entry.EntityName,
            EntityId = entry.EntityId,
            Action = entry.Action,
            BeforeJson = SerializeSafe(entry.Before),
            AfterJson = SerializeSafe(entry.After),
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            CorrelationId = httpContext?.TraceIdentifier,
        };

        dbContext.AuditLogs.Add(auditLog);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static int? GetUserId(ClaimsPrincipal? user)
    {
        var value = user?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static string? SerializeSafe(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var node = JsonSerializer.SerializeToNode(value, JsonOptions);
        if (node is null)
        {
            return null;
        }

        Sanitize(node);
        return node.ToJsonString(JsonOptions);
    }

    private static void Sanitize(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            var namesToRemove = jsonObject
                .Select(property => property.Key)
                .Where(IsSensitiveName)
                .ToList();

            foreach (var name in namesToRemove)
            {
                jsonObject.Remove(name);
            }

            foreach (var property in jsonObject.ToList())
            {
                if (property.Value is not null)
                {
                    Sanitize(property.Value);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    Sanitize(item);
                }
            }
        }
    }

    private static bool IsSensitiveName(string name)
    {
        var normalized = name.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        return SensitiveNameParts.Any(part => normalized.Contains(part.ToLowerInvariant(), StringComparison.Ordinal));
    }
}
