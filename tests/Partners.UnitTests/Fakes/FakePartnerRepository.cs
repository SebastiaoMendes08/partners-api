using Partners.Core.Common;
using Partners.Core.Partners;

namespace Partners.UnitTests.Fakes;

internal sealed class FakePartnerRepository : IPartnerRepository
{
    public List<Partner> Partners { get; } = [];
    public Result SaveResult { get; set; } = Result.Success();
    public int SaveCalls { get; private set; }
    public int ListCalls { get; private set; }

    public Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Partners.FirstOrDefault(p => p.Id == id));

    public Task<PagedResult<Partner>> ListAsync(PartnerSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        ListCalls++;
        return Task.FromResult(new PagedResult<Partner>(Partners, Partners.Count, criteria.Page, criteria.PageSize));
    }

    public Task<Result> AddAsync(Partner partner, CancellationToken cancellationToken = default)
    {
        SaveCalls++;
        if (SaveResult.IsSuccess)
        {
            Partners.Add(partner);
        }

        return Task.FromResult(SaveResult);
    }

    public Task<Result> UpdateAsync(Partner partner, byte[] expectedVersion,
        CancellationToken cancellationToken = default)
    {
        SaveCalls++;
        return Task.FromResult(SaveResult);
    }
}
