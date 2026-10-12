using System.Net;
using Partners.IntegrationTests.Infrastructure;

namespace Partners.IntegrationTests;

public class CreatePartnerTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Create_stores_normalized_values_and_returns_location()
    {
        var response = await PostAsync(new
        {
            name = " Ana Silva ",
            taxNumber = "001.234.567",
            type = "CUSTOMER",
            email = "   ",
            creditLimit = 1500.50m
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<PartnerDto>(response);
        Assert.Equal($"{BaseUrl}/{created.Id}", response.Headers.Location?.AbsolutePath);
        Assert.Equal("Ana Silva", created.Name);
        Assert.Equal("001234567", created.TaxNumber);
        Assert.Equal("CUSTOMER", created.Type);
        Assert.Null(created.Email);
        Assert.Equal(1500.50m, created.CreditLimit);
        Assert.True(created.IsActive);
        Assert.False(string.IsNullOrEmpty(created.Version));
        Assert.Equal(created.CreatedAt, created.UpdatedAt);

        var stored = await GetStoredAsync(created.Id);
        Assert.Equal("Ana Silva", stored.Name);
        Assert.Equal("001234567", stored.TaxNumber);
        Assert.Equal(created.Version, stored.Version);
    }

    [Fact]
    public async Task Same_tax_number_in_another_format_returns_conflict()
    {
        await CreateAsync("Ana Silva", "001.234.567");

        var response = await PostAsync(new
        {
            name = "Outra Ana",
            taxNumber = "001 234 567",
            type = "SUPPLIER",
            creditLimit = 0
        });

        var problem = await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Contains("001234567", problem.Detail);
        Assert.Equal(1, await CountStoredAsync());
    }

    [Fact]
    public async Task Tax_number_stays_reserved_after_deactivation()
    {
        var partner = await CreateAsync("Ana Silva", "001.234.567");
        await SetActiveAsync(partner, false);

        var response = await PostAsync(new
        {
            name = "Ana Silva",
            taxNumber = "001-234-567",
            type = "CUSTOMER",
            creditLimit = 0
        });

        await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Equal(1, await CountStoredAsync());
    }

    [Fact]
    public async Task Get_returns_partner_by_id()
    {
        var created = await CreateAsync("Ana Silva", "001234567", "BOTH", "ana@example.com", 250m);

        var stored = await GetStoredAsync(created.Id);

        Assert.Equal(created, stored);
    }

    [Fact]
    public async Task Get_unknown_id_returns_not_found()
    {
        var response = await GetAsync(Guid.NewGuid());

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_malformed_id_returns_bad_request()
    {
        var response = await Client.GetAsync($"{BaseUrl}/not-a-guid");

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Contains("id", problem.Errors!.Keys);
    }
}
