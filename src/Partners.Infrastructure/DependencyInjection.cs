using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Partners.Core.Partners;
using Partners.Infrastructure.Persistence;

namespace Partners.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Partners";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<PartnersDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
                                       .GetConnectionString(ConnectionStringName)
                                   ?? throw new InvalidOperationException(
                                       $"Connection string '{ConnectionStringName}' is not configured.");

            options.UseSqlServer(connectionString);

            // Duplicate tax numbers and version conflicts are expected outcomes (409),
            // handled by the repository, so they should not be logged as errors.
            options.ConfigureWarnings(warnings => warnings.Log(
                (CoreEventId.SaveChangesFailed, LogLevel.Debug),
                (RelationalEventId.CommandError, LogLevel.Debug)));
        });

        services.AddScoped<IPartnerRepository, PartnerRepository>();

        return services;
    }
}
