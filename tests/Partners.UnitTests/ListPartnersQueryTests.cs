using Partners.Application.Partners.Dtos;
using Partners.Core.Partners;

namespace Partners.UnitTests;

public class ListPartnersQueryTests
{
    private static readonly ListPartnersQuery Empty = new();

    [Fact]
    public void Defaults_are_applied()
    {
        var filter = Empty.ToCriteria().Value;

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
        var filter = (Empty with { Type = type }).ToCriteria().Value;

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
        var filter = (Empty with { Search = search }).ToCriteria().Value;

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
        var result = (Empty with { Page = page, PageSize = pageSize }).ToCriteria();

        Assert.True(result.IsFailure);
        Assert.Equal([field], result.Error.ValidationErrors!.Keys);
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("ALL")]
    [InlineData("1")]
    public void Invalid_type_is_rejected(string type)
    {
        var result = (Empty with { Type = type }).ToCriteria();

        Assert.True(result.IsFailure);
        Assert.Equal(["Type"], result.Error.ValidationErrors!.Keys);
    }
}
