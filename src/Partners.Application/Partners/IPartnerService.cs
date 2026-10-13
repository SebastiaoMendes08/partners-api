using Partners.Application.Partners.Dtos;
using Partners.Core.Common;

namespace Partners.Application.Partners;

public interface IPartnerService
{
    Task<PartnerDto> CreateAsync(CreatePartnerDto dto, CancellationToken cancellationToken = default);

    Task<PartnerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<PartnerDto>> ListAsync(ListPartnersQuery query, CancellationToken cancellationToken = default);

    Task<PartnerDto> UpdateAsync(Guid id, UpdatePartnerDto dto, CancellationToken cancellationToken = default);
}
