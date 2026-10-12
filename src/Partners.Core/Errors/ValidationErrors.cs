namespace Partners.Core.Errors;

public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public int Count => _errors.Values.Sum(messages => messages.Count);

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
    }

    public void ThrowIfAny()
    {
        if (_errors.Count > 0)
        {
            throw new ValidationException(_errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
        }
    }
}
