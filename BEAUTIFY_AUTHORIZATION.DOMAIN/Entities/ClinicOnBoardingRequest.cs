using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class ClinicOnBoardingRequest : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(50)] public int Status { get; set; } = 0;
    // 0 Pending 1 Approve 2 Reject 3 Banned
    [MaxLength(250)] public string? RejectReason { get; set; } 
    public DateTimeOffset SendMailDate { get; set; }
    public Guid ClinicId { get; set; }
    public virtual Clinic? Clinic { get; set; }
    
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}