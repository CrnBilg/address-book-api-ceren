using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AddressBook.Api.DTOs;

public enum ContactSortField
{
    FirstName,
    LastName,
    Email,
    Company,
    City,
    Country,
    CreatedAt,
    UpdatedAt
}

public enum SortDirection
{
    Asc,
    Desc
}

public sealed class ContactQueryParameters
{
    /// <summary>
    /// Case-insensitive text search across name, email, phone, company, postal address, notes, and tags.
    /// </summary>
    [FromQuery(Name = "search")]
    public string? Search { get; set; }

    /// <summary>
    /// Filters contacts that contain the given tag, case-insensitively.
    /// </summary>
    [FromQuery(Name = "tag")]
    public string? Tag { get; set; }

    /// <summary>
    /// Filters contacts by favorite state when provided.
    /// </summary>
    [FromQuery(Name = "isFavorite")]
    public bool? IsFavorite { get; set; }

    /// <summary>
    /// Field used for sorting the result set.
    /// </summary>
    [FromQuery(Name = "sortBy")]
    public ContactSortField SortBy { get; set; } = ContactSortField.LastName;

    /// <summary>
    /// Sort direction for the selected field.
    /// </summary>
    [FromQuery(Name = "sortOrder")]
    public SortDirection SortOrder { get; set; } = SortDirection.Asc;

    /// <summary>
    /// One-based page number. The maximum value is limited to prevent pagination overflow.
    /// </summary>
    [Range(1, 100_000)]
    [FromQuery(Name = "pageNumber")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of contacts returned per page.
    /// </summary>
    [Range(1, 100)]
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; set; } = 10;
}
