using Microsoft.AspNetCore.Mvc;
using Partners.Application.Partners;
using Partners.Application.Partners.Dtos;
using Partners.Core.Common;

namespace Partners.Api.Controllers;

[ApiController]
[Route("api/partners")]
public sealed class PartnersController(IPartnerService partnerService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PartnerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartnerDto>> Create(CreatePartnerDto request, CancellationToken cancellationToken)
    {
        var partner = await partnerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = partner.Id }, partner);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PartnerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerDto>> GetById(Guid id, CancellationToken cancellationToken) =>
        await partnerService.GetByIdAsync(id, cancellationToken);

    [HttpGet]
    [ProducesResponseType<PagedResult<PartnerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PartnerDto>>> List([FromQuery] ListPartnersQuery query,
        CancellationToken cancellationToken) =>
        await partnerService.ListAsync(query, cancellationToken);

    [HttpPut("{id}")]
    [ProducesResponseType<PartnerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartnerDto>> Update(Guid id, UpdatePartnerDto request,
        CancellationToken cancellationToken) =>
        await partnerService.UpdateAsync(id, request, cancellationToken);
}
