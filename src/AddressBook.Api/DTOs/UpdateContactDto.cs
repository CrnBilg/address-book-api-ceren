using System.ComponentModel.DataAnnotations;

namespace AddressBook.Api.DTOs;

public sealed class UpdateContactDto : IValidatableObject
{
    /// <summary>
    /// Given name of the contact.
    /// </summary>
    /// <example>Ceren</example>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Family name of the contact.
    /// </summary>
    /// <example>Bilgenoglu</example>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email address. Must be unique (case-insensitive). Email or phone number is required.
    /// </summary>
    /// <example>ceren@example.com</example>
    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }

    /// <summary>
    /// Phone number with at least 7 digits. Email or phone number is required.
    /// </summary>
    /// <example>+905551112233</example>
    [Phone]
    [StringLength(40)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Company or organization name.
    /// </summary>
    /// <example>SE4458</example>
    [StringLength(120)]
    public string? Company { get; set; }

    /// <summary>
    /// Street address line.
    /// </summary>
    /// <example>Example Street 42</example>
    [StringLength(200)]
    public string? AddressLine { get; set; }

    /// <summary>
    /// District or neighborhood.
    /// </summary>
    /// <example>Kadikoy</example>
    [StringLength(80)]
    public string? District { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    /// <example>Istanbul</example>
    [StringLength(80)]
    public string? City { get; set; }

    /// <summary>
    /// Postal code.
    /// </summary>
    /// <example>34710</example>
    [StringLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country name.
    /// </summary>
    /// <example>Turkiye</example>
    [StringLength(80)]
    public string? Country { get; set; }

    /// <summary>
    /// Free-text notes about the contact.
    /// </summary>
    /// <example>Assignment contact</example>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Whether the contact is marked as favorite.
    /// </summary>
    /// <example>false</example>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// Up to 10 tags, each at most 40 characters.
    /// </summary>
    /// <example>["school", "architecture"]</example>
    public List<string> Tags { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
        {
            yield return new ValidationResult(
                "At least one contact method is required.",
                [nameof(Email), nameof(PhoneNumber)]);
        }

        Tags ??= [];

        if (!string.IsNullOrWhiteSpace(Email) && !HasDottedEmailDomain(Email))
        {
            yield return new ValidationResult("Email domain must contain a dot.", [nameof(Email)]);
        }

        if (!string.IsNullOrWhiteSpace(PhoneNumber) && CountDigits(PhoneNumber) < 7)
        {
            yield return new ValidationResult("Phone number must contain at least 7 digits.", [nameof(PhoneNumber)]);
        }

        if (Tags.Count > 10)
        {
            yield return new ValidationResult("A contact can have at most 10 tags.", [nameof(Tags)]);
        }

        foreach (var tag in Tags)
        {
            if (string.IsNullOrWhiteSpace(tag) || tag.Length > 40)
            {
                yield return new ValidationResult("Tags must be non-empty and at most 40 characters.", [nameof(Tags)]);
                yield break;
            }
        }
    }

    private static bool HasDottedEmailDomain(string email)
    {
        var atIndex = email.LastIndexOf('@');
        return atIndex > 0 && email[(atIndex + 1)..].Contains('.');
    }

    private static int CountDigits(string value)
    {
        return value.Count(char.IsDigit);
    }
}
