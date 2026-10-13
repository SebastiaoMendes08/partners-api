using Partners.Application.Common;
using Partners.Core.Common;
using Partners.Core.Partners;

namespace Partners.Application.Partners.Dtos;

public sealed record ListPartnersQuery
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public string? Search { get; init; }
    public string? Type { get; init; }
    public bool? IncludeInactive { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public Result<PartnerSearchCriteria> ToCriteria()
    {
        var errors = new ValidationErrors();

        PartnerType? type = null;
        if (!string.IsNullOrWhiteSpace(Type))
        {
            if (PartnerTypes.TryParse(Type, out var parsed))
            {
                type = parsed;
            }
            else
            {
                errors.Add(nameof(Type), $"Type must be one of: {PartnerTypes.AllowedValues}.");
            }
        }

        var page = Page ?? DefaultPage;
        if (page < 1)
        {
            errors.Add(nameof(Page), "Page must be greater than or equal to 1.");
        }

        var pageSize = PageSize ?? DefaultPageSize;
        if (pageSize is < 1 or > MaxPageSize)
        {
            errors.Add(nameof(PageSize), $"PageSize must be between 1 and {MaxPageSize}.");
        }

        if (errors.HasErrors)
        {
            return errors.ToError();
        }

        return PartnerSearchCriteria.Create(Search, type, IncludeInactive ?? false, page, pageSize);
    }
}
