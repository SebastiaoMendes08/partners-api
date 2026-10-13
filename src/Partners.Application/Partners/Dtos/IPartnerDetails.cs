namespace Partners.Application.Partners.Dtos;

public interface IPartnerDetails
{
    string? Name { get; }
    string? TaxNumber { get; }
    string? Type { get; }
    string? Email { get; }
    decimal? CreditLimit { get; }
}
