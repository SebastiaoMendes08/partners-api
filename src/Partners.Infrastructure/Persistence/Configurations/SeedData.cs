using Partners.Core.Partners;

namespace Partners.Infrastructure.Persistence.Configurations;

internal static class SeedData
{
    private static readonly DateTime SeededAt = new(2026, 1, 15, 9, 0, 0, DateTimeKind.Utc);

    public static readonly object[] Partners =
    [
        Create("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1001", "Mercado Kianda, Lda", "500100200", PartnerType.Customer,
            "compras@kianda.example.com", 50_000m),
        Create("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1002", "Distribuidora Luanda Sul", "500200300", PartnerType.Supplier,
            "geral@luandasul.example.com", 0m),
        Create("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1003", "Transportes Benguela", "500300400", PartnerType.Both,
            null, 150_000m),
        Create("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1004", "Ana Costa", "123456780", PartnerType.Customer,
            "ana.costa@example.com", 2_500.50m),
        Create("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1005", "Ana Costa", "987654321", PartnerType.Both,
            null, 10_000m),
        Create("2b0f4b52-7a1e-4c55-9f43-0d6f2a1c1006", "Papelaria Central", "500400500", PartnerType.Supplier,
            "vendas@papelariacentral.example.com", 0m, isActive: false)
    ];

    private static object Create(string id, string name, string taxNumber, PartnerType type, string? email,
        decimal creditLimit, bool isActive = true) => new
    {
        Id = Guid.Parse(id),
        Name = name,
        TaxNumber = taxNumber,
        Type = type,
        Email = email,
        CreditLimit = creditLimit,
        IsActive = isActive,
        CreatedAt = SeededAt,
        UpdatedAt = SeededAt
    };
}
