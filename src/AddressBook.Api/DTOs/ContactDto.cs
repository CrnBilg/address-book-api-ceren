namespace AddressBook.Api.DTOs;

public sealed record ContactDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? AddressLine,
    string? District,
    string? City,
    string? PostalCode,
    string? Country,
    string? Notes,
    bool IsFavorite,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
