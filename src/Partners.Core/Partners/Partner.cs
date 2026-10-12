namespace Partners.Core.Partners;

public class Partner
{
    private Partner()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string TaxNumber { get; private set; } = string.Empty;
    public PartnerType Type { get; private set; }
    public string? Email { get; private set; }
    public decimal CreditLimit { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public byte[] Version { get; private set; } = [];

    public static Partner Create(PartnerData data, DateTime utcNow)
    {
        var partner = new Partner
        {
            IsActive = true,
            CreatedAt = utcNow
        };
        partner.Apply(data, utcNow);
        return partner;
    }

    public void Update(PartnerData data, bool isActive, DateTime utcNow)
    {
        Apply(data, utcNow);
        IsActive = isActive;
    }

    private void Apply(PartnerData data, DateTime utcNow)
    {
        Name = data.Name;
        TaxNumber = data.TaxNumber;
        Type = data.Type;
        Email = data.Email;
        CreditLimit = data.CreditLimit;
        UpdatedAt = utcNow;
    }
}
