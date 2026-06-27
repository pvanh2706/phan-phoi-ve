namespace PpvRecon.Application.Common;

public sealed class ApiError
{
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;
}
