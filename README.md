# StaffManagement

`StaffManagement` is an ASP.NET Core 8 Web API for basic staff record management. The solution is split into API, Application, Domain, Infrastructure, and Unit Test projects.

## Features

- Create a staff record
- Get all staff records
- Update an existing staff record
- Delete a staff record
- Search staff by `StaffId`, `Gender`, `startYear`, and `endYear`
- Unit tests for controller and repository logic

## Business Rule

When creating or updating a staff record:

- If `gender` is not provided, it defaults to `1`
- `Gender = 1` means male
- `Gender = 2` means female
- Any other value is rejected
- `StaffId` must be unique when creating a new record

If an invalid gender is submitted to `POST /api/Staff/Create`, the API returns:

```text
400 Bad Request
Gender must be 1 for male or 2 for female.
```

If a duplicate `StaffId` is submitted to `POST /api/Staff/Create`, the API returns:

```text
400 Bad Request
StaffId already exists.
```

## Solution Structure

```text
src/
  StaffManagement.API
  StaffManagement.Application
  StaffManagement.Domain
  StaffManagement.Infrastructure
tests/
  StaffManagement.UnitTests
```

## Technologies

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit

## Prerequisites

- .NET 8 SDK
- SQL Server

## Database Configuration

The API uses the connection string in [appsettings.json](C:\Users\USER\OneDrive - Royal University of Phnom Penh\Desktop\Testing\StaffManagement\src\StaffManagement.API\appsettings.json):

```json
"ConnectionStrings": {
  "CoffeeDbConnection": "Data Source=MSI;Initial Catalog=StaffManagement;User ID=sa;Password=123456;TrustServerCertificate=True"
}
```

Update this value to match your local SQL Server before running the API.

For local development, it is safer to keep your own machine-specific connection string in [appsettings.Development.json](C:\Users\USER\OneDrive - Royal University of Phnom Penh\Desktop\Testing\StaffManagement\src\StaffManagement.API\appsettings.Development.json) instead of editing the shared production-style value in `appsettings.json`.

Example local development connection string:

```json
"ConnectionStrings": {
  "CoffeeDbConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StaffManagement;Trusted_Connection=True;TrustServerCertificate=True"
}
```

## Database Setup For Other Developers

If someone clones this repository and wants to test the API, they can create the database with the scripts in [database/init.sql](C:\Users\USER\OneDrive - Royal University of Phnom Penh\Desktop\Testing\StaffManagement\database\init.sql) and [database/seed.sql](C:\Users\USER\OneDrive - Royal University of Phnom Penh\Desktop\Testing\StaffManagement\database\seed.sql).

Using `sqlcmd`:

```powershell
sqlcmd -S localhost -i database\init.sql
sqlcmd -S localhost -i database\seed.sql
```

Or they can open both `.sql` files in SQL Server Management Studio and run them manually.

After that, they only need to update `CoffeeDbConnection` in [appsettings.json](C:\Users\USER\OneDrive - Royal University of Phnom Penh\Desktop\Testing\StaffManagement\src\StaffManagement.API\appsettings.json) so it matches their own SQL Server instance.

If they are running the API in development mode, they can put their own connection string in [appsettings.Development.json](C:\Users\USER\OneDrive - Royal University of Phnom Penh\Desktop\Testing\StaffManagement\src\StaffManagement.API\appsettings.Development.json) instead.

## Run the API

From the repository root:

```powershell
dotnet restore
dotnet run --project src\StaffManagement.API\StaffManagement.API.csproj
```

Swagger is enabled in development mode.

## CORS

The API allows requests from:

```text
http://localhost:3000
```

This is configured for a React frontend.

## Data Model

### StaffDto

```json
{
  "staffId": "ST001",
  "fullName": "Alice",
  "birthDay": "1998-05-12",
  "gender": 2
}
```

Fields:

- `staffId`: string
- `fullName`: string
- `birthDay`: `DateOnly?`
- `gender`: `int?`

### StaffSearchFilterDto

```json
{
  "staffId": "ST001",
  "gender": 2,
  "startYear": 1990,
  "endYear": 2000
}
```

Fields:

- `staffId`: `string?`
- `gender`: `int?`
- `startYear`: `int?`
- `endYear`: `int?`

## API Endpoints

### Get all staff

```http
GET /api/Staff/GetAll
```

### Create staff

```http
POST /api/Staff/Create
Content-Type: application/json
```

Example body:

```json
{
  "staffId": "ST001",
  "fullName": "Alice",
  "birthDay": "1998-05-12",
  "gender": 2
}
```

If `gender` is omitted in the create request, the API saves it as `1`.

Success response:

```text
200 OK
true
```

Invalid gender response:

```text
400 Bad Request
Gender must be 1 for male or 2 for female.
```

Duplicate `StaffId` response:

```text
400 Bad Request
StaffId already exists.
```

### Update staff

```http
PUT /api/Staff/Update/{id}
Content-Type: application/json
```

Example:

```json
{
  "fullName": "Alice Updated",
  "birthDay": "1999-06-10",
  "gender": 1
}
```

If `gender` is omitted in the update request, the API saves it as `1`.

Success response:

```text
200 OK
Updated successfully
```

Invalid gender response:

```text
400 Bad Request
Gender must be 1 for male or 2 for female.
```

Not found response:

```text
404 Not Found
```

### Delete staff

```http
DELETE /api/Staff/Delete/{id}
```

Success response:

```text
200 OK
Deleted successfully
```

Not found response:

```text
404 Not Found
```

### Search staff

```http
GET /api/Staff/Search?staffId=ST001&gender=2&startYear=1990&endYear=2000
```

Notes:

- Query parameters are bound into `StaffSearchFilterDto`
- `staffId` uses exact match
- `gender` filters by exact value
- `startYear` and `endYear` must be supplied together to filter by `BirthDay.Year`
- records without `BirthDay` are excluded when year filters are used

## Run Unit Tests

```powershell
dotnet test tests\StaffManagement.UnitTests\StaffManagement.UnitTests.csproj
```

The unit tests cover:

- `StaffController` responses
- `StaffRepository` create, update, delete, get all, and search logic
- create and update validation for gender and duplicate `StaffId`

## Notes

- The repository currently uses the `CoffeeDbConnection` key name for the staff database connection string.
- The search logic uses exact `StaffId` matching, not partial matching.
