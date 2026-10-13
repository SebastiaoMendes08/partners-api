namespace Partners.Application.Partners.Dtos;

public sealed record UpdatePartnerDto(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit,
    bool? IsActive,
    string? Version) : IPartnerDetails;
