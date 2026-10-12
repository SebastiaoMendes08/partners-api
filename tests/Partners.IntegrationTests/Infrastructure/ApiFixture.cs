using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Partners.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace Partners.IntegrationTests.Infrastructure;

public sealed class ApiFixture : IAsyncLifetime
{
    public const string ServerVariable = "PARTNERS_TEST_SQLSERVER";
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";

    private MsSqlContainer? _container;

    public string ConnectionString { get; private set; } = string.Empty;
    public PartnersApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var serverConnectionString = Environment.GetEnvironmentVariable(ServerVariable);
        if (string.IsNullOrWhiteSpace(serverConnectionString))
        {
            _container = new MsSqlBuilder(SqlServerImage).Build();
            await _container.StartAsync();
            serverConnectionString = _container.GetConnectionString();
        }

        ConnectionString = new SqlConnectionStringBuilder(serverConnectionString)
        {
            InitialCatalog = $"PartnersTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        await using (var dbContext = CreateDbContext())
        {
            await dbContext.Database.MigrateAsync();
        }

        Factory = new PartnersApiFactory(ConnectionString);
    }

    public PartnersDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<PartnersDbContext>().UseSqlServer(ConnectionString).Options);

    public async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Partners.ExecuteDeleteAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();

        if (_container is not null)
        {
            await _container.DisposeAsync();
            return;
        }

        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
