namespace PpvRecon.Domain.Entities.Workflow;

/// <summary>
/// Cấu hình mặc định (factory) cho 5 cột cố định của bảng "Quy trình nạp tiền KVC".
/// Đây là DỮ LIỆU CẤU TRÚC, không phải dữ liệu nghiệp vụ: phải luôn tồn tại để
/// chức năng hoạt động. Dùng chung cho cả seed lúc khởi động lẫn khi reset dữ liệu,
/// đảm bảo chỉ có một nguồn sự thật trùng với bản seed trong migration AddWorkflowBoard.
/// </summary>
public static class WorkflowBoardDefaults
{
    /// <summary>Trường hiển thị mặc định trên card.</summary>
    public const string DefaultVisibleFields = "title,desc,amount,date,tag";

    /// <summary>Mốc thời gian seed cố định (khớp với migration) để dữ liệu tất định.</summary>
    public static readonly DateTime SeededAtUtc = new(2026, 6, 27, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>5 cột cố định theo đúng thứ tự quy trình.</summary>
    public static IReadOnlyList<WorkflowColumn> Columns { get; } =
    [
        new WorkflowColumn
        {
            Id = 1,
            ColumnKey = "lap-phieu",
            Title = "Kế toán / NVKD lập phiếu",
            HeadTone = "gray",
            CardStatusLabel = "Lập phiếu",
            CardTone = "gray",
            SortOrder = 1,
            VisibleFields = DefaultVisibleFields,
            PermittedUserIds = string.Empty,
            CreatedAtUtc = SeededAtUtc,
        },
        new WorkflowColumn
        {
            Id = 2,
            ColumnKey = "truong-bo-phan-duyet",
            Title = "Trưởng bộ phận duyệt",
            HeadTone = "sky",
            CardStatusLabel = "Chờ duyệt",
            CardTone = "blue",
            SortOrder = 2,
            VisibleFields = DefaultVisibleFields,
            PermittedUserIds = string.Empty,
            CreatedAtUtc = SeededAtUtc,
        },
        new WorkflowColumn
        {
            Id = 3,
            ColumnKey = "kiem-tra-chuyen-khoan",
            Title = "Kế toán kiểm tra & chuyển khoản",
            HeadTone = "indigo",
            CardStatusLabel = "Chuyển khoản",
            CardTone = "indigo",
            SortOrder = 3,
            VisibleFields = DefaultVisibleFields,
            PermittedUserIds = string.Empty,
            CreatedAtUtc = SeededAtUtc,
        },
        new WorkflowColumn
        {
            Id = 4,
            ColumnKey = "hoan-thanh",
            Title = "Hoàn thành",
            HeadTone = "green",
            CardStatusLabel = "Hoàn thành",
            CardTone = "green",
            SortOrder = 4,
            VisibleFields = DefaultVisibleFields,
            PermittedUserIds = string.Empty,
            CreatedAtUtc = SeededAtUtc,
        },
        new WorkflowColumn
        {
            Id = 5,
            ColumnKey = "that-bai",
            Title = "Thất bại",
            HeadTone = "red",
            CardStatusLabel = "Thất bại",
            CardTone = "red",
            SortOrder = 5,
            VisibleFields = DefaultVisibleFields,
            PermittedUserIds = string.Empty,
            CreatedAtUtc = SeededAtUtc,
        },
    ];
}
