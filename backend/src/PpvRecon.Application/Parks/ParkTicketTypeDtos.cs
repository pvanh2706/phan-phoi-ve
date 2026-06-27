using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Parks;

public sealed class ParkTicketTypeDto
{
    public int Id { get; set; }
    public int ParkId { get; set; }
    public string ParkCode { get; set; } = string.Empty;
    public string ParkName { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public string Code { get; set; } = string.Empty;
    public string TicketTypeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TicketGroupName { get; set; }
    public long CostPrice { get; set; }
    public RecordStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class CreateParkTicketTypeRequest
{
    public int ParkId { get; set; }

    [Required(ErrorMessage = "Mã KVC con là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Mã KVC con không được vượt quá 100 ký tự.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã loại vé là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Mã loại vé không được vượt quá 100 ký tự.")]
    public string TicketTypeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên loại vé là bắt buộc.")]
    [MaxLength(500, ErrorMessage = "Tên loại vé không được vượt quá 500 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TicketGroupName { get; set; }

    public long CostPrice { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;
}

public sealed class UpdateParkTicketTypeRequest
{
    public int ParkId { get; set; }

    [Required(ErrorMessage = "Mã KVC con là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Mã KVC con không được vượt quá 100 ký tự.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã loại vé là bắt buộc.")]
    [MaxLength(100, ErrorMessage = "Mã loại vé không được vượt quá 100 ký tự.")]
    public string TicketTypeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên loại vé là bắt buộc.")]
    [MaxLength(500, ErrorMessage = "Tên loại vé không được vượt quá 500 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TicketGroupName { get; set; }

    public long CostPrice { get; set; }
    public RecordStatus Status { get; set; }
}
