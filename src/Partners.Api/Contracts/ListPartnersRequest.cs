using Partners.Core.Partners;

namespace Partners.Api.Contracts;

public sealed class ListPartnersRequest
{
    public string? Search { get; init; }
    public string? Type { get; init; }
    public bool? IncludeInactive { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    public PartnerListInput ToInput() => new(Search, Type, IncludeInactive, Page, PageSize);
}
