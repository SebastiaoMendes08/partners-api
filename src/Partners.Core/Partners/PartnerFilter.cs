using Partners.Core.Errors;

namespace Partners.Core.Partners;

public sealed record PartnerListInput(
    string? Search,
    string? Type,
    bool? IncludeInactive,
    int? Page,
    int? PageSize);

public sealed record PartnerFilter(
    string? Search,
    string? TaxNumberSearch,
    IReadOnlyList<PartnerType>? Types,
    bool IncludeInactive,
    int Page,
    int PageSize)
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static PartnerFilter Create(PartnerListInput input)
    {
        var errors = new ValidationErrors();

        IReadOnlyList<PartnerType>? types = null;
        if (!string.IsNullOrWhiteSpace(input.Type))
        {
            if (PartnerTypes.TryParse(input.Type, out var type))
            {
                types = PartnerTypes.MatchedBy(type);
            }
            else
            {
                errors.Add(nameof(input.Type), $"Type must be one of: {PartnerTypes.AllowedValues}.");
            }
        }

        var page = input.Page ?? DefaultPage;
        if (page < 1)
        {
            errors.Add(nameof(input.Page), "Page must be greater than or equal to 1.");
        }

        var pageSize = input.PageSize ?? DefaultPageSize;
        if (pageSize is < 1 or > MaxPageSize)
        {
            errors.Add(nameof(input.PageSize), $"PageSize must be between 1 and {MaxPageSize}.");
        }

        errors.ThrowIfAny();

        var search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();

        return new PartnerFilter(
            search,
            ToTaxNumberSearch(search),
            types,
            input.IncludeInactive ?? false,
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
