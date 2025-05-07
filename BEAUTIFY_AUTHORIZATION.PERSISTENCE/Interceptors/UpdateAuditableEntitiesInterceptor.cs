using BEAUTIFY_PACKAGES.BEAUTIFY_PACKAGES.DOMAIN.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BEAUTIFY_AUTHORIZATION.PERSISTENCE.Interceptors;
public sealed class UpdateAuditableEntitiesInterceptor
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);

        var entries =
            dbContext
                .ChangeTracker
                .Entries<IAuditableEntity>();

        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
                entityEntry.Property(a => a.CreatedOnUtc).CurrentValue
                    = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnamTimeZone);

            if (entityEntry.State == EntityState.Modified)
                entityEntry.Property(a => a.ModifiedOnUtc).CurrentValue
                    = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnamTimeZone);
        }

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}