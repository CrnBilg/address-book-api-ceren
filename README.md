# Address Book REST API

Backend-only ASP.NET Core 8 Web API for **SE4458 Assignment 2 - Group 1 Address Book**.

- GitHub repository: <https://github.com/CrnBilg/address-book-api-ceren>
- Live API: <https://address-book-api-ceren.onrender.com>
- Live Swagger UI: <https://address-book-api-ceren.onrender.com/swagger>
- OpenAPI JSON: <https://address-book-api-ceren.onrender.com/swagger/v1/swagger.json>
- Health check: <https://address-book-api-ceren.onrender.com/health>

The project has no frontend. It exposes REST API endpoints and public Swagger documentation.

## Technology Stack

- ASP.NET Core Web API (.NET 8)
- C#
- Entity Framework Core InMemory
- Swashbuckle Swagger/OpenAPI
- xUnit integration tests with `WebApplicationFactory`
- Docker
- Render Free deployment

## Architecture

The API follows a Controller-Service-Repository structure:

- **Controllers** expose the REST contract under `/api/v1/contacts`.
- **Services** implement business rules, validation-related behavior, search, sorting, pagination, favorite handling, duplicate email checks, and DTO mapping.
- **Repositories** isolate EF Core data access.
- **DTOs** keep request and response contracts separate from persistence models.
- **Dependency Injection** wires controllers, services, repositories, and the in-memory database.

## API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/health` | Health check |
| `GET` | `/api/v1/contacts` | List contacts with search, filters, sorting, and pagination |
| `GET` | `/api/v1/contacts/{id}` | Retrieve one contact by ID |
| `POST` | `/api/v1/contacts` | Create a contact |
| `PUT` | `/api/v1/contacts/{id}` | Replace editable fields of a contact |
| `DELETE` | `/api/v1/contacts/{id}` | Delete a contact |
| `PATCH` | `/api/v1/contacts/{id}/favorite` | Mark a contact as favorite |
| `PATCH` | `/api/v1/contacts/{id}/unfavorite` | Remove a contact from favorites |

### Query Parameters for `GET /api/v1/contacts`

| Parameter | Type | Default | Description |
| --- | --- | --- | --- |
| `search` | string | - | Case-insensitive text search across name, email, phone, company, postal address, notes, and tags |
| `tag` | string | - | Returns contacts that have this tag (case-insensitive) |
| `isFavorite` | boolean | - | Filters by favorite state |
| `sortBy` | enum | `LastName` | `FirstName`, `LastName`, `Email`, `Company`, `City`, `Country`, `CreatedAt`, `UpdatedAt` |
| `sortOrder` | enum | `Asc` | `Asc` or `Desc` |
| `pageNumber` | integer | `1` | One-based page number, 1-100000 |
| `pageSize` | integer | `10` | Items per page, 1-100 |

Parameter names and enum values are case-insensitive, so `sortBy=city&sortOrder=desc` is also accepted.

Example query:

```http
GET /api/v1/contacts?search=istanbul&tag=school&isFavorite=true&sortBy=city&sortOrder=asc&pageNumber=1&pageSize=10
```

### Response Codes

| Status | When |
| --- | --- |
| `200 OK` | Successful read, update, or favorite change |
| `201 Created` | Contact created; `Location` header points to the new contact |
| `204 No Content` | Contact deleted |
| `400 Bad Request` | Validation failed (`ValidationProblemDetails`) |
| `404 Not Found` | Contact does not exist or the ID is not a valid GUID (`ProblemDetails`) |
| `409 Conflict` | Email address is already used by another contact (`ProblemDetails`) |

## Data Model

A contact contains:

- `id`
- `firstName`
- `lastName`
- `email`
- `phoneNumber`
- `company`
- `addressLine`
- `district`
- `city`
- `postalCode`
- `country`
- `notes`
- `isFavorite`
- `tags`
- `createdAt`
- `updatedAt`

Example create request:

```json
{
  "firstName": "Ceren",
  "lastName": "Bilgenoglu",
  "email": "ceren@example.com",
  "phoneNumber": "+905551112233",
  "company": "SE4458",
  "addressLine": "Example Street 42",
  "district": "Kadikoy",
  "city": "Istanbul",
  "postalCode": "34710",
  "country": "Turkiye",
  "notes": "Assignment contact",
  "isFavorite": false,
  "tags": ["school", "architecture"]
}
```

## Design Decisions

