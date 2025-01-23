namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Feedback : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid OrderDetailId { get; set; }
    public virtual OrderDetail? OrderDetail { get; set; }
    public string? Content { get; set; }
    public int Rating { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}