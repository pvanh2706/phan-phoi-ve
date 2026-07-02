using System.Text.Json;
using System.Text.Json.Serialization;

namespace PpvRecon.Api.Json;

/// <summary>
/// Mọi DateTime trong hệ thống đều là UTC, nhưng SQLite trả về Kind = Unspecified
/// khiến JSON thiếu hậu tố "Z" và client hiểu nhầm là giờ địa phương.
/// Converter này ép Kind = Utc khi serialize/deserialize để client tự quy đổi múi giờ.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
        writer.WriteStringValue(utc);
    }
}
