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

    /// <summary>
    /// Lists contacts with optional search, tag filtering, favorite filtering, sorting, and pagination.
    /// </summary>
    /// <remarks>
    /// Search matches name, email, phone, company, postal address, notes, and tags.
    /// Page numbers are one-based (1-100000) and page size is limited to 1-100.
    ///
    /// Example: `GET /api/v1/contacts?search=istanbul&amp;sortBy=city&amp;sortOrder=asc&amp;pageNumber=1&amp;pageSize=10`
    /// </remarks>
    /// <response code="200">A page of contacts matching the query.</response>
    /// <response code="400">A query parameter is invalid, for example an unknown sortBy value or an out-of-range page size.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetContacts(
        [FromQuery] ContactQueryParameters query,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.GetAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single contact by its unique identifier.
    /// </summary>
    /// <param name="id">The contact identifier (GUID).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The requested contact.</response>
    /// <response code="404">No contact exists with the given identifier.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> GetContact(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new address book contact.
    /// </summary>
    /// <remarks>
    /// First name and last name are required. At least one contact method, email or phone number, is required.
    /// Postal address fields are optional.
    /// </remarks>
    /// <param name="request">The contact to create.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="201">The contact was created. The Location header points to the new resource.</response>
    /// <response code="400">The request body failed validation.</response>
    /// <response code="409">Another contact already uses the same email address.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> CreateContact(
        [FromBody] CreateContactDto request,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetContact), new { id = result.Id }, result);
    }

    /// <summary>
    /// Replaces the editable fields of an existing contact.
    /// </summary>
    /// <remarks>
    /// PUT uses full replacement semantics for editable fields. Omitted optional fields are cleared or reset to their defaults.
    /// </remarks>
    /// <param name="id">The contact identifier (GUID).</param>
    /// <param name="request">The complete new state of the contact.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The updated contact.</response>
    /// <response code="400">The request body failed validation.</response>
    /// <response code="404">No contact exists with the given identifier.</response>
    /// <response code="409">Another contact already uses the same email address.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> UpdateContact(
        Guid id,
        [FromBody] UpdateContactDto request,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes an existing contact.
    /// </summary>
    /// <param name="id">The contact identifier (GUID).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="204">The contact was deleted.</response>
    /// <response code="404">No contact exists with the given identifier.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken cancellationToken)
    {
        await _contactService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks a contact as favorite.
    /// </summary>
    /// <remarks>
    /// The operation is idempotent. Marking an already favorite contact still returns the contact with isFavorite set to true.
    /// </remarks>
    /// <param name="id">The contact identifier (GUID).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The contact, now marked as favorite.</response>
    /// <response code="404">No contact exists with the given identifier.</response>
    [HttpPatch("{id:guid}/favorite")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> MarkFavorite(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.MarkFavoriteAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Removes a contact from favorites.
    /// </summary>
    /// <remarks>
    /// The operation is idempotent. Unmarking a non-favorite contact still returns the contact with isFavorite set to false.
    /// </remarks>
    /// <param name="id">The contact identifier (GUID).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <response code="200">The contact, no longer marked as favorite.</response>
    /// <response code="404">No contact exists with the given identifier.</response>
    [HttpPatch("{id:guid}/unfavorite")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> UnmarkFavorite(Guid id, CancellationToken cancellationToken)
    {
        var result = await _contactService.UnmarkFavoriteAsync(id, cancellationToken);
        return Ok(result);
    }
}
