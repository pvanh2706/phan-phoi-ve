using System.ComponentModel.DataAnnotations;

namespace PpvRecon.Application.Agencies;

public sealed class AgencyDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentCode { get; set; }
    public string? ParentName { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class SaveAgencyRequest
{
    [Required(ErrorMessage = "Mã đại lý là bắt buộc.")]
    [MaxLength(50, ErrorMessage = "Mã đại lý không được vượt quá 50 ký tự.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên đại lý là bắt buộc.")]
    [MaxLength(300, ErrorMessage = "Tên đại lý không được vượt quá 300 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "Mã đại lý mua cấp trên không được vượt quá 50 ký tự.")]
    public string? ParentCode { get; set; }

    [MaxLength(300, ErrorMessage = "Tên đại lý mua cấp trên không được vượt quá 300 ký tự.")]
    public string? ParentName { get; set; }

    [Required(ErrorMessage = "Nguồn dữ liệu là bắt buộc.")]
    [MaxLength(50, ErrorMessage = "Nguồn dữ liệu không được vượt quá 50 ký tự.")]
    public string Source { get; set; } = string.Empty;
}

public sealed class CreateAgencyRequest : SaveAgencyRequest
{
}

public sealed class UpdateAgencyRequest : SaveAgencyRequest
{
}
