using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class SubscriptionPackage : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(50)] public required string Name { get; set; }
    [MaxLength(200)] public required string Description { get; set; }
    [Column(TypeName = "decimal(18,2)")] public required decimal Price { get; set; }
    public required int Duration { get; set; }
    public bool IsActivated { get; set; } = false;
    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}