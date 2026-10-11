using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AddressBook.Api.Data;
using AddressBook.Api.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AddressBook.Api.Tests;

public sealed class ContactApiTests : IClassFixture<ContactApiFactory>
{
    private readonly ContactApiFactory _factory;
    private readonly HttpClient _client;

    public ContactApiTests(ContactApiFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateContact_ReturnsCreatedContactWithLocationHeader()
    {
        var response = await CreateContactAsync("Ceren", "Bilgenoglu", "ceren@example.com", tags: ["school", "architecture"]);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var contact = await response.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(contact);
        Assert.Equal("Ceren", contact.FirstName);
        Assert.Equal("Bilgenoglu", contact.LastName);
        Assert.Equal("ceren@example.com", contact.Email);
        Assert.Contains("school", contact.Tags);
    }

    [Fact]
    public async Task CreateAndRetrieveContact_PersistsOptionalPostalAddress()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/contacts", new CreateContactDto
        {
            FirstName = "Address",
            LastName = "Owner",
            Email = "address.owner@example.com",
            AddressLine = "Example Street 42",
            District = "Kadikoy",
            City = "Istanbul",
            PostalCode = "34710",
            Country = "Turkiye",
            Tags = ["home"]
        });

        var created = await response.Content.ReadFromJsonAsync<ContactDto>();
        var getResponse = await _client.GetAsync($"/api/v1/contacts/{created!.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<ContactDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(retrieved);
        Assert.Equal("Example Street 42", retrieved.AddressLine);
        Assert.Equal("Kadikoy", retrieved.District);
        Assert.Equal("Istanbul", retrieved.City);
        Assert.Equal("34710", retrieved.PostalCode);
        Assert.Equal("Turkiye", retrieved.Country);
    }

    [Fact]
    public async Task GetContactById_ReturnsExistingContact()
    {
        var created = await CreateAndReadContactAsync("Ada", "Lovelace", "ada@example.com");

        var response = await _client.GetAsync($"/api/v1/contacts/{created.Id}");
        var contact = await response.Content.ReadFromJsonAsync<ContactDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(contact);
        Assert.Equal(created.Id, contact.Id);
    }

