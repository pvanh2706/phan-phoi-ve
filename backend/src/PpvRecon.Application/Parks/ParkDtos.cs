using System.ComponentModel.DataAnnotations;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Application.Parks;

public sealed class ParkDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ParkPaymentType PaymentType { get; set; }
    public string? SearchCode { get; set; }
    public string? Location { get; set; }
    public string? BankAccount { get; set; }
    public string? BankName { get; set; }
    public long? CreditLimit { get; set; }
    public string? ApiSiteId { get; set; }
    public string? ApiProfileId { get; set; }
    public string? BalanceTransformRule { get; set; }
    public string? ApiNote { get; set; }
    public RecordStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class CreateParkRequest
{
    [Required(ErrorMessage = "Mã KVC là bắt buộc.")]
    [MaxLength(50, ErrorMessage = "Mã KVC không được vượt quá 50 ký tự.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên khu vui chơi là bắt buộc.")]
    [MaxLength(300, ErrorMessage = "Tên khu vui chơi không được vượt quá 300 ký tự.")]
    public string Name { get; set; } = string.Empty;

    public ParkPaymentType PaymentType { get; set; } = ParkPaymentType.Prepaid;

    [MaxLength(100)]
    public string? SearchCode { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? BankAccount { get; set; }

    [MaxLength(200)]
    public string? BankName { get; set; }

    public long? CreditLimit { get; set; }

    [MaxLength(100)]
    public string? ApiSiteId { get; set; }

    [MaxLength(100)]
    public string? ApiProfileId { get; set; }

    [MaxLength(100)]
    public string? BalanceTransformRule { get; set; }

    [MaxLength(1000)]
    public string? ApiNote { get; set; }

    public RecordStatus Status { get; set; } = RecordStatus.Active;
}

public sealed class UpdateParkRequest
{
    [Required(ErrorMessage = "Tên khu vui chơi là bắt buộc.")]
    [MaxLength(300, ErrorMessage = "Tên khu vui chơi không được vượt quá 300 ký tự.")]
    public string Name { get; set; } = string.Empty;

    public ParkPaymentType PaymentType { get; set; }

    [MaxLength(100)]
    public string? SearchCode { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? BankAccount { get; set; }

    [MaxLength(200)]
    public string? BankName { get; set; }

    public long? CreditLimit { get; set; }

    [MaxLength(100)]
    public string? ApiSiteId { get; set; }

    [MaxLength(100)]
    public string? ApiProfileId { get; set; }

    [MaxLength(100)]
    public string? BalanceTransformRule { get; set; }

    [MaxLength(1000)]
    public string? ApiNote { get; set; }

    public RecordStatus Status { get; set; }
}
