namespace Partners.Core.Errors;

public sealed class ConcurrencyConflictException(Guid id, Exception? innerException = null)
    : Exception($"Partner '{id}' was changed by another request. Get the latest version and try again.", innerException);
