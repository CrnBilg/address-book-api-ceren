using AddressBook.Api.Data;
using AddressBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Api.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly AddressBookDbContext _dbContext;

    public ContactRepository(AddressBookDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<Contact>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Contacts.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Contacts.FirstOrDefaultAsync(contact => contact.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludedContactId, CancellationToken cancellationToken)
    {
        return await _dbContext.Contacts.AnyAsync(contact =>
            contact.Email != null &&
            contact.Email.ToLower() == email.ToLower() &&
            (!excludedContactId.HasValue || contact.Id != excludedContactId.Value),
            cancellationToken);
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken)
    {
        await _dbContext.Contacts.AddAsync(contact, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Contact contact, CancellationToken cancellationToken)
    {
        _dbContext.Contacts.Update(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Contact contact, CancellationToken cancellationToken)
    {
        _dbContext.Contacts.Remove(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
