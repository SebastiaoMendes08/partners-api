using Microsoft.EntityFrameworkCore;
using Partners.Core.Partners;

namespace Partners.Infrastructure.Persistence;

public sealed class PartnersDbContext(DbContextOptions<PartnersDbContext> options) : DbContext(options)
{
    public DbSet<Partner> Partners => Set<Partner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PartnersDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }
}
