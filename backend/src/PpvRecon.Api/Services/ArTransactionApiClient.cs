using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PpvRecon.Api.Services.Settings;

namespace PpvRecon.Api.Services;

/// <summary>Cấu hình kết nối hệ thống AR (ar.ezcloud.vn). Không hard-code tài khoản/mật khẩu trong source (§17).</summary>
public sealed class ArApiOptions
{
    public string LoginUrl { get; set; } = "https://ar.ezcloud.vn/api/account-login/Process";
    public string DataUrl { get; set; } = "https://ar.ezcloud.vn/api/AR_TATR01/Process";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Lang { get; set; } = "1000000";
    public string TimeZone { get; set; } = "Asia/Ho_Chi_Minh";
    public int TimeoutSeconds { get; set; } = 60;
    public int RetryCount { get; set; } = 2;
    public int RetryDelaySeconds { get; set; } = 5;
}

/// <summary>Kết quả gọi API AR: file Excel (đã decode base64) hoặc thông tin lỗi.</summary>
public sealed class ArTransactionApiResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public byte[]? FileContents { get; init; }
    public string? FileDownloadName { get; init; }
    public string? ContentType { get; init; }
    public string RequestUrl { get; init; } = string.Empty;
    public int? ResponseStatusCode { get; init; }
    public int DurationMs { get; init; }
}

public interface IArTransactionApiClient
{
    /// <summary>Đăng nhập AR lấy token rồi gọi AR_TATR01 cho đúng 1 ngày, trả file Excel đã decode.</summary>
    Task<ArTransactionApiResult> FetchTransactionsAsync(DateOnly businessDate, CancellationToken cancellationToken);
}

