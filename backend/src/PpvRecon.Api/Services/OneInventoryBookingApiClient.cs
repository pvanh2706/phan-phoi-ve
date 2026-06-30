using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PpvRecon.Api.Services.Settings;

namespace PpvRecon.Api.Services;

public sealed class OneInventoryApiOptions
{
    /// <summary>Base URL của hệ thống Oneinventory, vd https://admin.oneinventory.com</summary>
    public string BaseUrl { get; set; } = "https://admin.oneinventory.com";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60;
}

/// <summary>Một dòng vé bán trả về từ procedure rp_booking_list (map 1:1 field JSON).</summary>
public sealed class OneInventoryBookingLine
{
    public string? GiamGia { get; set; }
    public string? TenDaiLyMuaCapTren { get; set; }
    public string? MaDatVe { get; set; }
    public string? DonGia { get; set; }
    public string? TenLoaiVe { get; set; }
    public string? TenNhomLoaiVe { get; set; }
    public string? TienBan { get; set; }
    public string? TienVon { get; set; }
    public string? MaDaiLyMuaCapTren { get; set; }
    public string? MaDaiLyBan { get; set; }
    public string? SoLuongVe { get; set; }
    public string? MaDaiLyMua { get; set; }
    public string? TenDaiLyMua { get; set; }
    public string? MaKhuVuiChoi { get; set; }
    public string? TenKhuVuiChoi { get; set; }
    public string? TamTinh { get; set; }
    public string? ID { get; set; }
    public string? NgayDat { get; set; }
    public string? TenDaiLyBan { get; set; }
    public string? MaLoaiVe { get; set; }
}

public sealed class OneInventoryBookingResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<OneInventoryBookingLine> Lines { get; init; } = Array.Empty<OneInventoryBookingLine>();
    public string RequestUrl { get; init; } = string.Empty;
    public int? ResponseStatusCode { get; init; }
    public int DurationMs { get; init; }
}

public interface IOneInventoryBookingApiClient
{
    /// <summary>Login lấy token rồi gọi rp_booking_list cho đúng 1 ngày businessDate.</summary>
    Task<OneInventoryBookingResult> FetchBookingsAsync(DateOnly businessDate, CancellationToken cancellationToken);
}

/// <summary>
/// Gọi API Oneinventory cho màn "Chi tiết giá vốn vé bán":
/// 1) POST /api/v1/admin/user/login → token (value.token)
/// 2) GET  /api/v1/admin/procedure?function=rp_booking_list&amp;startDate=...&amp;endDate=...
///         với header Authorization: Token &lt;token&gt; → mảng JSON các dòng vé.
/// </summary>
public sealed class OneInventoryBookingApiClient(
    HttpClient httpClient,
    IConnectionSettingsService connectionSettings) : IOneInventoryBookingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<OneInventoryBookingResult> FetchBookingsAsync(
        DateOnly businessDate,
        CancellationToken cancellationToken)
    {
        var opt = await connectionSettings.GetOneInventoryAsync(cancellationToken);
        var baseUrl = (string.IsNullOrWhiteSpace(opt.BaseUrl)
            ? "https://admin.oneinventory.com"
            : opt.BaseUrl).TrimEnd('/');

        if (string.IsNullOrWhiteSpace(opt.Username) || string.IsNullOrWhiteSpace(opt.Password))
        {
            return Failure(baseUrl, null, 0, "CredentialsMissing",
                "Chưa cấu hình username/password cho Oneinventory (ExternalApis:OneInventory).");
        }

        var stopwatch = Stopwatch.StartNew();

        // Timeout đọc từ cấu hình (áp dụng ngay), bao trùm cả login + gọi procedure.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(opt.TimeoutSeconds, 1, 300)));

        try
        {
            // 1) Login lấy token.
            var token = await LoginAsync(baseUrl, opt.Username, opt.Password, timeoutCts.Token);
            if (token is null)
            {
                stopwatch.Stop();
                return Failure(baseUrl, null, (int)stopwatch.ElapsedMilliseconds,
                    "LoginFailed", "Không lấy được token từ Oneinventory (sai tài khoản hoặc API đổi cấu trúc).");
            }

            // 2) Gọi procedure rp_booking_list cho đúng 1 ngày.
            var date = businessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var startDate = Uri.EscapeDataString($"{date} 00:00");
            var endDate = Uri.EscapeDataString($"{date} 23:59");
            var url = $"{baseUrl}/api/v1/admin/procedure?function=rp_booking_list&startDate={startDate}&endDate={endDate}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Authorization", $"Token {token}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await httpClient.SendAsync(request, timeoutCts.Token);
            var statusCode = (int)response.StatusCode;
            var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                return Failure(url, statusCode, (int)stopwatch.ElapsedMilliseconds,
                    "HttpRequestFailed", $"API trả về HTTP {statusCode}.");
            }

            var lines = JsonSerializer.Deserialize<List<OneInventoryBookingLine>>(body, JsonOptions)
                ?? new List<OneInventoryBookingLine>();

            return new OneInventoryBookingResult
            {
                IsSuccess = true,
                Lines = lines,
                RequestUrl = url,
                ResponseStatusCode = statusCode,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            };
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return Failure(baseUrl, null, (int)stopwatch.ElapsedMilliseconds,
                "Timeout", "Gọi API Oneinventory quá thời gian chờ.");
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            return Failure(baseUrl, null, (int)stopwatch.ElapsedMilliseconds,
                "InvalidJson", $"Response Oneinventory không phải JSON hợp lệ: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return Failure(baseUrl, null, (int)stopwatch.ElapsedMilliseconds,
                "HttpRequestError", ex.Message);
        }
    }

    private async Task<string?> LoginAsync(
        string baseUrl, string username, string password, CancellationToken cancellationToken)
    {
        var loginUrl = $"{baseUrl}/api/v1/admin/user/login";
        var payloadJson = JsonSerializer.Serialize(new { username, password });

        using var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
        {
            Content = new StringContent(payloadJson, Encoding.UTF8, "application/json"),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var parsed = JsonSerializer.Deserialize<LoginResponse>(body, JsonOptions);
        var token = parsed?.Value?.Token;
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    private static OneInventoryBookingResult Failure(
        string requestUrl, int? statusCode, int durationMs, string errorCode, string errorMessage)
        => new()
        {
            IsSuccess = false,
            RequestUrl = requestUrl,
            ResponseStatusCode = statusCode,
            DurationMs = durationMs,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
        };

    private sealed class LoginResponse
    {
        [JsonPropertyName("value")]
        public LoginValue? Value { get; set; }
    }

    private sealed class LoginValue
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}
