# SE4458 Assignment 2 Submission Checklist

## Mandatory Requirements

- [x] Create contacts
- [x] Read contacts
- [x] Update contacts
- [x] Delete contacts
- [x] Search contacts
- [x] Sort contacts
- [x] Mark contacts as favorites
- [x] Unmark favorites
- [x] Public Swagger documentation
- [x] EF Core InMemory database
- [x] No frontend
- [x] Public deployment

## API and Architecture

- [x] RESTful resource path: `/api/v1/contacts`
- [x] API versioning in URL: `/api/v1`
- [x] Controller-Service-Repository separation
- [x] DTOs for request and response models
- [x] Dependency Injection
- [x] Validation attributes and custom validation
- [x] Consistent ProblemDetails-style errors
- [x] Pagination support
- [x] Tag filtering support
- [x] Favorite filtering support

## Automated Tests

- [x] `dotnet restore .\AddressBook.sln -m:1`
- [x] `dotnet build .\AddressBook.sln -m:1 --no-restore`
- [x] `dotnet test .\AddressBook.sln --no-build -m:1`
- [x] 17 total tests
- [x] 17 passed
- [x] 0 failed
- [x] 0 skipped
- [x] `dotnet publish` succeeded

## Live API Verification

- [x] `GET /health`
- [x] `GET /swagger`
- [x] `GET /swagger/v1/swagger.json`
- [x] `POST /api/v1/contacts`
- [x] `GET /api/v1/contacts/{id}`
- [x] `GET /api/v1/contacts`
- [x] `PUT /api/v1/contacts/{id}`
- [x] `PATCH /api/v1/contacts/{id}/favorite`
- [x] `PATCH /api/v1/contacts/{id}/unfavorite`
- [x] Search query
- [x] Sorting query
- [x] Pagination query
- [x] Tag filter query
- [x] Invalid request validation
- [x] Nonexistent contact returns `404`
- [x] Duplicate email returns `409`
- [x] Temporary live test contact was deleted

## Deployment

- [x] Dockerfile exists at repository root
- [x] Render service uses Docker runtime
- [x] Render plan is Free
- [x] Branch is `main`
- [x] Health check path is `/health`
- [x] Render `PORT` binding is supported
- [x] Swagger is available in Production

## GitHub and Submission Links

- [x] GitHub repository: `https://github.com/CrnBilg/address-book-api-ceren`
- [x] Live API: `https://address-book-api-ceren.onrender.com`
- [x] Swagger UI: `https://address-book-api-ceren.onrender.com/swagger`
- [x] OpenAPI JSON: `https://address-book-api-ceren.onrender.com/swagger/v1/swagger.json`
- [x] Health check: `https://address-book-api-ceren.onrender.com/health`

## Repository Hygiene

- [x] `.dotnet/` excluded
- [x] `.nuget/` excluded
- [x] `.templateengine/` excluded
- [x] `bin/` excluded
- [x] `obj/` excluded
- [x] `publish/` excluded
- [x] `.env` and `*.env` excluded
- [x] Temporary files excluded
- [x] No committed credentials found in final audit scan

## Remaining Issues

- [x] No blocking issue found.
- [x] Known limitation: data is in-memory and resets on restart.
- [x] Known limitation: Render Free services may cold start after inactivity.
- [x] Known limitation: assignment PDF could not be text-extracted with available local tools; no PDF-only requirements were assumed.