    [Fact]
    public async Task GetContacts_ReturnsPagedContacts()
    {
        await CreateAndReadContactAsync("Ada", "Lovelace", "ada@example.com");
        await CreateAndReadContactAsync("Grace", "Hopper", "grace@example.com");
        await CreateAndReadContactAsync("Martin", "Fowler", "martin@example.com");

        var result = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?pageNumber=2&pageSize=1&sortBy=firstName&sortOrder=asc");

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.PageNumber);
        Assert.Single(result.Items);
        Assert.Equal("Grace", result.Items.Single().FirstName);
    }

    [Fact]
    public async Task UpdateContact_ReplacesEditableFields()
    {
        var created = await CreateAndReadContactAsync("Old", "Name", "old@example.com");

        var response = await _client.PutAsJsonAsync($"/api/v1/contacts/{created.Id}", new UpdateContactDto
        {
            FirstName = "New",
            LastName = "Name",
            Email = "new@example.com",
            PhoneNumber = "+905551112233",
            Company = "SE4458",
            AddressLine = "Updated Street 10",
            District = "Besiktas",
            City = "Istanbul",
            PostalCode = "34353",
            Country = "Turkiye",
            Notes = "Updated",
            IsFavorite = true,
            Tags = ["updated"]
        });
        var updated = await response.Content.ReadFromJsonAsync<ContactDto>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.FirstName);
        Assert.Equal("new@example.com", updated.Email);
        Assert.Equal("Istanbul", updated.City);
        Assert.Equal("Turkiye", updated.Country);
        Assert.True(updated.IsFavorite);
        Assert.Contains("updated", updated.Tags);
    }

    [Fact]
    public async Task DeleteContact_RemovesContact()
    {
        var created = await CreateAndReadContactAsync("Delete", "Me", phoneNumber: "+905551112233");

        var deleteResponse = await _client.DeleteAsync($"/api/v1/contacts/{created.Id}");
        var getResponse = await _client.GetAsync($"/api/v1/contacts/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task SearchContacts_MatchesNameAndEmail()
    {
        await CreateAndReadContactAsync("Ada", "Lovelace", "ada@example.com");
        await CreateAndReadContactAsync("Grace", "Hopper", "grace@example.com");

        var nameResult = await _client.GetFromJsonAsync<PagedResult<ContactDto>>("/api/v1/contacts?search=lovelace");
        var emailResult = await _client.GetFromJsonAsync<PagedResult<ContactDto>>("/api/v1/contacts?search=grace@example.com");

        Assert.NotNull(nameResult);
        Assert.NotNull(emailResult);
        Assert.Equal("Ada", nameResult.Items.Single().FirstName);
        Assert.Equal("Grace", emailResult.Items.Single().FirstName);
    }

    [Fact]
    public async Task SearchContacts_MatchesPostalAddressFields()
    {
        await CreateAndReadContactAsync(
            "City",
            "Search",
            "city.search@example.com",
            city: "Ankara",
            country: "Turkiye",
            addressLine: "Ataturk Boulevard");

        var cityResult = await _client.GetFromJsonAsync<PagedResult<ContactDto>>("/api/v1/contacts?search=ankara");
        var addressResult = await _client.GetFromJsonAsync<PagedResult<ContactDto>>("/api/v1/contacts?search=ataturk");

        Assert.NotNull(cityResult);
        Assert.NotNull(addressResult);
        Assert.Equal("City", cityResult.Items.Single().FirstName);
        Assert.Equal("City", addressResult.Items.Single().FirstName);
    }

    [Fact]
    public async Task SortContacts_SupportsAscendingAndDescending()
    {
        await CreateAndReadContactAsync("Charlie", "Zed", "charlie@example.com");
        await CreateAndReadContactAsync("Alice", "Young", "alice@example.com");
        await CreateAndReadContactAsync("Bob", "Xavier", "bob@example.com");

        var ascending = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?sortBy=firstName&sortOrder=asc&pageSize=10");
        var descending = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?sortBy=firstName&sortOrder=desc&pageSize=10");

        Assert.NotNull(ascending);
        Assert.NotNull(descending);
        Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, ascending.Items.Select(contact => contact.FirstName).ToArray());
        Assert.Equal(new[] { "Charlie", "Bob", "Alice" }, descending.Items.Select(contact => contact.FirstName).ToArray());
    }

    [Fact]
    public async Task SortContacts_SupportsCityAndCountry()
    {
        await CreateAndReadContactAsync("One", "Person", "one@example.com", city: "Izmir", country: "Turkiye");
        await CreateAndReadContactAsync("Two", "Person", "two@example.com", city: "Ankara", country: "Turkiye");

        var byCity = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?sortBy=city&sortOrder=asc&pageSize=10");

        Assert.NotNull(byCity);
        Assert.Equal(new[] { "Two", "One" }, byCity.Items.Select(contact => contact.FirstName).ToArray());
    }

    [Fact]
    public async Task FiltersContacts_ByTagAndFavoriteStatus()
    {
        await CreateAndReadContactAsync("Favorite", "Student", "favorite@example.com", true, ["school"]);
        await CreateAndReadContactAsync("Regular", "Student", "regular@example.com", false, ["school"]);
        await CreateAndReadContactAsync("Work", "Person", "work@example.com", true, ["work"]);

        var result = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?tag=school&isFavorite=true&pageSize=10");

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Favorite", result.Items.Single().FirstName);
    }

    [Fact]
    public async Task FavoriteEndpoints_MarkAndUnmarkContact()
    {
        var created = await CreateAndReadContactAsync("Martin", "Fowler", "martin@example.com");

        var favoriteResponse = await _client.PatchAsync($"/api/v1/contacts/{created.Id}/favorite", null);
        var favorite = await favoriteResponse.Content.ReadFromJsonAsync<ContactDto>();

        var unfavoriteResponse = await _client.PatchAsync($"/api/v1/contacts/{created.Id}/unfavorite", null);
        var unfavorite = await unfavoriteResponse.Content.ReadFromJsonAsync<ContactDto>();

        Assert.Equal(HttpStatusCode.OK, favoriteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unfavoriteResponse.StatusCode);
        Assert.True(favorite!.IsFavorite);
        Assert.False(unfavorite!.IsFavorite);
    }

    [Fact]
    public async Task DuplicateEmail_ReturnsConflict()
    {
        await CreateAndReadContactAsync("First", "Person", "duplicate@example.com");

        var response = await CreateContactAsync("Second", "Person", "duplicate@example.com");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task InvalidEmail_ReturnsBadRequest()
    {
        var response = await CreateContactAsync("Invalid", "Email", "not-an-email");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MissingRequiredFields_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/contacts", new CreateContactDto
        {
            Email = "missing@example.com"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task MissingContactMethod_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/contacts", new CreateContactDto
        {
            FirstName = "No",
            LastName = "Method"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task NonexistentContact_ReturnsNotFoundForReadUpdateDeleteAndFavorite()
    {
        var id = Guid.NewGuid();

        var getResponse = await _client.GetAsync($"/api/v1/contacts/{id}");
        var putResponse = await _client.PutAsJsonAsync($"/api/v1/contacts/{id}", new UpdateContactDto
        {
            FirstName = "Missing",
            LastName = "Contact",
            Email = "missing@example.com"
        });
        var deleteResponse = await _client.DeleteAsync($"/api/v1/contacts/{id}");
        var favoriteResponse = await _client.PatchAsync($"/api/v1/contacts/{id}/favorite", null);

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, favoriteResponse.StatusCode);
    }

    [Fact]
    public async Task InvalidPaginationParameters_ReturnBadRequest()
    {
        var pageNumberResponse = await _client.GetAsync("/api/v1/contacts?pageNumber=0");
        var pageSizeResponse = await _client.GetAsync("/api/v1/contacts?pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, pageNumberResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, pageSizeResponse.StatusCode);
    }

    [Fact]
    public async Task ExtremePageNumber_ReturnsBadRequestInsteadOfOverflowing()
    {
        await CreateAndReadContactAsync("Overflow", "Check", "overflow@example.com");

        var response = await _client.GetAsync("/api/v1/contacts?pageNumber=2147483647&pageSize=100");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HighValidPageNumber_ReturnsEmptyPage()
    {
        await CreateAndReadContactAsync("Empty", "Page", "empty.page@example.com");

        var result = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?pageNumber=100000&pageSize=100");

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(100000, result.PageNumber);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task InvalidSortParameters_ReturnBadRequest()
    {
        var sortByResponse = await _client.GetAsync("/api/v1/contacts?sortBy=hacker");
        var sortOrderResponse = await _client.GetAsync("/api/v1/contacts?sortOrder=sideways");

        Assert.Equal(HttpStatusCode.BadRequest, sortByResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, sortOrderResponse.StatusCode);
    }

    [Fact]
    public async Task WeakEmailAndPhoneValues_ReturnBadRequest()
    {
        var emailResponse = await CreateContactAsync("Weak", "Email", "a@b");
        var phoneResponse = await CreateContactAsync("Weak", "Phone", phoneNumber: "1");

        Assert.Equal(HttpStatusCode.BadRequest, emailResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, phoneResponse.StatusCode);
    }

    [Fact]
    public async Task ProblemDetailsResponses_UseProblemJsonContentType()
    {
        var response = await _client.GetAsync($"/api/v1/contacts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RootRedirectsToSwagger()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/swagger", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Swagger_IsAccessibleAndDocumentsContactEndpoints()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var swagger = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(swagger);
        var paths = document.RootElement.GetProperty("paths");
        var info = document.RootElement.GetProperty("info");
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Address Book REST API", info.GetProperty("title").GetString());
        Assert.Equal("v1", info.GetProperty("version").GetString());
        Assert.True(paths.TryGetProperty("/api/v1/contacts", out var contactsPath));
        Assert.True(paths.TryGetProperty("/api/v1/contacts/{id}", out var contactByIdPath));
        Assert.True(paths.TryGetProperty("/api/v1/contacts/{id}/favorite", out var favoritePath));
        Assert.True(paths.TryGetProperty("/api/v1/contacts/{id}/unfavorite", out var unfavoritePath));
        Assert.True(paths.TryGetProperty("/health", out _));

        Assert.True(contactsPath.GetProperty("get").GetProperty("responses").TryGetProperty("200", out _));
        Assert.True(contactsPath.GetProperty("post").TryGetProperty("requestBody", out _));
        Assert.True(contactsPath.GetProperty("post").GetProperty("responses").TryGetProperty("201", out _));
        Assert.True(contactsPath.GetProperty("post").GetProperty("responses").TryGetProperty("400", out _));
        Assert.True(contactsPath.GetProperty("post").GetProperty("responses").TryGetProperty("409", out _));
        Assert.True(contactByIdPath.GetProperty("put").TryGetProperty("requestBody", out _));
        Assert.True(contactByIdPath.GetProperty("delete").GetProperty("responses").TryGetProperty("204", out _));
        Assert.True(favoritePath.GetProperty("patch").GetProperty("responses").TryGetProperty("200", out _));
        Assert.True(unfavoritePath.GetProperty("patch").GetProperty("responses").TryGetProperty("200", out _));

        var createProperties = schemas.GetProperty("CreateContactDto").GetProperty("properties");
        Assert.True(createProperties.TryGetProperty("addressLine", out _));
        Assert.True(createProperties.TryGetProperty("city", out _));
        Assert.True(createProperties.TryGetProperty("country", out _));

        Assert.Equal("Ceren", createProperties.GetProperty("firstName").GetProperty("example").GetString());

        var parameters = contactsPath.GetProperty("get").GetProperty("parameters").EnumerateArray().ToList();
        Assert.Equal(
            new[] { "search", "tag", "isFavorite", "sortBy", "sortOrder", "pageNumber", "pageSize" },
            parameters.Select(parameter => parameter.GetProperty("name").GetString()).ToArray());

        var sortBySchemaRef = parameters
            .Single(parameter => parameter.GetProperty("name").GetString() == "sortBy")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        var sortBySchemaName = sortBySchemaRef!.Split('/').Last();
        Assert.True(schemas.GetProperty(sortBySchemaName).TryGetProperty("enum", out _));

        var badRequestSchemaRef = contactsPath
            .GetProperty("post")
            .GetProperty("responses")
            .GetProperty("400")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.EndsWith("/ValidationProblemDetails", badRequestSchemaRef);
    }

    [Fact]
    public async Task QueryParameters_RemainCaseInsensitive()
    {
        await CreateAndReadContactAsync("Bob", "Second", "bob.case@example.com");
        await CreateAndReadContactAsync("Alice", "First", "alice.case@example.com");

        var result = await _client.GetFromJsonAsync<PagedResult<ContactDto>>(
            "/api/v1/contacts?SortBy=FirstName&SortOrder=Desc&PageSize=10");

        Assert.NotNull(result);
        Assert.Equal(new[] { "Bob", "Alice" }, result.Items.Select(contact => contact.FirstName).ToArray());
    }

    [Fact]
    public async Task InvalidContactId_ReturnsProblemDetailsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/contacts/not-a-guid");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerUiAndHealthEndpoint_AreAccessible()
    {
        var swaggerResponse = await _client.GetAsync("/swagger");
        var healthResponse = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, swaggerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
    }

    private async Task<HttpResponseMessage> CreateContactAsync(
        string firstName,
        string lastName,
        string? email = null,
        bool isFavorite = false,
        List<string>? tags = null,
        string? phoneNumber = null,
        string? addressLine = null,
        string? city = null,
        string? country = null)
    {
        return await _client.PostAsJsonAsync("/api/v1/contacts", new CreateContactDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            AddressLine = addressLine,
            City = city,
            Country = country,
            IsFavorite = isFavorite,
            Tags = tags ?? []
        });
    }

    private async Task<ContactDto> CreateAndReadContactAsync(
        string firstName,
        string lastName,
        string? email = null,
        bool isFavorite = false,
        List<string>? tags = null,
        string? phoneNumber = null,
        string? addressLine = null,
        string? city = null,
        string? country = null)
    {
        var response = await CreateContactAsync(
            firstName,
            lastName,
            email,
            isFavorite,
            tags,
            phoneNumber,
            addressLine,
            city,
            country);
        response.EnsureSuccessStatusCode();
        var contact = await response.Content.ReadFromJsonAsync<ContactDto>();
        return contact!;
    }
}

public sealed class ContactApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"AddressBookTests-{Guid.NewGuid()}";

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AddressBookDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(service =>
                service.ServiceType == typeof(DbContextOptions<AddressBookDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AddressBookDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
