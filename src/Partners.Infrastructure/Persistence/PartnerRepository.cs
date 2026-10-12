using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Partners.Core.Errors;
using Partners.Core.Partners;

namespace Partners.Infrastructure.Persistence;

public sealed class PartnerRepository(PartnersDbContext dbContext) : IPartnerRepository
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Partners.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResult<Partner>> ListAsync(PartnerFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Partners.AsNoTracking();

        if (!filter.IncludeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (filter.Types is { } types)
        {
            query = query.Where(p => types.Contains(p.Type));
        }

        if (filter.Search is { } search)
        {
            query = filter.TaxNumberSearch is { } taxNumber
                ? query.Where(p => p.Name.Contains(search) || p.TaxNumber.Contains(taxNumber))
                : query.Where(p => p.Name.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var offset = (long)(filter.Page - 1) * filter.PageSize;
        if (offset >= totalCount)
        {
            return new PagedResult<Partner>([], totalCount, filter.Page, filter.PageSize);
        }

        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((int)offset)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Partner>(items, totalCount, filter.Page, filter.PageSize);
    }

    public async Task AddAsync(Partner partner, CancellationToken cancellationToken = default)
    {
        dbContext.Partners.Add(partner);
        await SaveChangesAsync(partner, cancellationToken);
    }

    public async Task UpdateAsync(Partner partner, byte[] expectedVersion, CancellationToken cancellationToken = default)
    {
        dbContext.Entry(partner).Property(p => p.Version).OriginalValue = expectedVersion;
        await SaveChangesAsync(partner, cancellationToken);
    }

    private async Task SaveChangesAsync(Partner partner, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(partner.Id, ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException
                                           {
                                               Number: UniqueIndexViolation or UniqueConstraintViolation
                                           })
        {
            throw new DuplicateTaxNumberException(partner.TaxNumber, ex);
        }
    }
}
