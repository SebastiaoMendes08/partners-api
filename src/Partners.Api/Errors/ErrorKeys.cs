using System.Text.Json;

namespace Partners.Api.Errors;

internal static class ErrorKeys
{
    public static Dictionary<string, string[]> ToCamelCase(IReadOnlyDictionary<string, string[]> errors) =>
        errors.ToDictionary(e => ToCamelCase(e.Key), e => e.Value);

    public static string ToCamelCase(string key) => JsonNamingPolicy.CamelCase.ConvertName(key);
}
