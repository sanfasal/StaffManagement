# StaffManagement

`StaffManagement` is an ASP.NET Core 8 Web API for basic staff record management. The solution is split into API, Application, Domain, Infrastructure, and Unit Test projects.

## Features

- Create a staff record
- Get all staff records (with pagination)
- Update an existing staff record
- Delete a staff record
- Search staff by `StaffId`, `Gender`, `StartYear`, and `EndYear`
- Unit tests for controller and repository logic

## Business Rules

When creating or updating a staff record:

- If `gender` is not provided, it defaults to `1`
- `Gender = 1` means male
- `Gender = 2` means female
- Any other value is rejected

If an invalid gender is submitted to `POST /api/Staff/Create` or `PUT /api/Staff/Update/{id}`, the API returns:

```text
400 Bad Request
Gender must be 1 for male or 2 for female.
```

## Database Schema (SQL Script)

If you are setting up the project for the first time, you can create the database and table using the following SQL script:

```sql
CREATE DATABASE [StaffManagement];
GO

USE [StaffManagement]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Staff](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StaffID] [nvarchar](50) NULL,
	[FullName] [nvarchar](100) NULL,
	[BirthDay] [date] NULL,
	[Gender] [int] NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NULL,
 CONSTRAINT [PK_Staff] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
```

## Database Configuration

The API uses the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "CoffeeDbConnection": "Data Source=MSI;Initial Catalog=StaffManagement;User ID=sa;Password=123456;TrustServerCertificate=True"
}
```

Update this value to match your local SQL Server before running the API. For local development, you can override this in `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "CoffeeDbConnection": "Server=(localdb)\\MSSQLLocalDB;Database=StaffManagement;Trusted_Connection=True;TrustServerCertificate=True"
}
```

## Run the API

From the repository root:

```powershell
dotnet restore
dotnet run --project src\StaffManagement.API\StaffManagement.API.csproj
```

Swagger is enabled in development mode.

## CORS

The API allows requests from: `http://localhost:3000` (Configured for a React frontend).

## Data Model

### StaffDto (Response / Update Model)

```json
{
  "id": 1,
  "staffId": "ST001",
  "fullName": "Alice",
  "birthDay": "1998-05-12",
  "gender": 2,
  "createdDate": "2024-01-01T10:00:00Z",
  "updatedDate": null
}
```

Fields:
- `id`: `int` (Primary Key in DB)
- `staffId`: `string?`
- `fullName`: `string?`
- `birthDay`: `DateOnly?`
- `gender`: `int?`
- `createdDate`: `DateTime?`
- `updatedDate`: `DateTime?`

### StaffFilterDto (Query Model)

```json
{
  "staffId": "ST001",
  "gender": 2,
  "startYear": 1990,
  "endYear": 2000,
  "page": 1,
  "pageSize": 10
}
```

Fields:
- `staffId`: `string?`
- `gender`: `int?`
- `startYear`: `int?`
- `endYear`: `int?`
- `page`: `int` (default: 1)
- `pageSize`: `int` (default: 10)

## API Endpoints

### Get all staff (with search and pagination)

```http
GET /api/Staff/GetAll?page=1&pageSize=10&staffId=ST001
```

Notes:
- Query parameters are bound into `StaffFilterDto`
- `staffId` uses exact match
- `gender` filters by exact value
- `startYear` and `endYear` filter by `BirthDay.Year`
- `page` and `pageSize` handle pagination

### Create staff

```http
POST /api/Staff/Create
Content-Type: application/json

{
  "staffId": "ST001",
  "fullName": "Alice",
  "birthDay": "1998-05-12",
  "gender": 2
}
```

### Update staff

```http
PUT /api/Staff/Update/{id}
Content-Type: application/json

{
  "staffId": "ST001",
  "fullName": "Alice Updated",
  "birthDay": "1999-06-10",
  "gender": 1
}
```

*Note: The `{id}` in the URL corresponds to the primary key `Id` (integer), not the `StaffId` string.*

### Delete staff

```http
DELETE /api/Staff/Delete/{id}
```

*Note: The `{id}` in the URL corresponds to the primary key `Id` (integer).*
