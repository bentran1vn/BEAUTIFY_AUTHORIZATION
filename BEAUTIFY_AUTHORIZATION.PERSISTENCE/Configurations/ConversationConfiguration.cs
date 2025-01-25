using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_AUTHORIZATION.PERSISTENCE.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BEAUTIFY_AUTHORIZATION.PERSISTENCE.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable(TableNames.Conversations);

        builder.HasKey(x => x.Id);
        
        builder
            .HasOne(c => c.Sender)
            .WithMany()
            .HasForeignKey(c => c.SenderId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete

        builder
            .HasOne(c => c.Receiver)
            .WithMany()
            .HasForeignKey(c => c.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete
    }
}