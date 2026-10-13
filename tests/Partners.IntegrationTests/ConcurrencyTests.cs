using System.Net;
using System.Net.Http.Json;
using Partners.Application.Partners;
using Partners.Core.Errors;
using Partners.Core.Partners;
using Partners.Infrastructure.Persistence;
using Partners.IntegrationTests.Infrastructure;

namespace Partners.IntegrationTests;

public class ConcurrencyTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Concurrent_creates_with_same_tax_number_store_a_single_partner()
    {
        using var otherClient = Fixture.Factory.CreateClient();

        var responses = await Task.WhenAll(
            PostAsync(new { name = "Ana Silva", taxNumber = "001.234.567", type = "CUSTOMER", creditLimit = 0 }),
            otherClient.PostAsJsonAsync(BaseUrl,
                new { name = "Ana Costa", taxNumber = "001 234 567", type = "SUPPLIER", creditLimit = 0 }));

        Assert.Equivalent(new[] { HttpStatusCode.Created, HttpStatusCode.Conflict },
            responses.Select(r => r.StatusCode));
        Assert.Equal(1, await CountStoredAsync());
    }

    [Fact]
    public async Task Concurrent_updates_with_same_version_apply_only_one()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");
        using var otherClient = Fixture.Factory.CreateClient();

        var responses = await Task.WhenAll(
            PutAsync(partner.Id, partner.ToUpdate() with { Name = "Alteracao A" }),
            otherClient.PutAsJsonAsync($"{BaseUrl}/{partner.Id}", partner.ToUpdate() with { Name = "Alteracao B" }));

        Assert.Equivalent(new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }, responses.Select(r => r.StatusCode));

        var winner = await ReadAsync<PartnerDto>(responses.Single(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(winner, await GetStoredAsync(partner.Id));
    }

    [Fact]
    public async Task Independent_contexts_cannot_insert_the_same_tax_number()
    {
        await using var first = Fixture.CreateDbContext();
        await using var second = Fixture.CreateDbContext();
        var data = new PartnerData("Ana Silva", "001234567", PartnerType.Customer, null, 0m);

        var results = await RunTogetherAsync(
            () => new PartnerRepository(first).AddAsync(Partner.Create(data, DateTime.UtcNow)),
            () => new PartnerRepository(second).AddAsync(Partner.Create(data, DateTime.UtcNow)));

        Assert.Single(results, r => r is null);
        Assert.Single(results, r => r is DuplicateTaxNumberException);
        Assert.Equal(1, await CountStoredAsync());
    }

    [Fact]
    public async Task Independent_contexts_cannot_update_with_the_same_version()
    {
        var created = await CreateAsync("Ana Silva", "001234567");
        Assert.True(VersionToken.TryDecode(created.Version, out var version));

        await using var first = Fixture.CreateDbContext();
        await using var second = Fixture.CreateDbContext();
        var firstRepository = new PartnerRepository(first);
        var secondRepository = new PartnerRepository(second);
        var firstCopy = (await firstRepository.GetByIdAsync(created.Id))!;
        var secondCopy = (await secondRepository.GetByIdAsync(created.Id))!;

        firstCopy.Update(new PartnerData("Alteracao A", "001234567", PartnerType.Customer, null, 0m), true,
            DateTime.UtcNow);
        secondCopy.Update(new PartnerData("Alteracao B", "001234567", PartnerType.Customer, null, 0m), true,
            DateTime.UtcNow);

        var results = await RunTogetherAsync(
            () => firstRepository.UpdateAsync(firstCopy, version),
            () => secondRepository.UpdateAsync(secondCopy, version));

        Assert.Single(results, r => r is null);
        Assert.Single(results, r => r is ConcurrencyConflictException);
    }

    private static async Task<Exception?[]> RunTogetherAsync(params Func<Task>[] actions)
    {
        using var start = new Barrier(actions.Length);

        var tasks = actions.Select(action => Task.Run(async () =>
        {
            start.SignalAndWait();
            try
            {
                await action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }));

        return await Task.WhenAll(tasks);
    }
}
