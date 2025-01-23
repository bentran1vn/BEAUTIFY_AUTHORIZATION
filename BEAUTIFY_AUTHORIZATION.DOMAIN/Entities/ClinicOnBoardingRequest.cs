namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class ClinicOnBoardingRequest : AggregateRoot<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Address { get; set; }
    public required string TaxCode { get; set; }
    public required string BusinessLicenseUrl { get; set; }
    public required string OperatingLicenseUrl { get; set; }
    public DateTimeOffset? OperatingLicenseExpiryDate { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }


    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}