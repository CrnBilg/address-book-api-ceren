# SE4458 Assignment 2 Final Report

## 1. Project Title and Objective

**Project:** Address Book REST API  
**Student:** Ceren Bilgenoglu  
**Course:** SE4458 - Software Architecture & Design of Modern Large Scale Systems

The objective of this project is to provide a backend-only Address Book REST API that supports contact management operations, public API documentation, automated tests, and public deployment.

## 2. Evidence Sources and Limitations

This report is based on the actual local source code, automated test execution, Git history, Dockerfile, README, Render deployment, and the public OpenAPI document.

The API Design lecture PowerPoint was available and its REST principles were considered: resource-oriented naming, HTTP methods, status codes, API versioning, dependency injection, Swagger/OpenAPI, and pagination.

The assignment PDF file was present locally, but text extraction was not available with the installed tools in this environment. Therefore, this report does not invent PDF-only requirements. It evaluates the mandatory requirements explicitly provided in the assignment prompt and the visible project evidence.

## 3. Assignment Requirements and Compliance Matrix

| Requirement | Implementation Location | Endpoint | Test Coverage | Status | Explanation |
| --- | --- | --- | --- | --- | --- |
| Create contacts | `ContactsController.CreateContact`, `ContactService.CreateAsync` | `POST /api/v1/contacts` | Unit/API tests and live test | PASS | Returns `201 Created` with a DTO and `Location` header. |
| Read contacts | `ContactsController.GetContacts`, `GetContact` | `GET /api/v1/contacts`, `GET /api/v1/contacts/{id}` | Unit/API tests and live test | PASS | Supports list and by-id retrieval. |
| Edit/update contacts | `ContactsController.UpdateContact`, `ContactService.UpdateAsync` | `PUT /api/v1/contacts/{id}` | Unit/API tests and live test | PASS | Replaces editable fields and returns `200 OK`. |
| Delete contacts | `ContactsController.DeleteContact` | `DELETE /api/v1/contacts/{id}` | Unit/API tests and live test | PASS | Returns `204 No Content`; deleted record later returns `404`. |
| Search contacts | `ContactService.GetAsync` | `GET /api/v1/contacts?search=...` | Unit/API tests and live test | PASS | Searches name, email, phone, company, notes, and tags. |
| Sort contacts | `ContactService.ApplySorting` | `GET /api/v1/contacts?sortBy=...&sortOrder=...` | Unit/API tests and live test | PASS | Supports `firstName`, `lastName`, `email`, `company`, `createdAt`, `updatedAt`. |
| Mark contacts as favorites | `MarkFavoriteAsync` | `PATCH /api/v1/contacts/{id}/favorite` | Unit/API tests and live test | PASS | Sets `isFavorite=true`. |
| Unmark favorites | `UnmarkFavoriteAsync` | `PATCH /api/v1/contacts/{id}/unfavorite` | Unit/API tests and live test | PASS | Sets `isFavorite=false`. |
| Public Swagger | `Program.cs`, Swashbuckle | `/swagger`, `/swagger/v1/swagger.json` | Unit/API tests and live test | PASS | Swagger is available in Production on Render. |
| In-memory database | `AddressBookDbContext`, `UseInMemoryDatabase` | All data endpoints | Unit/API tests | PASS | EF Core InMemory is used; data resets when app restarts. |
| No frontend | Project structure | N/A | Source inspection | PASS | Backend API only. |
| Public deployment | Render Docker service | Public URL | Live test | PASS | Deployed to Render Free plan. |

## 4. Technology Stack

- ASP.NET Core Web API (.NET 8)
- C#
- Entity Framework Core InMemory
- Swashbuckle Swagger/OpenAPI
- xUnit and `Microsoft.AspNetCore.Mvc.Testing`
- Docker multi-stage build
- Render Free plan deployment
- GitHub repository hosting

## 5. Architecture Explanation

The project follows a Controller-Service-Repository architecture:

