namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class UserConversation : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid ConversationId { get; set; }
    public virtual Conversation Conversation { get; set; } = null!;

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}