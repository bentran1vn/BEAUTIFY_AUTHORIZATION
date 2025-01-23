namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Procedure : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public int Duration { get; set; }
    public Guid? ProcedureBeforeId { get; set; }
    public virtual Guid? ProcedureBefore { get; set; }
    public Guid? ProcedureAfterId { get; set; }
    public virtual Guid? ProcedureAfter { get; set; }
    public Guid? ServiceId { get; set; }
    public virtual Guid? Service { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; }
}