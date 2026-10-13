namespace Partners.Core.Common;

public sealed record Error(
    ErrorType Type,
    string Title,
    string? Detail = null,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new(ErrorType.Validation, "One or more validation errors occurred.", ValidationErrors: errors);

    public static Error NotFound(string title, string detail) => new(ErrorType.NotFound, title, detail);

    public static Error Conflict(string title, string detail) => new(ErrorType.Conflict, title, detail);
}
