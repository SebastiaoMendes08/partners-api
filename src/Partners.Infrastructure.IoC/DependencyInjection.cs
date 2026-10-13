using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Partners.Application.Partners;
using Partners.Core.Partners;
using Partners.Infrastructure.Persistence;

namespace Partners.Infrastructure.IoC;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Partners";

    public static IServiceCollection AddPartners(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);

        services.AddScoped<IPartnerService, PartnerService>();

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
