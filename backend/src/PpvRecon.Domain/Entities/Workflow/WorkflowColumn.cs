using PpvRecon.Domain.Common;

namespace PpvRecon.Domain.Entities.Workflow;

/// <summary>
/// Một cột (bước) trong bảng Kanban "Quy trình nạp tiền KVC".
/// 5 cột cố định, seed sẵn; người dùng chỉ cấu hình trường hiển thị + phân quyền.
/// </summary>
public sealed class WorkflowColumn : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>Mã slug ổn định của cột (vd "lap-phieu").</summary>
    public string ColumnKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Màu header cột: gray | sky | amber | indigo | green | red.</summary>
    public string HeadTone { get; set; } = "gray";

    /// <summary>Nhãn trạng thái mặc định cho card thuộc cột (badge trên card).</summary>
    public string CardStatusLabel { get; set; } = string.Empty;

    /// <summary>Màu badge card: gray | blue | indigo | green | red | amber.</summary>
    public string CardTone { get; set; } = "gray";

    public int SortOrder { get; set; }

    /// <summary>CSV các id trường hiển thị trên card (vd "title,desc,amount,date,tag").</summary>
    public string VisibleFields { get; set; } = string.Empty;

    /// <summary>CSV các UserId được phép chuyển task khỏi cột này.</summary>
    public string PermittedUserIds { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
