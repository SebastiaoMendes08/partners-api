namespace Partners.IntegrationTests.Infrastructure;

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
    public UpdateBody ToUpdate() => new(Name, TaxNumber, Type, Email, CreditLimit, IsActive, Version);
}

public sealed record UpdateBody(
    string Name,
    string TaxNumber,
    string Type,
    string? Email,
    decimal CreditLimit,
    bool IsActive,
    string Version);

public sealed record PageDto(List<PartnerDto> Items, int TotalCount, int Page, int PageSize);

public sealed record ProblemDto(
    string? Type,
    string Title,
    int Status,
    string? Detail,
    Dictionary<string, string[]>? Errors);
