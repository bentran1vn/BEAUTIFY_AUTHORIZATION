using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class DoctorCertificate : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid DoctorId { get; set; }
    public virtual User? Doctor { get; set; }
    [MaxLength(250)] public required string CertificateUrl { get; set; }
    [MaxLength(100)] public required string CertificateName { get; set; }
    [MaxLength(50)] public required string CertificateNumber { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    [MaxLength(100)] public string? Note { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}