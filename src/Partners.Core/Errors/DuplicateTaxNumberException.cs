namespace Partners.Core.Errors;

public sealed class DuplicateTaxNumberException(string taxNumber, Exception? innerException = null)
    : Exception($"A partner with tax number '{taxNumber}' already exists.", innerException);
