namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Promotion : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public string? Status { get; set; }
    public double? Discount { get; set; }
    public Guid? ServiceId { get; set; }
    public virtual Service Service { get; set; }
    public bool IsActivated { get; set; } = false;
    public Guid? LivestreamRoomId { get; set; }
    public virtual LivestreamRoom? LivestreamRoom { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}