using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class User : AggregateRoot<Guid>, IAuditableEntity
{
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)(\.[a-zA-Z]{2,})$", ErrorMessage = "Invalid Email Format")]
    public required string Email { get; set; }

    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public Guid? RoleId { get; set; }
    public virtual Role? Role { get; set; }

    [Length(10, 10, ErrorMessage = "Phone Number must be 10 digits")]
    public required string PhoneNumber { get; set; }

    public required DateOnly DateOfBirth { get; set; }
    public required int Status { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? ProfilePicture { get; set; }
    public string? Address { get; set; }
    public string? RefreshToken { get; set; }


    public virtual ICollection<UserClinic>? UserClinics { get; set; }
    public virtual ICollection<DoctorCertificate>? DoctorCertificates { get; set; }
    public virtual ICollection<Conversation>? Conversations { get; set; }
    public virtual ICollection<DoctorService>? DoctorServices { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; }
    public virtual ICollection<Order>? Orders { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}