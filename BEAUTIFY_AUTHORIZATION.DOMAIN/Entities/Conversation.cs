namespace BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
public class Conversation : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid SenderId { get; set; }
    public virtual User? Sender { get; set; }
    public Guid ReceiverId { get; set; }
    public virtual User? Receiver { get; set; }
    public string? Type { get; set; }


    public virtual ICollection<Message>? Messages { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }
    public DateTimeOffset? ModifiedOnUtc { get; set; }
}