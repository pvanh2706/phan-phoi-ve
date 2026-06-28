using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PpvRecon.Domain.Entities.Parks;

namespace PpvRecon.Api.Services;

public sealed class ParkBalanceApiOptions
{
    public string Endpoint { get; set; } = "http://api-ezcmt.ezticket.com.vn/gw/common/check-ar";
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class ParkBalanceApiResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ProfileId { get; init; }
    public long? AvailableBalance { get; init; }
    public string RequestUrl { get; init; } = string.Empty;
    public string RequestPayloadJson { get; init; } = string.Empty;
    public int? ResponseStatusCode { get; init; }
    public string? ResponseBodyJson { get; init; }
    public int DurationMs { get; init; }
}

public interface IParkBalanceApiClient
{
    Task<ParkBalanceApiResult> FetchAsync(Park park, DateOnly businessDate, CancellationToken cancellationToken);
}

public sealed class ParkBalanceApiClient(
    HttpClient httpClient,
    IOptions<ParkBalanceApiOptions> options) : IParkBalanceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<ParkBalanceApiResult> FetchAsync(
        Park park,
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        var endpoint = string.IsNullOrWhiteSpace(options.Value.Endpoint)
            ? "http://api-ezcmt.ezticket.com.vn/gw/common/check-ar"
            : options.Value.Endpoint.Trim();

        var payload = new
        {
            value = new
            {
                date = businessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                siteID = park.ApiSiteId,
                profileID = park.ApiProfileId,
            },
        };
        var payloadJson = JsonSerializer.Serialize(payload);

        if (string.IsNullOrWhiteSpace(park.ApiSiteId) || string.IsNullOrWhiteSpace(park.ApiProfileId))
        {
            return Failure(
                endpoint,
                payloadJson,
                null,
                null,
                0,
                "ParkApiIdentityMissing",
                "KVC chưa có ApiSiteId hoặc ApiProfileId.");
        }

        var stopwatch = Stopwatch.StartNew();
        string? responseBody = null;
        int? statusCode = null;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(payloadJson, Encoding.UTF8, "application/json"),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(request, cancellationToken);
            statusCode = (int)response.StatusCode;
            responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                return Failure(
                    endpoint,
                    payloadJson,
                    statusCode,
                    responseBody,
                    (int)stopwatch.ElapsedMilliseconds,
                    "HttpRequestFailed",
                    $"API trả về HTTP {(int)response.StatusCode}.");
            }

            var parsed = JsonSerializer.Deserialize<CheckArResponse>(responseBody, JsonOptions);
            if (!string.Equals(parsed?.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                return Failure(
                    endpoint,
                    payloadJson,
                    statusCode,
                    responseBody,
                    (int)stopwatch.ElapsedMilliseconds,
                    "ApiStatusNotSuccess",
                    "API không trả về trạng thái SUCCESS.");
            }

            if (parsed?.Value is null)
            {
                return Failure(
                    endpoint,
                    payloadJson,
                    statusCode,
                    responseBody,
                    (int)stopwatch.ElapsedMilliseconds,
                    "ApiValueMissing",
                    "API không trả về value.");
            }

            var value = parsed.Value;

            if (value.Balance is null)
            {
                return Failure(
                    endpoint,
                    payloadJson,
                    statusCode,
                    responseBody,
                    (int)stopwatch.ElapsedMilliseconds,
                    "BalanceMissing",
                    "API không trả về balance.");
            }

            return new ParkBalanceApiResult
            {
                IsSuccess = true,
                ProfileId = park.ApiProfileId,
                AvailableBalance = decimal.ToInt64(decimal.Round(value.Balance.Value, 0, MidpointRounding.AwayFromZero)),
                RequestUrl = endpoint,
                RequestPayloadJson = payloadJson,
                ResponseStatusCode = statusCode,
                ResponseBodyJson = responseBody,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            };
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return Failure(
                endpoint,
                payloadJson,
                statusCode,
                responseBody,
                (int)stopwatch.ElapsedMilliseconds,
                "Timeout",
                "Gọi API số dư KVC quá thời gian chờ.");
        }
        catch (JsonException)
        {
            stopwatch.Stop();
            return Failure(
                endpoint,
                payloadJson,
                statusCode,
                responseBody,
                (int)stopwatch.ElapsedMilliseconds,
                "InvalidJson",
                "Response API không phải JSON hợp lệ.");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return Failure(
                endpoint,
                payloadJson,
                statusCode,
                responseBody,
                (int)stopwatch.ElapsedMilliseconds,
                "HttpRequestError",
                ex.Message);
        }
    }

    private static ParkBalanceApiResult Failure(
        string endpoint,
        string payloadJson,
        int? statusCode,
        string? responseBody,
        int durationMs,
        string errorCode,
        string errorMessage)
    {
        return new ParkBalanceApiResult
        {
            IsSuccess = false,
            RequestUrl = endpoint,
            RequestPayloadJson = payloadJson,
            ResponseStatusCode = statusCode,
            ResponseBodyJson = responseBody,
            DurationMs = durationMs,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };
    }

    private sealed class CheckArResponse
    {
        public string? Status { get; set; }
        public CheckArValue? Value { get; set; }
    }

    private sealed class CheckArValue
    {
        public string? ProfileID { get; set; }
        public decimal? Balance { get; set; }
    }
}
