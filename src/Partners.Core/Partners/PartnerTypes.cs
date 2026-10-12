using System.Diagnostics.CodeAnalysis;

namespace Partners.Core.Partners;

public static class PartnerTypes
{
    public const string Customer = "CUSTOMER";
    public const string Supplier = "SUPPLIER";
    public const string Both = "BOTH";
    public const string AllowedValues = $"{Customer}, {Supplier}, {Both}";

    public static string ToCode(this PartnerType type) => type switch
    {
        PartnerType.Customer => Customer,
        PartnerType.Supplier => Supplier,
        PartnerType.Both => Both,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public static bool TryParse([NotNullWhen(true)] string? code, out PartnerType type)
    {
        switch (code)
        {
            case Customer:
                type = PartnerType.Customer;
                return true;
            case Supplier:
                type = PartnerType.Supplier;
                return true;
            case Both:
                type = PartnerType.Both;
                return true;
            default:
                type = default;
                return false;
        }
    }

    public static PartnerType Parse(string code) =>
        TryParse(code, out var type) ? type : throw new FormatException($"Unknown partner type '{code}'.");

    public static IReadOnlyList<PartnerType> MatchedBy(PartnerType filter) => filter switch
    {
        PartnerType.Customer => [PartnerType.Customer, PartnerType.Both],
        PartnerType.Supplier => [PartnerType.Supplier, PartnerType.Both],
        _ => [PartnerType.Both]
    };
}
