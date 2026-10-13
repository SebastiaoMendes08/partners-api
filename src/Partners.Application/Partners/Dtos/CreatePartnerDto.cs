namespace Partners.Application.Partners.Dtos;

public sealed record CreatePartnerDto(
    string? Name,
    string? TaxNumber,
    string? Type,
    string? Email,
    decimal? CreditLimit) : IPartnerDetails;
