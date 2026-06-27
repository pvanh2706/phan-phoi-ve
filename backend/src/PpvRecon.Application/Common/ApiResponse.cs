namespace PpvRecon.Application.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<ApiError> Errors { get; init; } = [];

    public static ApiResponse<T> Ok(T? data, string message = "Thành công")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            Errors = [],
        };
    }

    public static ApiResponse<T> Fail(string message, IReadOnlyList<ApiError>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Errors = errors ?? [],
        };
    }
}
