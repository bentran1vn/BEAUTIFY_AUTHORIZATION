namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class DoctorCertificate : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid DoctorId { get; set; }
    public virtual User? Doctor { get; set; }
    public string? CertificateUrl { get; set; }
    public string? CertificateName { get; set; }
    public string? CertificateNumber { get; set; }
    public DateTimeOffset? ExpiryDate { get; set; }
    public string? Note { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}