# Baxture Users API

ASP.NET Core Web API for user management with JWT authentication, SQL Server persistence, admin-only endpoints, search, and export.

## Database

The API uses Entity Framework Core with SQL Server:

```text
Server=AJIT\SQLEXPRESS;Database=BaxtureUsersDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

The database and `Users` table are created automatically on application startup with `EnsureCreatedAsync`. If the table is empty, the app seeds one admin user and one normal user.

## Run

```powershell
dotnet run --project Baxture.Api
```

Swagger UI:

```text
https://localhost:7077/swagger
```

If the local HTTPS development certificate is not trusted yet, run:

```powershell
dotnet dev-certs https --trust
```

In Swagger, call `POST /api/auth/login`, copy the returned `token`, click `Authorize`, and paste the token value.

Seeded credentials:

- Admin: `admin` / `admin123`
- Normal user: `user` / `user123`

## Endpoints

- `POST /api/auth/login`
- `GET /api/users` admin only
- `GET /api/users/{userId}`
- `POST /api/users` admin only
- `PUT /api/users/{userId}`
- `DELETE /api/users/{userId}` admin only
- `POST /api/users/search` admin only
- `POST /api/users/export` admin only

Use `Authorization: Bearer <token>` for all endpoints except login.

## Login example

```json
{
  "username": "admin",
  "password": "admin123"
}
```

## Create user example

```json
{
  "username": "john",
  "password": "john123",
  "isAdmin": false,
  "age": 30,
  "hobbies": ["music", "cycling"]
}
```

## Search example

```json
{
  "filters": [
    { "fieldName": "username", "fieldValue": "jo" }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "sortBy": "age",
  "sortDirection": "desc"
}
```

## Export example

```json
{
  "format": "PDF",
  "search": {
    "filters": [],
    "pageNumber": 1,
    "pageSize": 20,
    "sortBy": "username",
    "sortDirection": "asc"
  }
}
```

Design pattern: repository and service layers separate storage, business logic, token handling, and controller routing.