/// <summary>
/// Gọi API AR cho màn "Giao dịch của các đại lý trên AR":
/// 1) POST account-login/Process → token (Data.Token) khi Status = "200".
/// 2) POST AR_TATR01/Process với FromDate/ToDate của ngày cần đồng bộ (giờ VN) và token trong body →
///    file Excel base64 (Data.FileContents). Có retry cho lỗi mạng/tạm thời theo cấu hình.
/// Không ghi token, mật khẩu hay toàn bộ base64 vào log.
/// </summary>
public sealed class ArTransactionApiClient(
    HttpClient httpClient,
    IConnectionSettingsService connectionSettings) : IArTransactionApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ArTransactionApiResult> FetchTransactionsAsync(
        DateOnly businessDate, CancellationToken cancellationToken)
    {
        var opt = await connectionSettings.GetArAsync(cancellationToken);
        var loginUrl = opt.LoginUrl?.Trim() ?? string.Empty;
        var dataUrl = opt.DataUrl?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(opt.Username) || string.IsNullOrWhiteSpace(opt.Password))
        {
            return Failure(loginUrl, null, 0, "CredentialsMissing",
                "Chưa cấu hình username/password cho AR (ExternalApis:Ar).");
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // 1) Đăng nhập lấy token.
            var (loginStatus, loginBody) = await PostWithRetryAsync(loginUrl, new
            {
                UserName = opt.Username,
                Password = opt.Password,
            }, opt, cancellationToken);

            if (loginStatus is < 200 or >= 300)
            {
                return Failure(loginUrl, loginStatus, (int)stopwatch.ElapsedMilliseconds,
                    "LoginHttpError", $"API đăng nhập AR trả về HTTP {loginStatus}.");
            }

            var login = JsonSerializer.Deserialize<ArResponse<LoginData>>(loginBody, JsonOptions);
            var token = login?.Data?.Token;
            if (login?.Status != "200" || string.IsNullOrWhiteSpace(token))
            {
                return Failure(loginUrl, loginStatus, (int)stopwatch.ElapsedMilliseconds,
                    "LoginFailed", "Đăng nhập AR thất bại (sai tài khoản hoặc không lấy được token).");
            }

            // 2) Lấy dữ liệu giao dịch cho đúng 1 ngày (giờ VN).
            var date = businessDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var (dataStatus, dataBody) = await PostWithRetryAsync(dataUrl, new
            {
                TravelAgentId = Array.Empty<string>(),
                FromDate = $"{date}T00:00:00",
                ToDate = $"{date}T23:59:59",
                Lang = string.IsNullOrWhiteSpace(opt.Lang) ? "1000000" : opt.Lang,
                Token = token,
            }, opt, cancellationToken);

            stopwatch.Stop();

            if (dataStatus is < 200 or >= 300)
            {
                return Failure(dataUrl, dataStatus, (int)stopwatch.ElapsedMilliseconds,
                    "DataHttpError", $"API lấy dữ liệu AR trả về HTTP {dataStatus}.");
            }

            var data = JsonSerializer.Deserialize<ArResponse<FileData>>(dataBody, JsonOptions);
            if (data?.Status != "200" || data.Data is null)
            {
                return Failure(dataUrl, dataStatus, (int)stopwatch.ElapsedMilliseconds,
                    "DataStatusError", $"API lấy dữ liệu AR trả về Status='{data?.Status}'.");
            }

            if (string.IsNullOrWhiteSpace(data.Data.FileContents))
            {
                return Failure(dataUrl, dataStatus, (int)stopwatch.ElapsedMilliseconds,
                    "EmptyFile", "AR không trả về nội dung file (FileContents rỗng).");
            }

            if (!IsExcel(data.Data.ContentType, data.Data.FileDownloadName))
            {
                return Failure(dataUrl, dataStatus, (int)stopwatch.ElapsedMilliseconds,
                    "InvalidContentType", $"File AR trả về không phải Excel (ContentType='{data.Data.ContentType}').");
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(data.Data.FileContents.Trim());
            }
            catch (FormatException)
            {
                return Failure(dataUrl, dataStatus, (int)stopwatch.ElapsedMilliseconds,
                    "InvalidBase64", "FileContents không phải chuỗi base64 hợp lệ.");
            }

            return new ArTransactionApiResult
            {
                IsSuccess = true,
                FileContents = bytes,
                FileDownloadName = data.Data.FileDownloadName,
                ContentType = data.Data.ContentType,
                RequestUrl = dataUrl,
                ResponseStatusCode = dataStatus,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            };
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return Failure(dataUrl, null, (int)stopwatch.ElapsedMilliseconds, "Timeout", "Gọi API AR quá thời gian chờ.");
        }
        catch (JsonException ex)
        {
            stopwatch.Stop();
            return Failure(dataUrl, null, (int)stopwatch.ElapsedMilliseconds, "InvalidJson", $"Response AR không phải JSON hợp lệ: {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            return Failure(dataUrl, null, (int)stopwatch.ElapsedMilliseconds, "HttpRequestError", ex.Message);
        }
    }

    /// <summary>
    /// POST JSON có retry cho lỗi tạm thời (timeout, lỗi mạng, HTTP 5xx). Không retry với 4xx/nội dung lỗi
    /// vì đó không phải lỗi tạm thời. Mỗi lần thử có timeout riêng theo cấu hình.
    /// </summary>
    private async Task<(int Status, string Body)> PostWithRetryAsync(
        string url, object payload, ArApiOptions opt, CancellationToken cancellationToken)
    {
        var attempts = Math.Clamp(opt.RetryCount, 0, 10) + 1;
        var delay = TimeSpan.FromSeconds(Math.Clamp(opt.RetryDelaySeconds, 0, 120));
        var timeout = TimeSpan.FromSeconds(Math.Clamp(opt.TimeoutSeconds, 1, 300));
        var payloadJson = JsonSerializer.Serialize(payload);

        for (var attempt = 1; ; attempt++)
        {
            var isLast = attempt >= attempts;
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(payloadJson, Encoding.UTF8, "application/json"),
                };
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await httpClient.SendAsync(request, timeoutCts.Token);
                var status = (int)response.StatusCode;
                var body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

                if (status >= 500 && !isLast)
                {
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }
                return (status, body);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested && !isLast)
            {
                await Task.Delay(delay, cancellationToken);
            }
            catch (HttpRequestException) when (!isLast)
            {
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static bool IsExcel(string? contentType, string? fileName)
    {
        var ct = contentType?.ToLowerInvariant() ?? string.Empty;
        var name = fileName?.ToLowerInvariant() ?? string.Empty;
        return ct.Contains("spreadsheet")
            || ct.Contains("excel")
            || ct.Contains("ms-excel")
            || name.EndsWith(".xlsx", StringComparison.Ordinal)
            || name.EndsWith(".xls", StringComparison.Ordinal);
    }

    private static ArTransactionApiResult Failure(
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

    private sealed class ArResponse<T>
    {
        [JsonPropertyName("Status")] public string? Status { get; set; }
        [JsonPropertyName("Data")] public T? Data { get; set; }
    }

    private sealed class LoginData
    {
        [JsonPropertyName("Token")] public string? Token { get; set; }
    }

    private sealed class FileData
    {
        [JsonPropertyName("FileContents")] public string? FileContents { get; set; }
        [JsonPropertyName("ContentType")] public string? ContentType { get; set; }
        [JsonPropertyName("FileDownloadName")] public string? FileDownloadName { get; set; }
    }
}
