using System.Net;
using Partners.IntegrationTests.Infrastructure;

namespace Partners.IntegrationTests;

public class UpdatePartnerTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Update_changes_data_and_returns_new_version()
    {
        var partner = await CreateAsync("Ana Silva", "001234567", "CUSTOMER", "ana@example.com", 100m);

        var updated = await UpdateAsync(partner, new UpdateBody(
            " Ana Maria Silva ", "001-234-567", "BOTH", null, 2500.75m, true, partner.Version));

        Assert.Equal(partner.Id, updated.Id);
        Assert.Equal("Ana Maria Silva", updated.Name);
        Assert.Equal("001234567", updated.TaxNumber);
        Assert.Equal("BOTH", updated.Type);
        Assert.Null(updated.Email);
        Assert.Equal(2500.75m, updated.CreditLimit);
        Assert.Equal(partner.CreatedAt, updated.CreatedAt);
        Assert.True(updated.UpdatedAt > partner.UpdatedAt);
        Assert.NotEqual(partner.Version, updated.Version);

        var stored = await GetStoredAsync(partner.Id);
        Assert.Equal(updated.Name, stored.Name);
        Assert.Equal(updated.Version, stored.Version);
        Assert.Equal(partner.CreatedAt, stored.CreatedAt);
    }

    [Fact]
    public async Task Update_keeping_own_tax_number_is_allowed()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");

        var updated = await UpdateAsync(partner, partner.ToUpdate() with { Name = "Ana S. Silva" });

        Assert.Equal("001234567", updated.TaxNumber);
        Assert.Equal("Ana S. Silva", updated.Name);
    }

    [Fact]
    public async Task Update_to_tax_number_of_another_partner_returns_conflict()
    {
        await CreateAsync("Bruno Costa", "100000002");
        var partner = await CreateAsync("Ana Silva", "001234567");

        var response = await PutAsync(partner.Id, partner.ToUpdate() with
        {
            Name = "Nome Alterado",
            TaxNumber = "100.000.002"
        });

        await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Equal(partner, await GetStoredAsync(partner.Id));
    }

    [Fact]
    public async Task Update_with_outdated_version_returns_conflict()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");
        var updated = await UpdateAsync(partner, partner.ToUpdate() with { Name = "Primeira Alteracao" });

        var response = await PutAsync(partner.Id, partner.ToUpdate() with { Name = "Segunda Alteracao" });

        await ReadProblemAsync(response, HttpStatusCode.Conflict);
        Assert.Equal(updated, await GetStoredAsync(partner.Id));
    }

    [Fact]
    public async Task Update_unknown_id_returns_not_found()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");

        var response = await PutAsync(Guid.NewGuid(), partner.ToUpdate());

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_requires_every_field()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");

        var response = await PutAsync(partner.Id, new { name = "Ana Maria" });

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equivalent(new[] { "taxNumber", "type", "creditLimit", "isActive", "version" }, problem.Errors!.Keys);
        Assert.Equal(partner, await GetStoredAsync(partner.Id));
    }

    [Fact]
    public async Task Update_ignores_id_and_dates_sent_by_the_client()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");

        var response = await PutAsync(partner.Id, new
        {
            id = Guid.NewGuid(),
            name = partner.Name,
            taxNumber = partner.TaxNumber,
            type = partner.Type,
            email = partner.Email,
            creditLimit = partner.CreditLimit,
            isActive = partner.IsActive,
            version = partner.Version,
            createdAt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            updatedAt = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await ReadAsync<PartnerDto>(response);
        Assert.Equal(partner.Id, updated.Id);
        Assert.Equal(partner.CreatedAt, updated.CreatedAt);
        Assert.True(updated.UpdatedAt > partner.UpdatedAt);
    }
}
