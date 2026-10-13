using Partners.Core.Partners;

namespace Partners.Application.Partners.Dtos;

public sealed record PartnerDto(
    Guid Id,
    string Name,
    string TaxNumber,
    string Type,
    string? Email,
    decimal CreditLimit,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Version)
{
    internal static PartnerDto From(Partner partner) => new(
        partner.Id,
        partner.Name,
        partner.TaxNumber,
        partner.Type.ToCode(),
        partner.Email,
        partner.CreditLimit,
        partner.IsActive,
        partner.CreatedAt,
        partner.UpdatedAt,
        VersionToken.Encode(partner.Version));
}
