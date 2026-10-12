using Partners.Core.Errors;

namespace Partners.Core.Partners;

public sealed class PartnerService(IPartnerRepository repository, TimeProvider timeProvider)
{
    public async Task<Partner> CreateAsync(PartnerInput input, CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var data = PartnerValidator.Validate(input, errors);
        errors.ThrowIfAny();

        var partner = Partner.Create(data!, UtcNow());
        await repository.AddAsync(partner, cancellationToken);
        return partner;
    }

    public async Task<Partner> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new PartnerNotFoundException(id);

    public Task<PagedResult<Partner>> ListAsync(PartnerListInput input, CancellationToken cancellationToken = default) =>
        repository.ListAsync(PartnerFilter.Create(input), cancellationToken);

    public async Task<Partner> UpdateAsync(Guid id, PartnerUpdateInput input, CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var data = PartnerValidator.Validate(input, errors);

        if (input.IsActive is null)
        {
            errors.Add(nameof(input.IsActive), "IsActive is required.");
        }

        byte[]? version = null;
        if (string.IsNullOrWhiteSpace(input.Version))
        {
            errors.Add(nameof(input.Version), "Version is required.");
        }
        else if (!VersionToken.TryDecode(input.Version, out version))
        {
            errors.Add(nameof(input.Version), "Version is not valid. Use the value returned by the API.");
        }

        errors.ThrowIfAny();

        var partner = await GetAsync(id, cancellationToken);
        partner.Update(data!, input.IsActive!.Value, UtcNow());
        await repository.UpdateAsync(partner, version!, cancellationToken);
        return partner;
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
