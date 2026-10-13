namespace Partners.Core.Partners;

public sealed record PartnerSearchCriteria(
    string? Search,
    string? TaxNumberSearch,
    IReadOnlyList<PartnerType>? Types,
    bool IncludeInactive,
    int Page,
    int PageSize)
{
    public static PartnerSearchCriteria Create(string? search, PartnerType? type, bool includeInactive, int page,
        int pageSize)
    {
        var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        return new PartnerSearchCriteria(
            term,
            ToTaxNumberSearch(term),
            type is { } value ? PartnerTypes.MatchedBy(value) : null,
            includeInactive,
            page,
            pageSize);
    }

    // Only separators are removed: a term like "Ana" must not become an empty
    // pattern that would match every tax number.
    private static string? ToTaxNumberSearch(string? search)
    {
        if (search is null)
        {
            return null;
        }

        var normalized = PartnerRules.NormalizeTaxNumber(search);
        return normalized.Length > 0 && normalized.All(char.IsAsciiDigit) ? normalized : null;
    }
}
