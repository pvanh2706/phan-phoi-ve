using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Jobs;

public class ExternalApiLogListItemDto
{
    public int Id { get; set; }
    public ExternalApiSource Source { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public int? ParkId { get; set; }
    public string? ParkName { get; set; }
    public int? JobRunId { get; set; }
    public string? RequestUrl { get; set; }
    public int? ResponseStatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? DurationMs { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
}

public sealed class ExternalApiLogDetailDto : ExternalApiLogListItemDto
{
    public string? RequestPayloadJson { get; set; }
    public string? ResponseBodyJson { get; set; }
}
