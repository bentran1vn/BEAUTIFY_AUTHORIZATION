using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class OrderDetail : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid OrderId { get; set; }
    public virtual Order? Order { get; set; }
    public Guid ServerId { get; set; }
    public virtual Service? Service { get; set; }
    public int Quantity { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal? Discount { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public Guid? FeedbackId { get; set; }
    public virtual Feedback? Feedback { get; set; }
}