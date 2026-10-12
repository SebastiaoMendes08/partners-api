namespace Partners.Core.Partners;

public record PartnerInput(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit);

public sealed record PartnerUpdateInput(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit,
    bool? IsActive,
    string? Version)
    : PartnerInput(Name, TaxNumber, Type, Email, CreditLimit);
