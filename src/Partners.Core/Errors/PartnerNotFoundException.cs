namespace Partners.Core.Errors;

public sealed class PartnerNotFoundException(Guid id)
    : Exception($"Partner '{id}' was not found.");
