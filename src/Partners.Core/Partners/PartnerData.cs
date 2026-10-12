namespace Partners.Core.Partners;

public sealed record PartnerData(
    string Name,
    string TaxNumber,
    PartnerType Type,
    string? Email,
    decimal CreditLimit);
