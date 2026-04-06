# Chd.Pos.Api – Point of Sale Demo

A real-world demo showing [Chd.AutoUI](https://www.nuget.org/packages/Chd.AutoUI), [Chd.AutoUI.EF](https://www.nuget.org/packages/Chd.AutoUI.EF), and [@mehmetyoldas/chd-auto-ui-react](https://www.npmjs.com/package/@mehmetyoldas/chd-auto-ui-react) working together in a complete application.

---

## 📝 Table of Contents

- [About](#about)
- [What Is Included](#what-is-included)
- [Project Structure](#project-structure)
- [Requirements](#requirements)
- [Quick Start](#quick-start)
  - [1. Start PostgreSQL](#1-start-postgresql)
  - [2. Run the API](#2-run-the-api)
  - [3. Start the Frontend](#3-start-the-frontend)
- [Authentication Setup](#authentication-setup)
  - [IUserTokenProvider Implementation](#iusertokenprovider-implementation)
  - [Demo Credentials](#demo-credentials)
- [Program.cs Walkthrough](#programcs-walkthrough)
- [Permission Management Example](#permission-management-example)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Technologies](#technologies)
- [Related Projects](#related-projects)

---

## About

Chd.Pos is a Point of Sale demo built with the CHD AutoUI framework. The domain is intentionally kept simple — the goal is to show the minimum wiring required to get metadata-driven CRUD working end-to-end with JWT authentication and role-based permissions. It is not a production POS system.

---

## 📦 What Is Included

- Product, category, customer, and supplier management
- Sales and sale items (invoice lines)
- Stock movement tracking
- JWT-based authentication via `IUserTokenProvider`
- Metadata and identity endpoints auto-registered by `Chd.AutoUI`
- Generic repository pattern via `Chd.AutoUI.EF`
- Role-based permissions demonstrated on all entities
- PostgreSQL with EF Core migrations
- Swagger for API exploration
- Serilog for structured logging

---

## 🏗️ Project Structure

```
Chd.Pos/
├── Chd.Pos.Core/
│   ├── Entities/
│   │   └── PosEntities.cs        # Product, Category, Customer, Sale, SaleItem...
│   └── DTOs/
│       └── PosDTOs.cs            # ProductDto, CategoryDto... with AutoUI attributes
└── Chd.Pos.Api/
    ├── Controllers/
    │   ├── ProductsController.cs
    │   ├── CategoriesController.cs
    │   ├── CustomersController.cs
    │   ├── SalesController.cs
    │   └── ...
    ├── Data/
    │   └── PosDbContext.cs
    ├── Migrations/
    ├── UserRepoesitory.cs        # IUserTokenProvider — demo authentication
    ├── Program.cs
    └── appsettings.json
```

---

## ⚙️ Requirements

- .NET 8 SDK
- Node.js 18+ (for the React frontend)
- Docker Desktop (for PostgreSQL)

---

## 🚀 Quick Start

### 1. Start PostgreSQL

```powershell
cd Library.Tests/Docker-Compose/postgres
docker-compose up -d
```

### 2. Run the API

```powershell
cd Chd.Pos.Api
dotnet ef database update
dotnet run
```

- API: `http://localhost:5218`
- Swagger: `http://localhost:5218/swagger`

### 3. Start the Frontend

```powershell
cd Chd.Pos.Web.Local.Npm
npm install
npm start
```

Frontend: `http://localhost:3000`

---

## 🔐 Authentication Setup

### IUserTokenProvider Implementation

The demo uses a minimal `UserRepoesitory` class that implements `IUserTokenProvider`. This is the only thing you need to implement for authentication — the login endpoint, JWT signing, and token issuance are all handled internally by `UseAutoUI`.

```csharp
using Chd.AutoUI.Extensions;
using Chd.Security.DTOs;
using Chd.Security.Models;

// In a real application, inject your DbContext and use proper password hashing.
// This demo uses a fixed password for simplicity.
class UserRepoesitory : IUserTokenProvider
{
    public Task<UserDTO?> GetUserTokenInfoAsync(UserModel model)
    {
        if (model.Password != "test")
            return Task.FromResult<UserDTO?>(null);

        var roles = model.UserName switch
        {
            "Admin"   => new List<string> { "User", "Admin" },
            "Manager" => new List<string> { "User", "Manager" },
            "User"    => new List<string> { "User" },
            _         => new List<string>()
        };

        if (!roles.Any())
            return Task.FromResult<UserDTO?>(null);

        return Task.FromResult<UserDTO?>(new UserDTO
        {
            UserName = model.UserName,
            Roles = roles,
            ExpirationSecond = 50000000
        });
    }
}
```

**In a real application you would:**
1. Inject `DbContext` (or a user service) via the constructor
2. Look up the user by username
3. Verify the password against a stored hash (bcrypt, PBKDF2, etc.)
4. Load roles from the database
5. Return `null` for any authentication failure

### Demo Credentials

| Username | Password | Roles |
|---|---|---|
| Admin | test | User, Admin |
| Manager | test | User, Manager |
| User | test | User |

---

## 🔧 Program.cs Walkthrough

```csharp
using Chd.AutoUI.Extensions;
using Chd.Pos.Api.Data;
using Chd.Pos.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Structured logging with Serilog
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("C:\\Temp\\logs\\application-log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

// Database
builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Generic repository — one line for all entities
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow requests from the React dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5218")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// UseAutoUI<UserRepoesitory> does all of this in one call:
//   1. Registers UserRepoesitory as IUserTokenProvider in DI
//   2. Configures JWT middleware (UseJwtTokenAuthorization)
//   3. Calls UseAuthentication() and UseAuthorization()
//   4. Registers GET  /api/metadata
//   5. Registers GET  /api/metadata/{entityName}
//   6. Registers GET  /api/me
//   7. Registers POST /api/account/login
var app = builder.UseAutoUI<UserRepoesitory>(typeof(ProductDto).Assembly);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
```

> **Important:** Do not call `app.UseAuthentication()` or `app.UseAuthorization()` yourself after `UseAutoUI` — the method already does it internally in the correct middleware order.

---

## 🔐 Permission Management Example

Permissions are defined in the DTO via attributes. The role strings you use here must match the roles returned by your `IUserTokenProvider`.

```csharp
// Chd.Pos.Core/DTOs/PosDTOs.cs

[AutoCRUD(Title = "Products", Icon = "shopping-bag", Route = "/products")]
[CreatePermission("Admin", "Manager")]   // Admins and Managers can create
[UpdatePermission("Admin", "Manager")]   // Admins and Managers can edit
[DeletePermission("Admin")]              // Only Admins can delete
public class ProductDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Product Name", Type = FieldType.Text, Required = true, MaxLength = 200, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 150, Format = "currency")]
    [FormField(Label = "Price", Type = FieldType.Number, Required = true, Order = 2)]
    public decimal Price { get; set; }
}
```

When a user logged in as `Manager` views the Products page:
- ✅ They can see the list
- ✅ They can create new products
- ✅ They can edit existing products
- ❌ Delete button is hidden (requires `Admin`)

The React component (`DynamicGrid`) reads the `permissions` field from the metadata and compares it against the current user's roles from `/api/me`. No extra React code is needed.

---

## 🌐 API Endpoints

### Auto-registered by Chd.AutoUI

```bash
# Get metadata for all entities
GET /api/metadata

# Get metadata for a specific entity
GET /api/metadata/ProductDto

# Current user info (requires JWT)
GET /api/me

# Get a JWT token
POST /api/account/login
Content-Type: application/json
{"UserName": "Admin", "Password": "test"}
```

### CRUD Controllers (example — Products)

```bash
# List all
GET /api/products

# Get one
GET /api/products/1

# Create
POST /api/products
Content-Type: application/json
{"name": "iPhone 15", "price": 999.99, "stock": 50, "categoryId": 1}

# Update
PUT /api/products/1
Content-Type: application/json
{"id": 1, "name": "iPhone 15 Pro", "price": 1199.99, "stock": 45, "categoryId": 1}

# Delete
DELETE /api/products/1
```

The same pattern applies to `/api/categories`, `/api/customers`, `/api/suppliers`, `/api/sales`, and `/api/saleitems`.

---

## ⚙️ Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=9999;Database=chd_pos;Username=your_user;Password=your_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Do not commit real credentials. Use environment variables or .NET user secrets in development:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;..."
```

---

## 🛠️ Technologies

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Web framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (Npgsql) |
| Authentication | JWT (via Chd.Security) |
| UI metadata | Chd.AutoUI |
| Data access | Chd.AutoUI.EF |
| Logging | Serilog |
| Frontend | React 18, TypeScript, chd-auto-ui-react |
| API docs | Swagger / OpenAPI |

---

## 🔗 Related Projects

| Project | Description |
|---|---|
| [Chd.AutoUI](https://www.nuget.org/packages/Chd.AutoUI) | NuGet package providing metadata generation and authentication |
| [Chd.AutoUI.EF](https://www.nuget.org/packages/Chd.AutoUI.EF) | Generic EF Core repository used in the controllers |
| [chd-auto-ui-react](https://www.npmjs.com/package/@mehmetyoldas/chd-auto-ui-react) | React package that renders the UI from the metadata |
| [All Demo Projects](https://github.com/mehmet-yoldas/Chd.Examples) | All CHD examples, benchmarks and test projects |

---

## 📄 License

MIT



