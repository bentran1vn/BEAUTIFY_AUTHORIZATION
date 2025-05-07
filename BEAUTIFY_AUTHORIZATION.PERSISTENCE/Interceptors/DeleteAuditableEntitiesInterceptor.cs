using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BEAUTIFY_AUTHORIZATION.PERSISTENCE.Interceptors;
public sealed class DeleteAuditableEntitiesInterceptor : SaveChangesInterceptor
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
                .Entries();

        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        foreach (var entityEntry in entries)
            if (entityEntry.State == EntityState.Deleted)
            {
                var entity = entityEntry.Entity;
                var isDeletedProperty = entity.GetType().GetProperty("IsDeleted");
                var updateProperty = entity.GetType().GetProperty("ModifiedOnUtc");
                if (isDeletedProperty != null && isDeletedProperty.CanWrite && updateProperty != null &&
                    updateProperty.CanWrite)
                {
                    entityEntry.State = EntityState.Modified;
                    isDeletedProperty.SetValue(entity, true);
                    updateProperty.SetValue(entity,
                        TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnamTimeZone));
                }
            }

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }
}