- RESTful resource naming uses the plural resource `/api/v1/contacts`.
- API versioning is represented in the URL with `/api/v1`.
- `POST` creates contacts and returns `201 Created` with a `Location` header.
- `GET`, `PUT`, and `DELETE` operate on `/api/v1/contacts/{id}`.
- `PATCH` is used for idempotent favorite state changes.
- `PUT` uses full replacement semantics for editable fields.
- Duplicate email addresses are rejected with `409 Conflict`.
- Invalid input returns `400 Bad Request` with validation problem details.
- Missing contacts return `404 Not Found`.
- The root URL redirects to `/swagger`.
- Swagger is enabled in Production because the assignment requires public API documentation.

## Assumptions

- An address book entry is a contact with a name, at least one contact method, and optional postal address information.
- At least one contact method, email or phone number, is required.
- Email addresses are unique case-insensitively.
- Search is a case-insensitive substring match across name, email, phone, company, postal address, notes, and tags.
- Sort fields are limited to documented values: `firstName`, `lastName`, `email`, `company`, `city`, `country`, `createdAt`, and `updatedAt`. The default sort is `lastName` ascending.
- Page number is one-based and limited to prevent overflow; page size is limited to 1-100.
- `PUT` replaces all editable fields; editing a contact means sending its complete new state. Favorite operations are idempotent.
- Data is stored in EF Core InMemory, so the address book starts empty and resets when the service restarts.
- No authentication is required because the assignment asks for public APIs and Swagger, and does not require user accounts.
- The application is designed for a single-user assignment scenario, without concurrency control or persistent storage.

## Validation Rules

- `firstName` and `lastName` are required.
- `email` must be valid and must have a dotted domain when provided.
- `phoneNumber` must contain at least 7 digits when provided.
- At least one of `email` or `phoneNumber` is required.
- Postal address fields are optional.
- A contact can have at most 10 tags.
- Tags must be non-empty and at most 40 characters.
- `pageNumber` must be between 1 and 100000.
- `pageSize` must be between 1 and 100.
- Invalid `sortBy` or `sortOrder` values return `400 Bad Request`.

## Local Setup

With a .NET 8 SDK installed:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/AddressBook.Api
```

Open Swagger at:

```text
http://localhost:<port>/swagger
```

## Testing

Run:

```bash
dotnet test
```

The integration tests cover:

- Create, retrieve, update, and delete contacts
- Postal address persistence and retrieval
- Search by name, email, and address fields
- Sorting, including city and country support
- Tag and favorite filters
- Pagination, including overflow regression coverage
- Marking and unmarking favorites
- Duplicate email conflict
- Invalid email, weak phone, missing required fields, and missing contact method
- Nonexistent contact responses
- Invalid pagination and sorting parameters
- Case-insensitive query parameter binding
- Swagger JSON (title, camelCase parameters, enum sorting, examples, error schemas), Swagger UI, root redirect, and health endpoint accessibility
- ProblemDetails content type for application errors and invalid IDs

Latest local result:

```text
28 total, 28 passed, 0 failed, 0 skipped
```

## Docker

Build and run (a `.dockerignore` keeps local build output and tooling out of the image context):

```bash
docker build -t address-book-api .
docker run -p 8080:8080 address-book-api
```

Open:

```text
http://localhost:8080/swagger
```

## Deployment

The repository includes a Dockerfile suitable for Render Web Service deployment.

Render configuration:

- Runtime: Docker
- Branch: `main`
- Dockerfile path: `./Dockerfile`
- Health check path: `/health`
- Instance type: Free

The application reads Render's `PORT` environment variable at startup and binds to `http://0.0.0.0:{PORT}`. No external database is required.

## Issues Encountered and Resolved

- The project needed public Swagger in Production, so Swagger middleware is enabled outside Development.
- Render requires the application to listen on the assigned `PORT`; startup configuration reads this environment variable.
- Pagination originally used an `int` multiplication that could overflow for extreme page numbers; the final version limits page numbers and uses safe offset calculation.
- The address book model originally contained contact information but not postal address fields; optional address fields were added across model, DTOs, mapping, search, sorting, Swagger, and tests.
- Swagger originally had minimal metadata; the final version adds project title, version, description, XML comments, request examples, documented status codes with error schemas, and enum-backed sorting parameters.
- Swagger listed query parameters in PascalCase (`SortBy`) while the documentation used camelCase (`sortBy`); explicit camelCase binding names now keep Swagger and README consistent, and binding remains case-insensitive.
- Requests with a non-GUID ID returned an empty `404` body; status code pages now return a ProblemDetails body for such responses.

## Known Limitations

- Data is stored in EF Core InMemory and resets when the application restarts or Render restarts the container.
- Render Free services can cold start after inactivity.
- There is no authentication or authorization because it is outside the assignment scope.
- `PUT` is full replacement; there is no general partial-update (`PATCH /api/v1/contacts/{id}`) endpoint.
- No sample data is seeded; the address book starts empty after every restart.
