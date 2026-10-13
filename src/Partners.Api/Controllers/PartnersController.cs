using Microsoft.AspNetCore.Mvc;
using Partners.Application.Partners;
using Partners.Application.Partners.Dtos;
using Partners.Core.Common;

namespace Partners.Api.Controllers;

[Route("api/partners")]
public sealed class PartnersController(IPartnerService partnerService) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<PartnerDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartnerDto>> Create(CreatePartnerDto request, CancellationToken cancellationToken)
    {
        var result = await partnerService.CreateAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : Problem(result.Error);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<PartnerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PartnerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await partnerService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<PartnerDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PartnerDto>>> List([FromQuery] ListPartnersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await partnerService.ListAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }

    [HttpPut("{id}")]
    [ProducesResponseType<PartnerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PartnerDto>> Update(Guid id, UpdatePartnerDto request,
        CancellationToken cancellationToken)
    {
        var result = await partnerService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error);
    }
}
