using Partners.Core.Common;

namespace Partners.Core.Partners;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Partner>> ListAsync(PartnerSearchCriteria criteria, CancellationToken cancellationToken = default);

    Task<Result> AddAsync(Partner partner, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(Partner partner, byte[] expectedVersion, CancellationToken cancellationToken = default);
}
