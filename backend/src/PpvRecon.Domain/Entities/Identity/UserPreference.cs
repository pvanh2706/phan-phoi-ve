using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Identity;

public sealed class UserPreference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ThemeMode ThemeMode { get; set; }
    public string? Language { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
