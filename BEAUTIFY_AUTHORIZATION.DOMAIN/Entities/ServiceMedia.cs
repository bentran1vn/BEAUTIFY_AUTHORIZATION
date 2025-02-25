namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;

public class ServiceMedia: AggregateRoot<Guid>, IAuditableEntity
{
    public string ImageUrl { get; set; } = default!;
    public int ServiceMediaType { get; set; } // 0 Product Slide, 1 Product Description
    public int IndexNumber { get; set; }
    
    public Guid ServiceId { get; set; } = default!;
    public virtual Service Service { get; set; } = default!;
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}