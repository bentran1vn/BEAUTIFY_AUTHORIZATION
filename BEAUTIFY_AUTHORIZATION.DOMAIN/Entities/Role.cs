namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Role : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}