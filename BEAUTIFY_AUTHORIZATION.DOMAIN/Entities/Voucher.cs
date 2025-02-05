using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Voucher : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(6)] public required string Code { get; set; }

    public Guid? ClinicVoucherId { get; set; }
    public virtual ClinicVoucher? ClinicVoucher { get; set; }
    public Guid? OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}