namespace AddressBook.Api.DTOs;

public sealed record ContactDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? Notes,
    bool IsFavorite,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
