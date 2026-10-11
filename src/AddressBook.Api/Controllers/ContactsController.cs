using AddressBook.Api.DTOs;
using AddressBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AddressBook.Api.Controllers;

[ApiController]
[Route("api/v1/contacts")]
[Produces("application/json")]
public sealed class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ContactDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetContacts(
        [FromQuery] ContactQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.GetAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> GetContact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> CreateContact(
        [FromBody] CreateContactDto request,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetContact), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> UpdateContact(
        Guid id,
        [FromBody] UpdateContactDto request,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken cancellationToken)
    {
        await _contactService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/favorite")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> MarkFavorite(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.MarkFavoriteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/unfavorite")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> UnmarkFavorite(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.UnmarkFavoriteAsync(id, cancellationToken);
        return Ok(result);
    }
}
