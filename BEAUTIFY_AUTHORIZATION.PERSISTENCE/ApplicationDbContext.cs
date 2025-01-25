using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using Microsoft.EntityFrameworkCore;

namespace BEAUTIFY_AUTHORIZATION.PERSISTENCE;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder) =>
        builder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);

    public DbSet<Category> Categories { get; set; }
    public DbSet<ClinicOnBoardingRequest> ClinicOnBoardingRequests { get; set; }
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<ClinicVoucher> ClinicVouchers { get; set; }
    public DbSet<Config> Configs { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<CustomerSchedule> CustomerSchedules { get; set; }
    public DbSet<DoctorCertificate> DoctorCertificates { get; set; }
    public DbSet<DoctorService> DoctorServices { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<LivestreamRoom> LivestreamRooms { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Procedure> Procedures { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
    public DbSet<SystemTransaction> SystemTransactions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserClinic> UserClinics { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    public DbSet<WorkingSchedule> WorkingSchedules { get; set; }
}