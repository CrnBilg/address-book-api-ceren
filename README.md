# Address Book REST API

ASP.NET Core 8 Web API implementation for SE4458 Assignment 2. The project is intentionally backend-only and stores data in memory with Entity Framework Core InMemory.

The API has no frontend. Swagger is exposed publicly at `/swagger`.

## Architecture

The application follows a Controller-Service-Repository structure:

- Controllers define the REST contract under `/api/v1/contacts`.
- Services contain contact use cases, filtering, sorting, pagination, favorite handling, and mapping to DTOs.
- Repositories isolate EF Core data access.
- DTOs keep API input/output models separate from persistence models.
- Dependency Injection wires controllers, services, repositories, and the in-memory database.

## Design Decisions

- RESTful resource naming uses the plural resource `/api/v1/contacts`.
- API versioning is represented in the URL with `/api/v1`.
- `POST` creates contacts and returns `201 Created` with a `Location` header.
- `GET`, `PUT`, and `DELETE` operate on `/api/v1/contacts/{id}`.
- `PATCH` is used for partial state changes: marking or unmarking favorites.
- `PUT` replaces the editable contact fields.
- Duplicate email addresses are rejected with `409 Conflict`.
- Validation failures return `400 Bad Request` with validation problem details.
- Missing contacts return `404 Not Found`.
- The in-memory database is process-local and resets after application restart.
- `/health` is provided for deployment health checks.

## Features

- Create, retrieve, update, and delete contacts
- Search contacts by name, email, phone, company, notes, or tag
- Sort contacts by `firstName`, `lastName`, `email`, `company`, `createdAt`, or `updatedAt`
- Mark and unmark favorites
- Filter by tag and favorite status
- Pagination with `pageNumber` and `pageSize`
- Input validation with validation problem responses
- Consistent problem details for conflicts, not found, and unexpected errors
- Swagger/OpenAPI at `/swagger`
- Automated API tests

## API Endpoints

Base path: `/api/v1/contacts`

| Method | Endpoint | Description |
| --- | --- | --- |
| `GET` | `/api/v1/contacts` | List contacts with search, filters, sort, and pagination |
| `GET` | `/api/v1/contacts/{id}` | Get a contact by id |
| `POST` | `/api/v1/contacts` | Create a contact |
| `PUT` | `/api/v1/contacts/{id}` | Update a contact |
| `DELETE` | `/api/v1/contacts/{id}` | Delete a contact |
| `PATCH` | `/api/v1/contacts/{id}/favorite` | Mark a contact as favorite |
| `PATCH` | `/api/v1/contacts/{id}/unfavorite` | Unmark a contact as favorite |
| `GET` | `/health` | Health check endpoint |

Example list query:

```http
GET /api/v1/contacts?search=ceren&tag=school&isFavorite=true&sortBy=lastName&sortOrder=asc&pageNumber=1&pageSize=10
```

Example create request:

```json
{
  "firstName": "Ceren",
  "lastName": "Bilgenoglu",
  "email": "ceren@example.com",
  "phoneNumber": "+905551112233",
  "company": "SE4458",
  "notes": "Assignment contact",
  "isFavorite": false,
  "tags": ["school", "architecture"]
}
```

## Run Locally

If a global .NET 8 SDK is installed:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/AddressBook.Api
```

If using the workspace-local SDK created during development:

```powershell
$env:DOTNET_CLI_HOME=(Get-Location).Path
.\.dotnet\dotnet.exe restore
.\.dotnet\dotnet.exe build .\AddressBook.sln -m:1
.\.dotnet\dotnet.exe test .\AddressBook.sln --no-build -m:1
.\.dotnet\dotnet.exe run --project .\src\AddressBook.Api\AddressBook.Api.csproj
```

Open Swagger:

```text
http://localhost:<port>/swagger
```

The actual port can vary depending on the local ASP.NET Core launch profile or environment.

## Tests

The automated tests cover:

- Create, retrieve, update, and delete contacts
- Search by name and email
- Sorting ascending and descending
- Tag and favorite filters
- Pagination
- Marking and unmarking favorites
- Duplicate email conflict
- Invalid email and missing required fields
- Nonexistent contact responses
- Invalid pagination parameters
- Swagger JSON, Swagger UI, and health endpoint accessibility

## Docker

Build and run:

```bash
docker build -t address-book-api .
docker run -p 8080:8080 address-book-api
```

Open:

```text
http://localhost:8080/swagger
```

## Render Deployment Notes

This repository includes a Dockerfile suitable for Render Web Service deployment.

Recommended Render settings:

- Environment: Docker
- Health check path: `/health`
- No external database is required because the app uses EF Core InMemory.
- Render's `PORT` environment variable is read at startup and mapped to `http://0.0.0.0:{PORT}`.

Data is stored only in process memory and is reset when the application restarts.

After deployment, add these links before final submission:

- GitHub repository URL: `TODO`
- Render deployment URL: `TODO`
- Public Swagger URL: `TODO/swagger`

## Encountered Issues During Development

- The machine initially had a .NET runtime but no .NET SDK. A workspace-local .NET 8 SDK was installed under `.dotnet`.
- NuGet restore required network access to download packages.
- A running local API process locked the Windows apphost executable during rebuild. The project sets `UseAppHost=false` because the API is run as a DLL and Docker also starts the DLL.
- Docker was not available on the development machine during the last audit, so Dockerfile syntax is present but local Docker build verification could not be executed there.
