using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Service : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(50)] public required string Name { get; set; }
    [MaxLength(200)] public required string Description { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    public int NumberOfCustomersUsed { get; set; } = 0;
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? DiscountPrice { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }

    public virtual ICollection<Promotion>? Promotions { get; set; }
    public virtual ICollection<Procedure>? Procedures { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; }
    public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    public virtual ICollection<ClinicVoucher>? ClinicVouchers { get; set; }
}