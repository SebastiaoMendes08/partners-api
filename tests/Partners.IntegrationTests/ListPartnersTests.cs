using System.Data.SqlTypes;
using System.Net;
using Partners.IntegrationTests.Infrastructure;

namespace Partners.IntegrationTests;

public class ListPartnersTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Default_listing_returns_paging_metadata()
    {
        await CreateAsync("Ana Silva", "100000001");

        var page = await ListAsync();

        Assert.Single(page.Items);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, page.Page);
        Assert.Equal(20, page.PageSize);
    }

    [Fact]
    public async Task Partner_of_type_both_matches_every_type_filter()
    {
        var both = await CreateAsync("Comercial Both", "100000001", "BOTH");
        var customer = await CreateAsync("Cliente Lda", "100000002", "CUSTOMER");
        var supplier = await CreateAsync("Fornecedor Lda", "100000003", "SUPPLIER");

        Assert.Equal([customer.Id, both.Id], Ids(await ListAsync("?Type=CUSTOMER")));
        Assert.Equal([both.Id, supplier.Id], Ids(await ListAsync("?Type=SUPPLIER")));
        Assert.Equal([both.Id], Ids(await ListAsync("?Type=BOTH")));
    }

    [Fact]
    public async Task Inactive_partner_is_only_listed_when_requested()
    {
        var partner = await CreateAsync("Ana Silva", "100000001");
        var other = await CreateAsync("Bruno Costa", "100000002");

        var inactive = await SetActiveAsync(partner, false);

        Assert.Equal([other.Id], Ids(await ListAsync()));
        Assert.Equal([partner.Id, other.Id], Ids(await ListAsync("?IncludeInactive=true")));
        Assert.False((await GetStoredAsync(partner.Id)).IsActive);

        await SetActiveAsync(inactive, true);

        Assert.Equal([partner.Id, other.Id], Ids(await ListAsync()));
    }

    [Fact]
    public async Task Filters_are_combined()
    {
        var match = await CreateAsync("Ana Fornecedora", "100000001", "SUPPLIER");
        var matchBoth = await CreateAsync("Ana Comercial", "100000002", "BOTH");
        await CreateAsync("Ana Cliente", "100000003", "CUSTOMER");
        await CreateAsync("Bruno Fornecedor", "100000004", "SUPPLIER");
        var inactive = await CreateAsync("Ana Inativa", "100000005", "SUPPLIER");
        await SetActiveAsync(inactive, false);

        var page = await ListAsync("?Search=ana&Type=SUPPLIER");
        Assert.Equal([matchBoth.Id, match.Id], Ids(page));
        Assert.Equal(2, page.TotalCount);

        var withInactive = await ListAsync("?Search=ana&Type=SUPPLIER&IncludeInactive=true");
        Assert.Equal([matchBoth.Id, match.Id, inactive.Id], Ids(withInactive));
    }

    [Fact]
    public async Task Total_count_is_calculated_before_paging()
    {
        for (var i = 1; i <= 5; i++)
        {
            await CreateAsync($"Parceiro {i}", $"10000000{i}");
        }

        var second = await ListAsync("?Page=2&PageSize=2");
        Assert.Equal(["Parceiro 3", "Parceiro 4"], second.Items.Select(p => p.Name));
        Assert.Equal(5, second.TotalCount);
        Assert.Equal(2, second.Page);
        Assert.Equal(2, second.PageSize);

        var last = await ListAsync("?Page=3&PageSize=2");
        Assert.Equal(["Parceiro 5"], last.Items.Select(p => p.Name));

        var beyond = await ListAsync("?Page=4&PageSize=2");
        Assert.Empty(beyond.Items);
        Assert.Equal(5, beyond.TotalCount);
    }

    [Fact]
    public async Task Page_far_beyond_results_returns_empty_items()
    {
        await CreateAsync("Ana Silva", "100000001");

        var page = await ListAsync($"?Page={int.MaxValue}&PageSize=100");

        Assert.Empty(page.Items);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task Partners_with_equal_names_are_ordered_by_id()
    {
        var created = new List<PartnerDto>();
        for (var i = 1; i <= 4; i++)
        {
            created.Add(await CreateAsync("Ana Silva", $"20000000{i}"));
        }

        await CreateAsync("Abel Santos", "300000001");

        var expected = created
            .Select(p => p.Id)
            .OrderBy(id => new SqlGuid(id))
            .ToList();

        var page = await ListAsync("?Search=Ana Silva");
        Assert.Equal(expected, Ids(page));

        var firstPage = await ListAsync("?Search=Ana Silva&PageSize=2");
        var secondPage = await ListAsync("?Search=Ana Silva&PageSize=2&Page=2");
        Assert.Equal(expected, Ids(firstPage).Concat(Ids(secondPage)));
    }

    [Fact]
    public async Task Search_by_name_does_not_match_every_tax_number()
    {
        var ana = await CreateAsync("Ana Silva", "001234567");
        await CreateAsync("Bruno Costa", "100000002");
        await CreateAsync("Carlos Lima", "100000003");

        var page = await ListAsync("?Search=Ana");

        Assert.Equal([ana.Id], Ids(page));
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task Search_by_name_is_partial_and_case_insensitive()
    {
        var ana = await CreateAsync("Ana Silva", "100000001");
        await CreateAsync("Bruno Costa", "100000002");

        Assert.Equal([ana.Id], Ids(await ListAsync("?Search=nA sIL")));
    }

    [Theory]
    [InlineData("001234567")]
    [InlineData("001.234.567")]
    [InlineData("001 234 567")]
    [InlineData("234-56")]
    public async Task Search_matches_tax_number_ignoring_separators(string search)
    {
        var ana = await CreateAsync("Ana Silva", "001234567");
        await CreateAsync("Bruno Costa", "100000002");

        var page = await ListAsync($"?Search={Uri.EscapeDataString(search)}");

        Assert.Equal([ana.Id], Ids(page));
    }

    [Fact]
    public async Task Search_made_only_of_separators_does_not_match_every_tax_number()
    {
        await CreateAsync("Ana Silva", "100000001");
        var dotted = await CreateAsync("A.B. Comercial", "100000002");

        var page = await ListAsync("?Search=.");

        Assert.Equal([dotted.Id], Ids(page));
    }

    [Fact]
    public async Task Search_treats_wildcards_as_literal_text()
    {
        var percent = await CreateAsync("Desconto 100% Lda", "100000001");
        await CreateAsync("Ana Silva", "100000002");

        Assert.Equal([percent.Id], Ids(await ListAsync("?Search=%25")));
        Assert.Empty((await ListAsync("?Search=_")).Items);
    }

    [Theory]
    [InlineData("?Page=0")]
    [InlineData("?Page=-1")]
    [InlineData("?PageSize=0")]
    [InlineData("?PageSize=101")]
    [InlineData("?Type=customer")]
    [InlineData("?Type=ALL")]
    [InlineData("?IncludeInactive=maybe")]
    [InlineData("?Page=abc")]
    public async Task Invalid_filters_return_bad_request(string query)
    {
        var response = await Client.GetAsync($"{BaseUrl}{query}");

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.NotEmpty(problem.Errors!);
    }

    private static List<Guid> Ids(PageDto page) => page.Items.Select(p => p.Id).ToList();
}
