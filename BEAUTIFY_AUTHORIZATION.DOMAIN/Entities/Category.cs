using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Category : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(250)] public string? Description { get; set; }
    public bool IsParent { get; set; } = false;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
    public Guid? ParentId { get; set; }
    public virtual Category? Parent { get; set; }

    public virtual ICollection<Category> Children { get; set; } = [];
    // public Guid? ClinicId { get; set; }
    // public virtual Clinic? Clinic { get; set; }

    public virtual ICollection<Service> Services { get; set; } = [];

    public virtual ICollection<DoctorService>? DoctorServices { get; set; }
}