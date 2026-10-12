using System.Text.RegularExpressions;

namespace Partners.Core.Partners;

public static partial class PartnerRules
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;
    public const int TaxNumberLength = 9;
    public const int EmailMaxLength = 254;
    public const decimal CreditLimitMin = 0m;
    public const decimal CreditLimitMax = 1_000_000m;
    public const int CreditLimitDecimals = 2;

    public static string NormalizeTaxNumber(string value) =>
        string.Concat(value.Where(c => c is not (' ' or '.' or '-')));

    public static bool IsValidTaxNumber(string normalized) =>
        normalized.Length == TaxNumberLength && normalized.All(char.IsAsciiDigit);

    public static string? NormalizeEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static bool IsValidEmail(string email) =>
        email.Length <= EmailMaxLength && EmailPattern().IsMatch(email);

    public static bool HasValidPrecision(decimal value) =>
        decimal.Round(value, CreditLimitDecimals) == value;

    [GeneratedRegex(@"^[^@\s]+@[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?(?:\.[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?)+$")]
    private static partial Regex EmailPattern();
}
