using Partners.Core.Partners;

namespace Partners.Api.Contracts;

public sealed record UpdatePartnerRequest(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit,
    bool? IsActive,
    string? Version)
{
    public PartnerUpdateInput ToInput() => new(Name, TaxNumber, Type, Email, CreditLimit, IsActive, Version);
}
