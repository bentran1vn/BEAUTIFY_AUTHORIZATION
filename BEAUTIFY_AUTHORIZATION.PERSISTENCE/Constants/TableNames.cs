namespace BEAUTIFY_AUTHORIZATION.PERSISTENCE.Constants;

internal static class TableNames
{
    // For Outbox Pattern
    internal const string OutboxMessages = nameof(OutboxMessages);

    // *********** Singular Nouns ***********
    internal const string Clinics = nameof(Clinics);
    internal const string Conversations = nameof(Conversations);
    internal const string CustomerSchedules = nameof(CustomerSchedules);
}