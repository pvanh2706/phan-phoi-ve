using PpvRecon.Domain.Common;
using PpvRecon.Domain.Enums;

namespace PpvRecon.Domain.Entities.Parks;

public sealed class Park : IAuditableEntity, ISoftDelete
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
    public DateTime CreatedAtUtc { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public int? UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public int? DeletedByUserId { get; set; }
}
