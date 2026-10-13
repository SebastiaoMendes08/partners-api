using Partners.Application.Common;
using Partners.Application.Partners.Dtos;
using Partners.Core.Common;
using Partners.Core.Errors;
using Partners.Core.Partners;

namespace Partners.Application.Partners;

public sealed class PartnerService(IPartnerRepository repository, TimeProvider timeProvider) : IPartnerService
{
    public async Task<PartnerDto> CreateAsync(CreatePartnerDto dto, CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var data = PartnerValidator.Validate(dto, errors);
        errors.ThrowIfAny();

        var partner = Partner.Create(data!, UtcNow());
        await repository.AddAsync(partner, cancellationToken);
        return PartnerDto.From(partner);
    }

    public async Task<PartnerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        PartnerDto.From(await FindAsync(id, cancellationToken));

    public async Task<PagedResult<PartnerDto>> ListAsync(ListPartnersQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await repository.ListAsync(query.ToCriteria(), cancellationToken);
        return page.Map(PartnerDto.From);
    }

    public async Task<PartnerDto> UpdateAsync(Guid id, UpdatePartnerDto dto,
        CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var data = PartnerValidator.Validate(dto, errors);

        if (dto.IsActive is null)
        {
            errors.Add(nameof(dto.IsActive), "IsActive is required.");
        }

        byte[]? version = null;
        if (string.IsNullOrWhiteSpace(dto.Version))
        {
            errors.Add(nameof(dto.Version), "Version is required.");
        }
        else if (!VersionToken.TryDecode(dto.Version, out version))
        {
            errors.Add(nameof(dto.Version), "Version is not valid. Use the value returned by the API.");
        }

        errors.ThrowIfAny();

        var partner = await FindAsync(id, cancellationToken);
        partner.Update(data!, dto.IsActive!.Value, UtcNow());
        await repository.UpdateAsync(partner, version!, cancellationToken);
        return PartnerDto.From(partner);
    }

    private async Task<Partner> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await repository.GetByIdAsync(id, cancellationToken) ?? throw new PartnerNotFoundException(id);

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
