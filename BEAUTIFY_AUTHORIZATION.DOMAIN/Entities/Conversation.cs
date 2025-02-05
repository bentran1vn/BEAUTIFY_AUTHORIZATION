using System.ComponentModel.DataAnnotations;

namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Conversation : AggregateRoot<Guid>, IAuditableEntity
{
    [MaxLength(50)] public required string Type { get; set; }
    public virtual ICollection<Message>? Messages { get; set; }
    public virtual ICollection<UserConversation>? UserConversations { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}