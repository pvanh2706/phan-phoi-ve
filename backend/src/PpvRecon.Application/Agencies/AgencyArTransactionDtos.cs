namespace PpvRecon.Application.Agencies;

/// <summary>Một dòng giao dịch đại lý trên AR trả về cho giao diện.</summary>
public sealed class AgencyArTransactionDto
{
    public int Id { get; set; }
    public string BookingId { get; set; } = string.Empty;
    public string? TravelAgentName { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public long Amount { get; set; }
    public string? TravelAgentCode { get; set; }
    public string? ReceivableAccountCode { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }

    /// <summary>Ngày đồng bộ = thời điểm hệ thống lưu/cập nhật bản ghi = UpdatedAtUtc ?? CreatedAtUtc.</summary>
    public DateTime SyncedAtUtc { get; set; }
}

/// <summary>Kết quả trang danh sách kèm tổng số tiền (§14).</summary>
public sealed class AgencyArTransactionListResult
{
    public IReadOnlyList<AgencyArTransactionDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    /// <summary>Tổng "Số tiền" trên toàn bộ tập kết quả đã lọc.</summary>
    public long TotalAmount { get; set; }
}

/// <summary>
/// Kết quả một lần đồng bộ "Giao dịch đại lý trên AR" từ file Excel của AR — dùng cho log tổng hợp (§16)
/// và thông báo UI. Các con số phản ánh đúng các bước xử lý file.
/// </summary>
public sealed class AgencyArTransactionSyncResult
{
    public DateOnly BusinessDate { get; set; }

    /// <summary>Tổng số dòng có dữ liệu trong file Excel.</summary>
    public int TotalRows { get; set; }

    /// <summary>Số dòng bỏ qua ở đầu file (tiêu đề/thông tin báo cáo).</summary>
    public int SkippedHeaderRows { get; set; }

    /// <summary>Số dòng (sau khi bỏ đầu file) có nội dung mô tả tại cột __EMPTY_7.</summary>
    public int RowsWithDescription { get; set; }

    /// <summary>Số giao dịch "Thanh toán tiền cho booking" hợp lệ (trích được bookingId + ngày + tiền).</summary>
    public int ValidBookingTransactions { get; set; }

    /// <summary>Số dòng có mô tả nhưng không phải "Thanh toán tiền cho booking" → bỏ qua.</summary>
    public int SkippedNonBookingRows { get; set; }

    /// <summary>Số dòng khớp loại giao dịch nhưng lỗi (thiếu bookingId, ngày/tiền không hợp lệ) → bỏ qua.</summary>
    public int ErrorRows { get; set; }

    public int Inserted { get; set; }
    public int Updated { get; set; }

    /// <summary>Số bản ghi đã tồn tại và không thay đổi.</summary>
    public int Unchanged { get; set; }

    /// <summary>Một số cảnh báo tiêu biểu (giới hạn số lượng) kèm số dòng để kiểm tra lại.</summary>
    public List<string> Warnings { get; set; } = new();
}
