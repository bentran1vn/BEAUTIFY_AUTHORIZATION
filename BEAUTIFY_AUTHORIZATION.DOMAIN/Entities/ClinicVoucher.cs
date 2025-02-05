using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class ClinicVoucher : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid ClinicId { get; set; }
    public virtual Clinics? Clinic { get; set; }
    public Guid ServiceId { get; set; }
    public virtual Service? Service { get; set; }
    [MaxLength(100)] public required string Name { get; set; }
    public double MaximumDiscountPercent { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal MaximumDiscountAmount { get; set; }
    public int MaximumUsage { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActivated { get; set; } = false;
    public int? TotalUsage { get; set; }
    public virtual ICollection<Voucher>? Vouchers { get; set; } = [];

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}