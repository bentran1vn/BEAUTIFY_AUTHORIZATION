namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Voucher : AggregateRoot<Guid>, IAuditableEntity
{
    public string Code { get; set; }
    public Guid? ClinicVoucherId { get; set; }
    public virtual ClinicVoucher? ClinicVoucher { get; set; }
    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}