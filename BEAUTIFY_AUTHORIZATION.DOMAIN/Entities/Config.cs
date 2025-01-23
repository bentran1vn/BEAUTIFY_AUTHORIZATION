namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Config : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Key { get; set; }
    public required string Value { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}