- **Controllers** define the HTTP API contract under `/api/v1/contacts`.
- **Services** contain business behavior such as search, sorting, pagination, tag filtering, favorite handling, duplicate email checks, and DTO mapping.
- **Repositories** isolate EF Core data access.
- **DTOs** separate API input/output models from the persistence model.
- **Dependency Injection** wires `IContactService`, `IContactRepository`, and `AddressBookDbContext`.

This design aligns with the lecture emphasis on API contracts, REST resources, separation of concerns, dependency injection, and public Swagger documentation.

## 6. API Endpoints and HTTP Methods

| Method | Endpoint | Purpose | Expected Success Status |
| --- | --- | --- | --- |
| `GET` | `/health` | Health check | `200 OK` |
| `GET` | `/swagger` | Swagger UI | `200 OK` |
| `GET` | `/swagger/v1/swagger.json` | OpenAPI JSON | `200 OK` |
| `GET` | `/api/v1/contacts` | List/search/filter/sort/page contacts | `200 OK` |
| `GET` | `/api/v1/contacts/{id}` | Retrieve contact by ID | `200 OK` |
| `POST` | `/api/v1/contacts` | Create contact | `201 Created` |
| `PUT` | `/api/v1/contacts/{id}` | Update contact | `200 OK` |
| `DELETE` | `/api/v1/contacts/{id}` | Delete contact | `204 No Content` |
| `PATCH` | `/api/v1/contacts/{id}/favorite` | Mark favorite | `200 OK` |
| `PATCH` | `/api/v1/contacts/{id}/unfavorite` | Remove favorite | `200 OK` |

## 7. Data Model and Validation

The contact API uses DTOs with validation attributes:

- `firstName` and `lastName` are required.
- `email` must be a valid email when provided.
- `phoneNumber` uses phone validation.
- At least one contact method is required: email or phone number.
- Tags are limited to at most 10 items and each tag must be non-empty and at most 40 characters.
- Duplicate email addresses are rejected with `409 Conflict`.
- Invalid input returns `400 Bad Request` with validation problem details.
- Missing contacts return `404 Not Found` with problem details.

## 8. Swagger and API Documentation

Swagger is enabled outside Development mode and was verified on the live Render deployment.

Verified URLs:

- Swagger UI: `https://address-book-api-ceren.onrender.com/swagger`
- OpenAPI JSON: `https://address-book-api-ceren.onrender.com/swagger/v1/swagger.json`

The OpenAPI document exposes the expected paths:

- `/health`
- `/api/v1/contacts`
- `/api/v1/contacts/{id}`
- `/api/v1/contacts/{id}/favorite`
- `/api/v1/contacts/{id}/unfavorite`

## 9. Testing Methodology and Actual Results

### Automated Local Tests

Executed with the workspace-local .NET 8 SDK.

Commands:

```powershell
.\.dotnet\dotnet.exe restore .\AddressBook.sln -m:1
.\.dotnet\dotnet.exe build .\AddressBook.sln -m:1 --no-restore
.\.dotnet\dotnet.exe test .\AddressBook.sln --no-build -m:1
.\.dotnet\dotnet.exe publish .\src\AddressBook.Api\AddressBook.Api.csproj -c Release -o .\publish\AddressBook.Api
```

Results:

- Restore: PASS with `-m:1`
- Build: PASS
- Build warnings: 0
- Build errors: 0
- Automated tests: 17 total, 17 passed, 0 failed, 0 skipped
- Publish: PASS

Note: On this Windows/OneDrive/local SDK environment, solution-level restore without `-m:1` returned a silent exit code 1. Project-level restore and `dotnet restore .\AddressBook.sln -m:1` succeeded. The solution file was adjusted to use SDK-style project type GUIDs, and the documented local command uses `-m:1`.

### Live Render End-to-End Tests

All tests used the real public Render URL, not localhost:

`https://address-book-api-ceren.onrender.com`

A unique fictional temporary contact was created and deleted during the test.

