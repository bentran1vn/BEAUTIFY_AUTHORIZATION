using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Order : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid CustomerId { get; set; }
    public virtual User? Customer { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? TotalAmount { get; set; }
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    public string? Status { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
}