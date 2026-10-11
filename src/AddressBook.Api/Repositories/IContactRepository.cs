using AddressBook.Api.Models;

namespace AddressBook.Api.Repositories;

public interface IContactRepository
{
    Task<IReadOnlyCollection<Contact>> GetAllAsync(CancellationToken cancellationToken);
    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> EmailExistsAsync(string email, Guid? excludedContactId, CancellationToken cancellationToken);
    Task AddAsync(Contact contact, CancellationToken cancellationToken);
    Task UpdateAsync(Contact contact, CancellationToken cancellationToken);
    Task DeleteAsync(Contact contact, CancellationToken cancellationToken);
}
