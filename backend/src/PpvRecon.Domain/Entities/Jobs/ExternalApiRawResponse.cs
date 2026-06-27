using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Jobs;

public sealed class ExternalApiRawResponse
{
    public int Id { get; set; }
    public ExternalApiSource Source { get; set; }
    public DateOnly? BusinessDate { get; set; }
    public int? ParkId { get; set; }
    public int? JobRunId { get; set; }
    public int? JobRunItemId { get; set; }
    public string? RequestUrl { get; set; }
    public string? RequestPayloadJson { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBodyJson { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? DurationMs { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
}
