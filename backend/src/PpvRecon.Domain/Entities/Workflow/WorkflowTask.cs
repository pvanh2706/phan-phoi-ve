using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Workflow;

/// <summary>
/// Một nhiệm vụ (card) trong bảng Kanban "Quy trình nạp tiền KVC".
/// </summary>
public sealed class WorkflowTask : IAuditableEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Loại: Prepaid (Nạp tiền) hoặc Debt (Công nợ).</summary>
    public ParkPaymentType PaymentType { get; set; }

    public int? ParkId { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public long Amount { get; set; }
    public DateOnly? ExecuteDate { get; set; }
    public string? Note { get; set; }

    /// <summary>Cột (bước) hiện tại của card.</summary>
    public int ColumnId { get; set; }

    /// <summary>Thứ tự trong cột.</summary>
    public int SortOrder { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
}
