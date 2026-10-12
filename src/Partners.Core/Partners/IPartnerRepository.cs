namespace Partners.Core.Partners;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Partner>> ListAsync(PartnerFilter filter, CancellationToken cancellationToken = default);

    Task AddAsync(Partner partner, CancellationToken cancellationToken = default);

    Task UpdateAsync(Partner partner, byte[] expectedVersion, CancellationToken cancellationToken = default);
}
