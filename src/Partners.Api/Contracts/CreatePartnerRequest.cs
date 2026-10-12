using Partners.Core.Partners;

namespace Partners.Api.Contracts;

public sealed record CreatePartnerRequest(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit)
{
    public PartnerInput ToInput() => new(Name, TaxNumber, Type, Email, CreditLimit);
}
