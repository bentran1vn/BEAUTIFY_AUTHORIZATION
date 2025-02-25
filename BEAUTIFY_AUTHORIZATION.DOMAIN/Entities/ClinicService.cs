namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;

public class ClinicService : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid ClinicId { get; set; }
    public virtual Clinic Clinics { get; set; }
    
    public Guid ServiceId { get; set; }
    public virtual Service Services { get; set; }
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}