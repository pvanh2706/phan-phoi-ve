using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Workflow;

public sealed class WorkflowBoardDto
{
    public List<WorkflowColumnDto> Columns { get; set; } = new();
    public List<WorkflowUserOptionDto> Users { get; set; } = new();
}

public sealed class WorkflowUserOptionDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public sealed class WorkflowColumnDto
{
    public int Id { get; set; }
    public string ColumnKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string HeadTone { get; set; } = "gray";
    public string CardStatusLabel { get; set; } = string.Empty;
    public string CardTone { get; set; } = "gray";
    public int SortOrder { get; set; }
    public List<string> VisibleFields { get; set; } = new();
    public List<int> PermittedUserIds { get; set; } = new();
    public List<WorkflowTaskDto> Tasks { get; set; } = new();
}

public sealed class WorkflowTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public int? ParkId { get; set; }
    public string? ParkName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public long Amount { get; set; }
    public DateOnly? ExecuteDate { get; set; }
    public string? Note { get; set; }
    public int ColumnId { get; set; }
    public int SortOrder { get; set; }
}

public sealed class CreateWorkflowTaskRequest
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public ParkPaymentType PaymentType { get; set; }
    public int? ParkId { get; set; }

    [MaxLength(100)]
    public string? BankAccount { get; set; }

    [MaxLength(200)]
    public string? BankName { get; set; }

    public long Amount { get; set; }
    public DateOnly? ExecuteDate { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }

    /// <summary>Cột khởi tạo; nếu null sẽ đặt vào cột đầu tiên.</summary>
    public int? ColumnId { get; set; }
}

public sealed class UpdateWorkflowTaskRequest
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    public ParkPaymentType PaymentType { get; set; }
    public int? ParkId { get; set; }

    [MaxLength(100)]
    public string? BankAccount { get; set; }

    [MaxLength(200)]
    public string? BankName { get; set; }

    public long Amount { get; set; }
    public DateOnly? ExecuteDate { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }
}

public sealed class MoveWorkflowTaskRequest
{
    public int ColumnId { get; set; }
}

public sealed class UpdateWorkflowColumnSettingsRequest
{
    public List<string> VisibleFields { get; set; } = new();
    public List<int> PermittedUserIds { get; set; } = new();
}
