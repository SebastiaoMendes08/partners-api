using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace Partners.IntegrationTests.Infrastructure;

[Collection(ApiCollection.Name)]
public abstract class ApiTestBase(ApiFixture fixture) : IAsyncLifetime
{
    protected const string BaseUrl = "/api/partners";

    protected ApiFixture Fixture { get; } = fixture;
    protected HttpClient Client { get; } = fixture.Factory.CreateClient();

    public Task InitializeAsync() => Fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }

    protected Task<HttpResponseMessage> PostAsync(object body) => Client.PostAsJsonAsync(BaseUrl, body);

    protected Task<HttpResponseMessage> PutAsync(Guid id, object body) => Client.PutAsJsonAsync($"{BaseUrl}/{id}", body);

    protected Task<HttpResponseMessage> GetAsync(Guid id) => Client.GetAsync($"{BaseUrl}/{id}");

    protected async Task<PartnerDto> CreateAsync(string name, string taxNumber, string type = "CUSTOMER",
        string? email = null, decimal creditLimit = 0m)
    {
        var response = await PostAsync(new { name, taxNumber, type, email, creditLimit });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await ReadAsync<PartnerDto>(response);
    }

    protected async Task<PartnerDto> UpdateAsync(PartnerDto partner, UpdateBody body)
    {
        var response = await PutAsync(partner.Id, body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadAsync<PartnerDto>(response);
    }

    protected Task<PartnerDto> SetActiveAsync(PartnerDto partner, bool isActive) =>
        UpdateAsync(partner, partner.ToUpdate() with { IsActive = isActive });

    protected async Task<PageDto> ListAsync(string query = "")
    {
        var response = await Client.GetAsync($"{BaseUrl}{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadAsync<PageDto>(response);
    }

    protected async Task<PartnerDto> GetStoredAsync(Guid id)
    {
        var response = await GetAsync(id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await ReadAsync<PartnerDto>(response);
    }

    protected async Task<int> CountStoredAsync()
    {
        await using var dbContext = Fixture.CreateDbContext();
        return await dbContext.Partners.CountAsync();
    }

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException("Empty response body.");

    protected static async Task<ProblemDto> ReadProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await ReadAsync<ProblemDto>(response);
    }
}
