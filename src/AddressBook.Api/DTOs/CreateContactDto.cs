using System.ComponentModel.DataAnnotations;

namespace AddressBook.Api.DTOs;

public sealed class CreateContactDto : IValidatableObject
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(40)]
    public string? PhoneNumber { get; set; }

    [StringLength(120)]
    public string? Company { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsFavorite { get; set; }

    public List<string> Tags { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(PhoneNumber))
        {
            yield return new ValidationResult(
                "At least one contact method is required.",
                [nameof(Email), nameof(PhoneNumber)]);
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
}
