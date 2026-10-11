# SE4458 Assignment 2 Submission Checklist

## Required Links

- [x] GitHub repository: <https://github.com/CrnBilg/address-book-api-ceren>
- [x] Live Swagger UI: <https://address-book-api-ceren.onrender.com/swagger>
- [x] Live API: <https://address-book-api-ceren.onrender.com>
- [x] Health check: <https://address-book-api-ceren.onrender.com/health>

## Mandatory Assignment Requirements

- [x] Create contacts
- [x] Retrieve contacts
- [x] Edit/update contacts
- [x] Delete contacts
- [x] Search contacts
- [x] Sort contacts
- [x] Mark contacts as favorites
- [x] Unmark favorites
- [x] Store data in memory
- [x] Public Swagger documentation
- [x] No frontend
- [x] Public deployment
- [x] GitHub README with source link, design, assumptions, and encountered issues

## Address Book Scope

- [x] Optional postal address fields are supported
- [x] Address fields are accepted on create
- [x] Address fields are accepted on update
- [x] Address fields are returned in responses
- [x] Address fields are searchable
- [x] City and country can be used for sorting

## API Design

- [x] RESTful base path: `/api/v1/contacts`
- [x] API versioning in URL: `/api/v1`
- [x] Correct HTTP methods for CRUD operations
- [x] Appropriate success status codes
- [x] Validation errors return `400 Bad Request`
- [x] Missing resources return `404 Not Found`
- [x] Duplicate email returns `409 Conflict`
- [x] Pagination is implemented and constrained
- [x] Invalid sort parameters return `400 Bad Request`
- [x] Favorite operations are idempotent

## Architecture

- [x] Controller-Service-Repository separation
- [x] DTOs for API contracts
- [x] Dependency Injection
- [x] EF Core InMemory
- [x] Swagger/OpenAPI
- [x] Automated integration tests
- [x] Dockerfile and `.dockerignore` for deployment

## Verification

- [x] Automated tests: 28 total, 28 passed
- [x] Release build and publish completed successfully
- [x] Swagger documents title, version, description, endpoint summaries, response codes, error schemas, and request examples
- [x] Swagger query parameter names match the README (camelCase)
- [x] README includes source link, design, assumptions, and issues encountered
- [x] Final report is up to date

## Known Limitations

- [x] Data resets when the service restarts because EF Core InMemory is used
- [x] Render Free can cold start after inactivity
- [x] Authentication is not implemented because it is outside the assignment scope
- [x] No sample data is seeded; the address book starts empty after every restart
- [x] `PUT` is full replacement; there is no general partial-update endpoint
