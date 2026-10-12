using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Partners.Core.Partners;

namespace Partners.Infrastructure.Persistence.Configurations;

public sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("Partners", table =>
        {
            table.HasCheckConstraint("CK_Partners_TaxNumber",
                "LEN([TaxNumber]) = 9 AND [TaxNumber] NOT LIKE '%[^0-9]%'");
            table.HasCheckConstraint("CK_Partners_Type",
                "[Type] IN ('CUSTOMER', 'SUPPLIER', 'BOTH')");
            table.HasCheckConstraint("CK_Partners_CreditLimit",
                "[CreditLimit] >= 0 AND [CreditLimit] <= 1000000");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(PartnerRules.NameMaxLength)
            .UseCollation("Latin1_General_100_CI_AS");

        builder.Property(p => p.TaxNumber)
            .HasMaxLength(PartnerRules.TaxNumberLength)
            .IsUnicode(false);

        builder.Property(p => p.Type)
            .HasMaxLength(8)
            .IsUnicode(false)
            .HasConversion(type => type.ToCode(), code => PartnerTypes.Parse(code));

        builder.Property(p => p.Email)
            .HasMaxLength(PartnerRules.EmailMaxLength);

        builder.Property(p => p.CreditLimit)
            .HasPrecision(18, 2);

        builder.Property(p => p.Version)
            .IsRowVersion();

        builder.HasIndex(p => p.TaxNumber)
            .IsUnique();

        builder.HasIndex(p => new { p.Name, p.Id });

        builder.HasData(SeedData.Partners);
    }
}