| # | Method | Endpoint | Expected | Actual | Result |
| --- | --- | --- | --- | --- | --- |
| 1 | `GET` | `/health` | 200 | 200 | PASS |
| 2 | `GET` | `/swagger` | 200 | 200 | PASS |
| 3 | `GET` | `/swagger/v1/swagger.json` | 200 | 200 | PASS |
| 4 | `POST` | `/api/v1/contacts` | 201 | 201 | PASS |
| 5 | `GET` | `/api/v1/contacts/{id}` | 200 | 200 | PASS |
| 6 | `GET` | `/api/v1/contacts?pageNumber=1&pageSize=100` | 200 | 200 | PASS |
| 7 | `PUT` | `/api/v1/contacts/{id}` | 200 | 200 | PASS |
| 8 | `PATCH` | `/api/v1/contacts/{id}/favorite` | 200 | 200 | PASS |
| 9 | `PATCH` | `/api/v1/contacts/{id}/unfavorite` | 200 | 200 | PASS |
| 10 | `GET` | `/api/v1/contacts?search={name}` | 200 | 200 | PASS |
| 11 | `GET` | `/api/v1/contacts?sortBy=firstName&sortOrder=desc` | 200 | 200 | PASS |
| 12 | `GET` | `/api/v1/contacts?pageNumber=1&pageSize=1` | 200 | 200 | PASS |
| 13 | `GET` | `/api/v1/contacts?tag={tag}` | 200 | 200 | PASS |
| 14 | `POST` | `/api/v1/contacts` with invalid email | 400 | 400 | PASS |
| 15 | `GET` | `/api/v1/contacts/{nonexistentId}` | 404 | 404 | PASS |
| 16 | `POST` | `/api/v1/contacts` with duplicate email | 409 | 409 | PASS |
| 17 | `DELETE` | `/api/v1/contacts/{id}` | 204 | 204 | PASS |
| 18 | `GET` | `/api/v1/contacts/{deletedId}` | 404 | 404 | PASS |

Live API result: 18 total, 18 passed, 0 failed.

## 10. Deployment Architecture and Render Configuration

The application is deployed as a Docker-based Render Web Service.

Render configuration verified during deployment:

- Service name: `address-book-api-ceren`
- Runtime: Docker
- Plan: Free
- Branch: `main`
- Dockerfile path: `./Dockerfile`
- Health check path: `/health`
- Public URL: `https://address-book-api-ceren.onrender.com`

The Dockerfile uses a multi-stage build:

1. `mcr.microsoft.com/dotnet/sdk:8.0` to restore and publish.
2. `mcr.microsoft.com/dotnet/aspnet:8.0` to run the published API.

The application reads Render's `PORT` environment variable and binds to `http://0.0.0.0:{PORT}`. Logs confirmed the live container listened on Render's assigned port.

## 11. GitHub and Live Deployment URLs

- GitHub repository: `https://github.com/CrnBilg/address-book-api-ceren`
- Live API: `https://address-book-api-ceren.onrender.com`
- Swagger UI: `https://address-book-api-ceren.onrender.com/swagger`
- OpenAPI JSON: `https://address-book-api-ceren.onrender.com/swagger/v1/swagger.json`
- Health check: `https://address-book-api-ceren.onrender.com/health`

## 12. Security and Repository Hygiene

The repository excludes local SDK installs, package caches, build outputs, publish outputs, editor folders, temporary files, and environment files through `.gitignore`.

No API keys, access tokens, passwords, or connection strings were found in the committed source files during the final audit scan. Matches such as `CancellationToken` are code terms, not secrets.

## 13. Known Limitations

- The database is intentionally in-memory, so all data is reset when the application restarts or Render restarts the container.
- Render Free services can experience cold starts after inactivity.
- Docker was verified through Render's remote build. Local Docker was not available earlier in the development environment.
- The assignment PDF existed locally but could not be text-extracted with available tools; this report avoids PDF-only assumptions.

## 14. Conclusion

Based on source inspection, automated tests, public Swagger inspection, live Render end-to-end testing, and deployment verification, the project satisfies the provided SE4458 Assignment 2 Address Book REST API requirements.

The API is ready for submission, with the normal caveats of in-memory storage and Render Free cold-start behavior.
