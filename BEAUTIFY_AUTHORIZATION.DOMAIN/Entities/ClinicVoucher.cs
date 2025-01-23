namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class ClinicVoucher : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    public Guid ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    public string? Name { get; set; }
    public decimal MaximumAmountPercent { get; set; }
    public decimal MaximumDiscountAmount { get; set; }
    public int MaximumUsage { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; }
    public decimal MinimumAmount { get; set; }
    public bool IsActivated { get; set; } = false;
    public int? TotalUsage { get; set; }
    public Guid VoucherId { get; set; }
    public virtual Voucher? Voucher { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}