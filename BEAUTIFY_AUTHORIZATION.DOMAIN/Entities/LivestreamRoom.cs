namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class LivestreamRoom : AggregateRoot<Guid>, IAuditableEntity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TimeOnly? StartDate { get; set; }
    public TimeOnly? EndDate { get; set; }
    public string? Status { get; set; }
    public DateOnly? Date { get; set; }
    public string? Type { get; set; }
    public int? Duration { get; set; }
    public int? TotalViewers { get; set; }
    public Guid? ClinicId { get; set; }
    public virtual Clinic Clinic { get; set; } = null!;


    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public virtual ICollection<Promotion>? Promotions { get; set; } = [];
}