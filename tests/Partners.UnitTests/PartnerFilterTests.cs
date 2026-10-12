using Partners.Core.Errors;
using Partners.Core.Partners;

namespace Partners.UnitTests;

public class PartnerFilterTests
{
    private static readonly PartnerListInput Empty = new(null, null, null, null, null);

    [Fact]
    public void Defaults_are_applied()
    {
        var filter = PartnerFilter.Create(Empty);

        Assert.Null(filter.Search);
        Assert.Null(filter.TaxNumberSearch);
        Assert.Null(filter.Types);
        Assert.False(filter.IncludeInactive);
        Assert.Equal(1, filter.Page);
        Assert.Equal(20, filter.PageSize);
    }

    [Theory]
    [InlineData("CUSTOMER", new[] { PartnerType.Customer, PartnerType.Both })]
    [InlineData("SUPPLIER", new[] { PartnerType.Supplier, PartnerType.Both })]
    [InlineData("BOTH", new[] { PartnerType.Both })]
    public void Type_filter_includes_partners_that_are_both(string type, PartnerType[] expected)
    {
        var filter = PartnerFilter.Create(Empty with { Type = type });

        Assert.Equal(expected, filter.Types);
    }

    [Theory]
    [InlineData("Ana", "Ana", null)]
    [InlineData("  Ana  ", "Ana", null)]
    [InlineData("001.234", "001.234", "001234")]
    [InlineData("001 234 567", "001 234 567", "001234567")]
    [InlineData("12a", "12a", null)]
    [InlineData(".-.", ".-.", null)]
    [InlineData("   ", null, null)]
    public void Tax_number_search_is_only_used_for_digits(string search, string? expectedSearch,
        string? expectedTaxNumber)
    {
        var filter = PartnerFilter.Create(Empty with { Search = search });

        Assert.Equal(expectedSearch, filter.Search);
        Assert.Equal(expectedTaxNumber, filter.TaxNumberSearch);
    }

    [Theory]
    [InlineData(0, null, "Page")]
    [InlineData(-1, null, "Page")]
    [InlineData(null, 0, "PageSize")]
    [InlineData(null, 101, "PageSize")]
    public void Invalid_paging_is_rejected(int? page, int? pageSize, string field)
    {
        var exception = Assert.Throws<ValidationException>(() =>
            PartnerFilter.Create(Empty with { Page = page, PageSize = pageSize }));

        Assert.Equal([field], exception.Errors.Keys);
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("ALL")]
    [InlineData("1")]
    public void Invalid_type_is_rejected(string type)
    {
        var exception = Assert.Throws<ValidationException>(() => PartnerFilter.Create(Empty with { Type = type }));

        Assert.Equal(["Type"], exception.Errors.Keys);
    }
}
