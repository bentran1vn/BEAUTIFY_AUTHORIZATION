namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;

public class ProcedureMedia: AggregateRoot<Guid>, IAuditableEntity
{
    public string ImageUrl { get; set; } = default!;
    public int IndexNumber { get; set; }
    
    public Guid ProcedureId { get; set; } = default!;
    public virtual Procedure Procedure { get; set; } = default!;
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}