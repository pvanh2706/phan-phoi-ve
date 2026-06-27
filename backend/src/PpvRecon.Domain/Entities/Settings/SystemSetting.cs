using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Settings;

public sealed class SystemSetting
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SettingValueType ValueType { get; set; }
    public string? Description { get; set; }
    public bool IsSensitive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
