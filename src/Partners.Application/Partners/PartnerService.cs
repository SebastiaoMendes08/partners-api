using Partners.Application.Common;
using Partners.Application.Partners.Dtos;
using Partners.Core.Common;
using Partners.Core.Partners;

namespace Partners.Application.Partners;

public sealed class PartnerService(IPartnerRepository repository, TimeProvider timeProvider) : IPartnerService
{
    public async Task<Result<PartnerDto>> CreateAsync(CreatePartnerDto dto,
        CancellationToken cancellationToken = default)
    {
        var errors = new ValidationErrors();
        var data = PartnerValidator.Validate(dto, errors);
        if (data is null)
        {
            return errors.ToError();
        }

        var partner = Partner.Create(data, UtcNow());
        var saved = await repository.AddAsync(partner, cancellationToken);
        if (saved.IsFailure)
        {
            return saved.Error;
        }

        return PartnerDto.From(partner);
    }

    public async Task<Result<PartnerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var partner = await repository.GetByIdAsync(id, cancellationToken);
        if (partner is null)
        {
            return PartnerErrors.NotFound(id);
        }

        return PartnerDto.From(partner);
    }

    public async Task<Result<PagedResult<PartnerDto>>> ListAsync(ListPartnersQuery query,
        CancellationToken cancellationToken = default)
    {
        var criteria = query.ToCriteria();
        if (criteria.IsFailure)
        {
            return criteria.Error;
        }

        var page = await repository.ListAsync(criteria.Value, cancellationToken);
        return page.Map(PartnerDto.From);
    }

    public async Task<Result<PartnerDto>> UpdateAsync(Guid id, UpdatePartnerDto dto,
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

        if (data is null || dto.IsActive is not { } isActive || version is null)
        {
            return errors.ToError();
        }

        var partner = await repository.GetByIdAsync(id, cancellationToken);
        if (partner is null)
        {
            return PartnerErrors.NotFound(id);
        }

        partner.Update(data, isActive, UtcNow());
        var saved = await repository.UpdateAsync(partner, version, cancellationToken);
        if (saved.IsFailure)
        {
            return saved.Error;
        }

        return PartnerDto.From(partner);
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
