using System.Net;
using System.Text;
using Partners.IntegrationTests.Infrastructure;

namespace Partners.IntegrationTests;

public class ValidationTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task Credit_limit_with_more_than_two_decimals_is_rejected()
    {
        var response = await PostAsync(new
        {
            name = "Ana Silva",
            taxNumber = "001234567",
            type = "CUSTOMER",
            creditLimit = 10.999m
        });

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(["creditLimit"], problem.Errors!.Keys);
        Assert.Equal(0, await CountStoredAsync());
    }

    [Theory]
    [InlineData("00123456A")]
    [InlineData("ABC.DEF.GHI")]
    [InlineData("12345678")]
    [InlineData("")]
    public async Task Invalid_tax_number_is_rejected(string taxNumber)
    {
        var response = await PostAsync(new
        {
            name = "Ana Silva",
            taxNumber,
            type = "CUSTOMER",
            creditLimit = 0
        });

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(["taxNumber"], problem.Errors!.Keys);
        Assert.Equal(0, await CountStoredAsync());
    }

    [Fact]
    public async Task All_invalid_fields_are_reported()
    {
        var response = await PostAsync(new
        {
            name = " A ",
            taxNumber = "123",
            type = "OTHER",
            email = "not-an-email",
            creditLimit = 2_000_000
        });

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equivalent(new[] { "name", "taxNumber", "type", "email", "creditLimit" }, problem.Errors!.Keys);
    }

    [Theory]
    [InlineData("""{"name":"Ana Silva","taxNumber":"001234567","type":"CUSTOMER","creditLimit":"abc"}""")]
    [InlineData("""{"name":"Ana Silva","taxNumber":001234567,"type":"CUSTOMER","creditLimit":0}""")]
    [InlineData("""{"name":"Ana Silva",""")]
    [InlineData("")]
    public async Task Malformed_body_is_rejected(string json)
    {
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await Client.PostAsync(BaseUrl, content);

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.NotEmpty(problem.Errors!);
        Assert.Equal(0, await CountStoredAsync());
    }

    [Theory]
    [InlineData("not-base64!")]
    [InlineData("AAAA")]
    [InlineData("")]
    public async Task Malformed_version_is_rejected_without_changes(string version)
    {
        var partner = await CreateAsync("Ana Silva", "001234567");

        var response = await PutAsync(partner.Id, partner.ToUpdate() with { Name = "Outro Nome", Version = version });

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(["version"], problem.Errors!.Keys);
        Assert.Equal(partner, await GetStoredAsync(partner.Id));
    }

    [Fact]
    public async Task Invalid_update_does_not_change_the_partner()
    {
        var partner = await CreateAsync("Ana Silva", "001234567");

        var response = await PutAsync(partner.Id, partner.ToUpdate() with
        {
            Name = "Nome Valido",
            CreditLimit = 10.999m
        });

        await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.Equal(partner, await GetStoredAsync(partner.Id));
    }

    [Fact]
    public async Task Errors_do_not_expose_internal_details()
    {
        var response = await Client.PostAsync(BaseUrl,
            new StringContent("""{"creditLimit":"abc"}""", Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.DoesNotContain("System.", body);
        Assert.DoesNotContain("Exception", body);
        Assert.DoesNotContain(" at ", body);
    }
}
