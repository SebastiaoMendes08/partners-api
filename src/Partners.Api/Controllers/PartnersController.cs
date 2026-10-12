using Microsoft.AspNetCore.Mvc;
using Partners.Api.Contracts;
using Partners.Core.Partners;

namespace Partners.Api.Controllers;

[ApiController]
[Route("api/partners")]
[Produces("application/json")]
public sealed class PartnersController(PartnerService partnerService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartnerResponse>> Create(CreatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var partner = await partnerService.CreateAsync(request.ToInput(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = partner.Id }, PartnerResponse.From(partner));
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var partner = await partnerService.GetAsync(id, cancellationToken);
        return PartnerResponse.From(partner);
    }

    [HttpGet]
    [ProducesResponseType<PagedResponse<PartnerResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<PartnerResponse>>> List([FromQuery] ListPartnersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await partnerService.ListAsync(request.ToInput(), cancellationToken);
        return new PagedResponse<PartnerResponse>(
            result.Items.Select(PartnerResponse.From).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<PartnerResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartnerResponse>> Update(Guid id, UpdatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var partner = await partnerService.UpdateAsync(id, request.ToInput(), cancellationToken);
        return PartnerResponse.From(partner);
    }
}
