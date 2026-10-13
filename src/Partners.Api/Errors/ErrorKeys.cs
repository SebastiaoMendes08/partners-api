using System.Text.Json;

namespace Partners.Api.Errors;

internal static class ErrorKeys
{
    public static string ToCamelCase(string key) => JsonNamingPolicy.CamelCase.ConvertName(key);
}
