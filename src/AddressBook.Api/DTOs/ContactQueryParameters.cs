using System.ComponentModel.DataAnnotations;

namespace AddressBook.Api.DTOs;

public sealed class ContactQueryParameters
{
    public string? Search { get; set; }
    public string? Tag { get; set; }
    public bool? IsFavorite { get; set; }
    public string? SortBy { get; set; } = "lastName";
    public string? SortOrder { get; set; } = "asc";

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
