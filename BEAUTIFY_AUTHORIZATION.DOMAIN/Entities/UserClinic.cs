namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class UserClinic : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public Guid ClinicId { get; set; }
    public virtual Clinics? Clinic { get; set; }
    public virtual User? User { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public virtual ICollection<WorkingSchedule>? WorkingSchedules { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; }
}