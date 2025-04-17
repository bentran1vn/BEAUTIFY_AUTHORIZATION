using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Clinic : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(100)] public required string Email { get; set; }

    [MaxLength(15, ErrorMessage = "Phone Number must be 10 digits")]
    public required string PhoneNumber { get; set; }

    public TimeSpan? WorkingTimeStart { get; set; }
    public TimeSpan? WorkingTimeEnd { get; set; }

    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? District { get; set; }
    [MaxLength(100)] public string? Ward { get; set; }
    [MaxLength(100)] public string? Address { get; set; }

    [MaxLength(250)] public string? FullAddress => $"{Address}, {Ward}, {District}, {City}".Trim(',', ' ', '\n');
    [MaxLength(20)] public required string TaxCode { get; set; }
    [MaxLength(250)] public required string BusinessLicenseUrl { get; set; }
    [MaxLength(250)] public required string OperatingLicenseUrl { get; set; }
    public DateTimeOffset? OperatingLicenseExpiryDate { get; set; }
    public int AdditionBranches { get; set; } = 0;
    public int AdditionLivestreams { get; set; } = 0;
    public int Status { get; set; } = 0;

    // 0 Pending, 1 Approve, 2 Reject, 3 Banned
    public int TotalApply { get; set; } = 0;
    [MaxLength(250)] public string? ProfilePictureUrl { get; set; }
    public int? TotalBranches { get; set; } = 0;
    public bool IsActivated { get; set; } = false;
    public bool? IsParent { get; set; } = false;
    public bool? IsFirstLogin { get; set; }
    [MaxLength(255)] public string? BankName { get; set; }
    [MaxLength(100)] public string? BankAccountNumber { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Balance { get; set; }
    public Guid? ParentId { get; set; }
    public virtual Clinic? Parent { get; set; }
    [MaxLength(250)] public string? Note { get; set; }
    public virtual ICollection<Clinic> Children { get; set; } = [];
    public virtual ICollection<ClinicOnBoardingRequest>? ClinicOnBoardingRequests { get; set; }
    public virtual ICollection<SystemTransaction>? SystemTransaction { get; set; }

    public virtual ICollection<ClinicService>? ClinicServices { get; set; }
    public virtual ICollection<UserClinic>? UserClinics { get; set; }

    public virtual ICollection<LivestreamRoom>? LivestreamRooms { get; set; }
    public virtual ICollection<UserConversation>? UserConversations { get; set; }
    public virtual ICollection<ClinicVoucher>? ClinicVouchers { get; set; }
    // public virtual ICollection<WalletTransaction>? Transactions { get; init; } = [];
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}