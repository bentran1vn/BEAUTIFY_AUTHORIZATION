namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Message : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid? ConversationId { get; set; }
    public virtual Conversation? Conversation { get; set; }
    public Guid SenderId { get; set; }
    public virtual User? Sender { get; set; }
    public string? Content { get; set; }
    public bool IsRead { get; set; } = false;

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}