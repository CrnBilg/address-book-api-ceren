using AddressBook.Api.DTOs;
using AddressBook.Api.Exceptions;
using AddressBook.Api.Models;
using AddressBook.Api.Repositories;

namespace AddressBook.Api.Services;

public sealed class ContactService : IContactService
{
    private static readonly HashSet<string> AllowedSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "firstName",
        "lastName",
        "email",
        "company",
        "createdAt",
        "updatedAt"
    };

    private readonly IContactRepository _repository;

    public ContactService(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ContactDto>> GetAsync(ContactQueryParameters query, CancellationToken cancellationToken)
    {
        var contacts = await _repository.GetAllAsync(cancellationToken);
        var filtered = contacts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            filtered = filtered.Where(contact =>
                Contains(contact.FirstName, term) ||
                Contains(contact.LastName, term) ||
                Contains(contact.Email, term) ||
                Contains(contact.PhoneNumber, term) ||
                Contains(contact.Company, term) ||
                Contains(contact.Notes, term) ||
                contact.Tags.Any(tag => Contains(tag, term)));
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim();
            filtered = filtered.Where(contact =>
                contact.Tags.Any(contactTag => string.Equals(contactTag, tag, StringComparison.OrdinalIgnoreCase)));
        }

        if (query.IsFavorite.HasValue)
        {
            filtered = filtered.Where(contact => contact.IsFavorite == query.IsFavorite.Value);
        }

        filtered = ApplySorting(filtered, query.SortBy, query.SortOrder);

        var totalCount = filtered.Count();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        var items = filtered
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ToDto)
            .ToList();

        return new PagedResult<ContactDto>(items, query.PageNumber, query.PageSize, totalCount, totalPages);
    }

    public async Task<ContactDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await GetExistingContactAsync(id, cancellationToken);
        return ToDto(contact);
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto request, CancellationToken cancellationToken)
    {
        await EnsureEmailIsUniqueAsync(request.Email, null, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = NormalizeOptional(request.Email),
            PhoneNumber = NormalizeOptional(request.PhoneNumber),
            Company = NormalizeOptional(request.Company),
            Notes = NormalizeOptional(request.Notes),
            IsFavorite = request.IsFavorite,
            Tags = NormalizeTags(request.Tags),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddAsync(contact, cancellationToken);
        return ToDto(contact);
    }

    public async Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto request, CancellationToken cancellationToken)
    {
        var contact = await GetExistingContactAsync(id, cancellationToken);
        await EnsureEmailIsUniqueAsync(request.Email, id, cancellationToken);

        contact.FirstName = request.FirstName.Trim();
        contact.LastName = request.LastName.Trim();
        contact.Email = NormalizeOptional(request.Email);
        contact.PhoneNumber = NormalizeOptional(request.PhoneNumber);
        contact.Company = NormalizeOptional(request.Company);
        contact.Notes = NormalizeOptional(request.Notes);
        contact.IsFavorite = request.IsFavorite;
        contact.Tags = NormalizeTags(request.Tags);
        contact.UpdatedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(contact, cancellationToken);
        return ToDto(contact);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await GetExistingContactAsync(id, cancellationToken);
        await _repository.DeleteAsync(contact, cancellationToken);
    }

    public async Task<ContactDto> MarkFavoriteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await GetExistingContactAsync(id, cancellationToken);
        contact.IsFavorite = true;
        contact.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(contact, cancellationToken);
        return ToDto(contact);
    }

    public async Task<ContactDto> UnmarkFavoriteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await GetExistingContactAsync(id, cancellationToken);
        contact.IsFavorite = false;
        contact.UpdatedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(contact, cancellationToken);
        return ToDto(contact);
    }

    private async Task<Contact> GetExistingContactAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetByIdAsync(id, cancellationToken);
        return contact ?? throw new NotFoundException($"Contact '{id}' was not found.");
    }

    private async Task EnsureEmailIsUniqueAsync(string? email, Guid? excludedContactId, CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeOptional(email);
        if (normalizedEmail is null)
        {
            return;
        }

        if (await _repository.EmailExistsAsync(normalizedEmail, excludedContactId, cancellationToken))
        {
            throw new ConflictException($"Contact email '{normalizedEmail}' is already in use.");
        }
    }

    private static IEnumerable<Contact> ApplySorting(IEnumerable<Contact> contacts, string? sortBy, string? sortOrder)
    {
        var field = string.IsNullOrWhiteSpace(sortBy) ? "lastName" : sortBy.Trim();
        if (!AllowedSortFields.Contains(field))
        {
            field = "lastName";
        }

        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        return field.ToLowerInvariant() switch
        {
            "firstname" => descending ? contacts.OrderByDescending(c => c.FirstName) : contacts.OrderBy(c => c.FirstName),
            "email" => descending ? contacts.OrderByDescending(c => c.Email) : contacts.OrderBy(c => c.Email),
            "company" => descending ? contacts.OrderByDescending(c => c.Company) : contacts.OrderBy(c => c.Company),
            "createdat" => descending ? contacts.OrderByDescending(c => c.CreatedAt) : contacts.OrderBy(c => c.CreatedAt),
            "updatedat" => descending ? contacts.OrderByDescending(c => c.UpdatedAt) : contacts.OrderBy(c => c.UpdatedAt),
            _ => descending ? contacts.OrderByDescending(c => c.LastName) : contacts.OrderBy(c => c.LastName)
        };
    }

    private static ContactDto ToDto(Contact contact)
    {
        return new ContactDto(
            contact.Id,
            contact.FirstName,
            contact.LastName,
            contact.Email,
            contact.PhoneNumber,
            contact.Company,
            contact.Notes,
            contact.IsFavorite,
            contact.Tags.OrderBy(tag => tag).ToList(),
            contact.CreatedAt,
            contact.UpdatedAt);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<string> NormalizeTags(IEnumerable<string> tags)
    {
        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(tag => tag)
            .ToList();
    }

    private static bool Contains(string? source, string term)
    {
        return source?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;
    }
}
