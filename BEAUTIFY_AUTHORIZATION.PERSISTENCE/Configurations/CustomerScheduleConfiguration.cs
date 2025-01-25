using BEAUTIFY_AUTHORIZATION.DOMAIN.Entities;
using BEAUTIFY_AUTHORIZATION.PERSISTENCE.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BEAUTIFY_AUTHORIZATION.PERSISTENCE.Configurations;

internal sealed class CustomerScheduleConfiguration : IEntityTypeConfiguration<CustomerSchedule>
{
    public void Configure(EntityTypeBuilder<CustomerSchedule> builder)
    {
        builder.ToTable(TableNames.CustomerSchedules);

        builder.HasKey(x => x.Id);
        
        builder
            .HasOne(c => c.Customer)
            .WithMany()
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete
    }
}