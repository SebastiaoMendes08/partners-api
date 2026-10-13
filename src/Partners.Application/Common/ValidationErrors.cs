using Partners.Core.Common;

namespace Partners.Application.Common;

public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public int Count => _errors.Values.Sum(messages => messages.Count);

    public bool HasErrors => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
    }

    public Error ToError() => Error.Validation(_errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
}
