# SE4458 Assignment 2 Final Report

## Project

**Student:** Ceren Bilgenoglu  
**Course:** SE4458 - Software Architecture & Design of Modern Large Scale Systems  
**Assignment:** Develop and deploy a simple REST API  
**Group:** Group 1 - Address Book

Links:

- GitHub repository: <https://github.com/CrnBilg/address-book-api-ceren>
- Live API: <https://address-book-api-ceren.onrender.com>
- Swagger UI: <https://address-book-api-ceren.onrender.com/swagger>
- OpenAPI JSON: <https://address-book-api-ceren.onrender.com/swagger/v1/swagger.json>
- Health check: <https://address-book-api-ceren.onrender.com/health>

## Objective

The project implements a backend-only Address Book REST API. It supports creating, retrieving, updating, deleting, searching, sorting, and favoriting address book contacts. Data is stored in memory with Entity Framework Core InMemory and the API is publicly deployed with Swagger documentation.

## Architecture

The application uses a Controller-Service-Repository architecture:

- Controllers expose REST endpoints under `/api/v1/contacts`.
- Services contain business rules, search, sorting, pagination, favorite handling, duplicate email checks, and DTO mapping.
- Repositories isolate EF Core data access.
- DTOs define request and response contracts separately from persistence models.
- Dependency Injection wires controllers, services, repositories, and the database context.

## Technology Stack

- ASP.NET Core Web API (.NET 8)
- C#
- Entity Framework Core InMemory
- Swashbuckle Swagger/OpenAPI
- xUnit integration tests
- Docker
- Render Free deployment

## Assignment Compliance

| Requirement | Implementation | Status |
| --- | --- | --- |
| Create contacts | `POST /api/v1/contacts` | PASS |
| Retrieve contacts | `GET /api/v1/contacts`, `GET /api/v1/contacts/{id}` | PASS |
| Edit/update contacts | `PUT /api/v1/contacts/{id}` | PASS |
| Delete contacts | `DELETE /api/v1/contacts/{id}` | PASS |
| Search contacts | `GET /api/v1/contacts?search=...` | PASS |
| Sort contacts | `sortBy` and `sortOrder` query parameters | PASS |
| Mark favorites | `PATCH /api/v1/contacts/{id}/favorite` | PASS |
| Unmark favorites | `PATCH /api/v1/contacts/{id}/unfavorite` | PASS |
| Address book data | Optional postal address fields on contact DTOs and model | PASS |
| In-memory database | EF Core InMemory | PASS |
| Public Swagger | `/swagger` and `/swagger/v1/swagger.json` | PASS |
| No frontend | Backend API only | PASS |
| Public deployment | Render Docker Web Service | PASS |
| README with design, assumptions, and issues | `README.md` | PASS |

## API Endpoints

| Method | Endpoint | Success Status |
| --- | --- | --- |
| `GET` | `/health` | `200 OK` |
| `GET` | `/api/v1/contacts` | `200 OK` |
| `GET` | `/api/v1/contacts/{id}` | `200 OK` |
| `POST` | `/api/v1/contacts` | `201 Created` |
| `PUT` | `/api/v1/contacts/{id}` | `200 OK` |
| `DELETE` | `/api/v1/contacts/{id}` | `204 No Content` |
| `PATCH` | `/api/v1/contacts/{id}/favorite` | `200 OK` |
| `PATCH` | `/api/v1/contacts/{id}/unfavorite` | `200 OK` |

## Address Fields

The final contact model includes optional postal address information:

- `addressLine`
- `district`
- `city`
- `postalCode`
- `country`

These fields are supported in create, update, retrieve, search, sorting where appropriate, Swagger schemas, and automated tests.

## Validation and Error Handling

- Required fields: `firstName`, `lastName`.
- At least one contact method is required: email or phone number.
- Duplicate emails return `409 Conflict`.
- Invalid input returns `400 Bad Request`.
- Missing contacts return `404 Not Found`.
- Application errors use ProblemDetails-style responses.
- Pagination is constrained to prevent integer overflow.
- Invalid `sortBy` and `sortOrder` values return `400 Bad Request`.
- Validation errors use `ValidationProblemDetails`; not-found and conflict errors, including non-GUID IDs, use `application/problem+json`.

## Swagger Documentation

- Title, version, and description for the Address Book API.
- Summaries, remarks, and per-status-code response descriptions for every endpoint.
- Typed error schemas: `ValidationProblemDetails` for `400`, `ProblemDetails` for `404` and `409`.
- Request examples for all create and update fields.
- camelCase query parameter names matching the README, with `sortBy` and `sortOrder` shown as enum drop-downs.

## Testing Results

Automated integration tests (xUnit with `WebApplicationFactory` and an isolated InMemory database) were run locally after the final changes.

```text
28 total, 28 passed, 0 failed, 0 skipped
```

Covered areas:

- CRUD operations
- Postal address persistence, retrieval, update, and search
- Search, sorting, tag filtering, favorite filtering, and pagination
- Pagination overflow regression
- Invalid pagination and sort parameters
- Favorite and unfavorite endpoints
- Duplicate email conflict
- Validation errors
- Not found responses
- Swagger and health endpoint accessibility
- Root redirect to Swagger
- ProblemDetails content type, including invalid IDs
- Case-insensitive query parameters and Swagger parameter naming

## Deployment

The application is deployed as a Docker-based Render Web Service on the Free plan.

Configuration:

- Runtime: Docker
- Branch: `main`
- Dockerfile: `./Dockerfile`
- Health check path: `/health`
- Public Swagger: `/swagger`
- Docker build context is limited by `.dockerignore`

## Known Limitations

- Data is stored in memory and resets on service restart.
- Render Free services can cold start after inactivity.
- No authentication is implemented because it is outside the assignment scope.
- `PUT` is full replacement; there is no general partial-update (`PATCH /api/v1/contacts/{id}`) endpoint.
- No sample data is seeded; the address book starts empty after every restart.

## Conclusion

The project satisfies the stated SE4458 Assignment 2 Address Book REST API requirements: backend-only REST API, in-memory storage, public Swagger, public deployment, GitHub repository, README documentation, and address book functionality including create, retrieve, update, delete, search, sort, and favorite operations.
