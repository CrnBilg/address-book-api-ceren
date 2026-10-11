using AddressBook.Api.DTOs;

namespace AddressBook.Api.Services;

public interface IContactService
{
    Task<PagedResult<ContactDto>> GetAsync(ContactQueryParameters query, CancellationToken cancellationToken);
    Task<ContactDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ContactDto> CreateAsync(CreateContactDto request, CancellationToken cancellationToken);
    Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<ContactDto> MarkFavoriteAsync(Guid id, CancellationToken cancellationToken);
    Task<ContactDto> UnmarkFavoriteAsync(Guid id, CancellationToken cancellationToken);
}
