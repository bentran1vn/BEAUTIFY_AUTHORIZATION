using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class User : AggregateRoot<Guid>, IAuditableEntity
{
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)(\.[a-zA-Z]{2,})$", ErrorMessage = "Invalid Email Format")]
    [MaxLength(100)]
    [Required]
    public required string Email { get; init; }

    [MaxLength(50)] public required string FirstName { get; set; }
    [MaxLength(50)] public required string LastName { get; set; }
    [MaxLength(255)] public required string Password { get; set; }
    [MaxLength(50)] public required int Status { get; set; }
    // 0 Pending 1 Approve 2 Reject 3 Banned
    public required DateOnly DateOfBirth { get; set; }
    public Guid? RoleId { get; set; }
    public virtual Role? Role { get; set; }

    [MaxLength(14, ErrorMessage = "Phone Number must be 10 digits")]
    public required string PhoneNumber { get; set; }

    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    [MaxLength(250)] public string? ProfilePicture { get; set; }
    [MaxLength(250)] public string? Address { get; set; }
    [MaxLength(250)] public string? RefreshToken { get; set; }


    public virtual ICollection<UserClinic>? UserClinics { get; set; }
    public virtual ICollection<DoctorCertificate>? DoctorCertificates { get; set; }
    public virtual ICollection<UserConversation>? UserConversations { get; set; }
    public virtual ICollection<DoctorService>? DoctorServices { get; set; }
    public virtual ICollection<CustomerSchedule>? CustomerSchedules { get; set; }
    public virtual ICollection<Order>? Orders { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}