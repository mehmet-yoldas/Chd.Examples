# Chd.StockTracking - Stock Tracking System

**Metadata-Driven Stock Tracking Application**

Chd.StockTracking is a comprehensive stock tracking system built using the Chd.AutoUI framework. It was created by converting a real SQL schema and demonstrates the framework's ability to work across different domains.

## 🎯 Features

- ✅ **Product Management**: Barcode, pricing, stock, categories, expiration dates
- ✅ **Category Management**: Product categories with discount rates
- ✅ **Invoice System**: Sales invoices and line items
- ✅ **Sales Details**: Product-based sales items, tax, discounts
- ✅ **Stock Entries**: Product purchases and expiration date tracking
- ✅ **Customer Management**: Customer information, balance, tax details
- ✅ **Customer Payments**: Payment records and balance tracking
- ✅ **User Management**: User roles and authorization
- ✅ **Unit/Branch Management**: Hierarchical organization structure
- ✅ **Auto-Generated UI**: Metadata-driven React frontend
- ✅ **REST API**: Documented with Swagger
- ✅ **PostgreSQL**: Easy setup with Docker

## 🏗️ Project Structure

```
Chd.StockTracking/
├── Chd.StockTracking.Core/       # Domain Layer
│   ├── Entities/                 # Entity classes
│   │   ├── BaseEntity.cs         # Base entity (long Id)
│   │   └── StockEntities.cs      # Product, Bill, Selling, Customer...
│   └── DTOs/                     # Data Transfer Objects
│       └── StockDTOs.cs          # DTOs with AutoCRUD attributes
├── Chd.StockTracking.Api/        # API Layer
│   ├── Controllers/              # REST Controllers
│   │   ├── MetadataController.cs
│   │   ├── ProductsController.cs
│   │   └── ...
│   ├── Data/                     # Database Context
│   │   └── StockTrackingDbContext.cs
│   ├── Migrations/               # EF Core Migrations
│   └── Program.cs                # API Configuration
└── Chd.StockTracking.Web/        # Frontend Layer
    ├── src/
    │   ├── components/           # React Components
    │   │   ├── DynamicGrid.tsx
    │   │   └── DynamicForm.tsx
    │   ├── services/             # API Services
    │   └── types/                # TypeScript Types
    └── package.json
```

## 📦 Installation

### Requirements

- .NET 8.0 SDK
- Node.js 18+
- Docker Desktop (for PostgreSQL)
- PostgreSQL (Docker or local)

### 1. Start PostgreSQL with Docker

```bash
cd Library.Tests/Docker-Compose/postgres
docker-compose up -d
```

### 2. Backend Setup

```bash
# Apply migrations
cd Chd.StockTracking.Api
dotnet ef database update

# Start API
dotnet run
```

API will run at: `http://localhost:5057`  
Swagger: `http://localhost:5057/swagger`

### 3. Frontend Setup

```bash
cd Chd.StockTracking.Web
npm install
npm run dev
```

Frontend will run at: `http://localhost:3001`

## 🚀 Usage

### 1. Web Interface

Open `http://localhost:3001` in your browser:

1. **Products** 📦: Add, edit, delete products
2. **Product Categories** 📁: Category management
3. **Bills** 🧾: Sales invoices
4. **Sellings** 💰: Sales line items
5. **Insertings** 📥: Stock entries
6. **Customers** 👤: Customer information
7. **Units** 🏢: Branch/unit management

### 2. API Usage

#### Metadata Endpoint

```bash
# Get all entity metadata
curl http://localhost:5057/api/metadata

# Get specific entity metadata
curl http://localhost:5057/api/metadata/ProductDto
```

#### Products CRUD

```bash
# Get all products
curl http://localhost:5057/api/products

# Get single product
curl http://localhost:5057/api/products/1

# Create new product
curl -X POST http://localhost:5057/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Samsung Galaxy S24",
    "barcodeNumber": "8806094123456",
    "purchasePrice": 750.00,
    "price": 899.99,
    "productCount": 50,
    "minCount": 10,
    "tax": 18.0,
    "discount": 5.0,
    "productCategoryId": 1
  }'

# Update product
curl -X PUT http://localhost:5057/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Samsung Galaxy S24 Ultra",
    "barcodeNumber": "8806094123456",
    "purchasePrice": 800.00,
    "price": 999.99,
    "productCount": 45,
    "minCount": 10,
    "tax": 18.0,
    "discount": 5.0,
    "productCategoryId": 1
  }'

# Delete product
curl -X DELETE http://localhost:5057/api/products/1
```

## 📊 Database Schema

### Products
```sql
CREATE TABLE "Products" (
    "Id" bigint PRIMARY KEY,
    "Name" varchar(250) NOT NULL,
    "BarcodeNumber" varchar(250) NOT NULL,
    "PurchasePrice" double precision NOT NULL,
    "Price" double precision NOT NULL,
    "ProductCount" double precision NOT NULL,
    "MinCount" double precision,
    "SelledCount" double precision NOT NULL,
    "Discount" double precision,
    "Tax" double precision,
    "ProductCategoryId" integer,
    "BuilderUserId" integer NOT NULL,
    "ExpirationDate" timestamp,
    "Code" varchar(50),
    "CreationDate" timestamp NOT NULL
);
```

### Product Categories
```sql
CREATE TABLE "ProductCategories" (
    "Id" integer PRIMARY KEY,
    "Name" varchar(250) NOT NULL,
    "BuilderUserId" integer NOT NULL,
    "Discount" double precision,
    "CreationDate" timestamp NOT NULL
);
```

### Bills (Invoices)
```sql
CREATE TABLE "Bills" (
    "Id" bigint PRIMARY KEY,
    "TimeOfSelling" timestamp NOT NULL,
    "UnitId" integer NOT NULL,
    "CustomerId" integer,
    "BillType" integer,
    "UserId" integer,
    "CreationDate" timestamp NOT NULL,
    FOREIGN KEY ("UnitId") REFERENCES "Units"("Id"),
    FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id"),
    FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);
```

### Sellings (Sales Line Items)
```sql
CREATE TABLE "Sellings" (
    "Id" bigint PRIMARY KEY,
    "SellingCount" double precision NOT NULL,
    "ProductId" bigint NOT NULL,
    "Price" double precision NOT NULL,
    "PurchasePrice" double precision NOT NULL,
    "ReadingTime" timestamp NOT NULL,
    "BillId" bigint NOT NULL,
    "Tax" double precision,
    "Status" integer,
    "Discount" double precision,
    "CreationDate" timestamp NOT NULL,
    FOREIGN KEY ("ProductId") REFERENCES "Products"("Id"),
    FOREIGN KEY ("BillId") REFERENCES "Bills"("Id")
);
```

### Insertings (Stock Entries)
```sql
CREATE TABLE "Insertings" (
    "Id" integer PRIMARY KEY,
    "ProductId" bigint NOT NULL,
    "InsertingCount" double precision NOT NULL,
    "Price" double precision NOT NULL,
    "Time" timestamp NOT NULL,
    "UnitId" integer NOT NULL,
    "ExpirationDate" timestamp,
    "IntertingType" smallint,
    "CreationDate" timestamp NOT NULL,
    FOREIGN KEY ("ProductId") REFERENCES "Products"("Id"),
    FOREIGN KEY ("UnitId") REFERENCES "Units"("Id")
);
```

### Customers
```sql
CREATE TABLE "Customers" (
    "Id" integer PRIMARY KEY,
    "UnitId" integer NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Balance" double precision NOT NULL,
    "Phone" varchar(50),
    "TaxNumber" varchar(50),
    "Adress" varchar(250),
    "TaxAdministration" varchar(250),
    "Province" varchar(250),
    "CreationDate" timestamp NOT NULL,
    FOREIGN KEY ("UnitId") REFERENCES "Units"("Id")
);
```

### Customer Payments
```sql
CREATE TABLE "CustomerPayments" (
    "Id" bigint PRIMARY KEY,
    "CustomerId" integer NOT NULL,
    "PayCost" double precision NOT NULL,
    "PayType" smallint NOT NULL,
    "Description" text,
    "SignCode" varchar(50),
    "CreationDate" timestamp NOT NULL,
    FOREIGN KEY ("CustomerId") REFERENCES "Customers"("Id")
);
```

### Units (Branches/Organizations)
```sql
CREATE TABLE "Units" (
    "Id" integer PRIMARY KEY,
    "BuilderUserId" integer NOT NULL,
    "ParentUnitId" integer,
    "Name" varchar(250) NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreationDate" timestamp NOT NULL
);
```

### Users
```sql
CREATE TABLE "Users" (
    "Id" integer PRIMARY KEY,
    "BuilderUserId" integer NOT NULL,
    "UnitId" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "Email" varchar(250) NOT NULL,
    "Password" varchar(100) NOT NULL,
    "Name" varchar(250) NOT NULL,
    "UserTypeId" smallint NOT NULL,
    "Culture" varchar(10),
    "MobilePhone" varchar(50),
    "CreationDate" timestamp NOT NULL,
    FOREIGN KEY ("UnitId") REFERENCES "Units"("Id"),
    FOREIGN KEY ("UserTypeId") REFERENCES "UserTypes"("Id")
);
```

## 🎨 DTO Examples

### ProductDto

```csharp
[AutoCRUD(Title = "Products", Icon = "📦", Route = "/products", Description = "Product list and stock management")]
public class ProductDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Product Name", Type = FieldType.Text, Required = true, MaxLength = 250, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Barcode", Type = FieldType.Text, Required = true, MaxLength = 250, Order = 2)]
    public string BarcodeNumber { get; set; } = string.Empty;

    [GridColumn(Order = 4, Width = 120, Format = "currency")]
    [FormField(Label = "Purchase Price", Type = FieldType.Number, Required = true, Order = 3)]
    public double PurchasePrice { get; set; }

    [GridColumn(Order = 5, Width = 120, Format = "currency")]
    [FormField(Label = "Sales Price", Type = FieldType.Number, Required = true, Order = 4)]
    public double Price { get; set; }

    [GridColumn(Order = 6, Width = 100)]
    [FormField(Label = "Stock", Type = FieldType.Number, Required = true, Order = 5)]
    public double ProductCount { get; set; }

    [GridColumn(Order = 7, Width = 100)]
    [FormField(Label = "Min. Stock", Type = FieldType.Number, Order = 6)]
    public double? MinCount { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "Sold", Type = FieldType.Number, ReadOnly = true, Order = 7)]
    public double SelledCount { get; set; }

    [GridColumn(Order = 9, Width = 100, Format = "percent")]
    [FormField(Label = "Discount %", Type = FieldType.Number, Order = 8)]
    public double? Discount { get; set; }

    [GridColumn(Order = 10, Width = 100, Format = "percent")]
    [FormField(Label = "Tax %", Type = FieldType.Number, Order = 9)]
    public double? Tax { get; set; }

    [GridColumn(Order = 11, Width = 150)]
    [FormField(Label = "Category", Type = FieldType.Select, Order = 10)]
    public int? ProductCategoryId { get; set; }

    [GridColumn(Order = 12, Width = 150)]
    public string? CategoryName { get; set; }

    [GridColumn(Order = 13, Width = 120, Format = "date")]
    [FormField(Label = "Expiration Date", Type = FieldType.Date, Order = 11)]
    public DateTime? ExpirationDate { get; set; }
}
```

### BillDto (Invoice)

```csharp
[AutoCRUD(Title = "Bills", Icon = "🧾", Route = "/bills", Description = "Sales invoices")]
public class BillDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 150, Format = "date")]
    [FormField(Label = "Sale Date", Type = FieldType.Date, Required = true, Order = 1)]
    public DateTime TimeOfSelling { get; set; }

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Unit", Type = FieldType.Select, Required = true, Order = 2)]
    public int UnitId { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    public string? UnitName { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    [FormField(Label = "Customer", Type = FieldType.Select, Order = 3)]
    public int? CustomerId { get; set; }

    [GridColumn(Order = 6, Width = 150)]
    public string? CustomerName { get; set; }

    [GridColumn(Order = 7, Width = 100)]
    [FormField(Label = "Bill Type", Type = FieldType.Select, Order = 4)]
    public int? BillType { get; set; }

    [GridColumn(Order = 8, Width = 150)]
    [FormField(Label = "User", Type = FieldType.Select, Order = 5)]
    public int? UserId { get; set; }

    [GridColumn(Order = 9, Width = 150)]
    public string? UserName { get; set; }
}
```

### CustomerDto

```csharp
[AutoCRUD(Title = "Customers", Icon = "👤", Route = "/customers", Description = "Customer list and balance tracking")]
public class CustomerDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Customer Name", Type = FieldType.Text, Required = true, MaxLength = 100, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 120, Format = "currency")]
    [FormField(Label = "Balance", Type = FieldType.Number, Required = true, Order = 2)]
    public double Balance { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    [FormField(Label = "Phone", Type = FieldType.Text, MaxLength = 50, Order = 3)]
    public string? Phone { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    [FormField(Label = "Tax Number", Type = FieldType.Text, MaxLength = 50, Order = 4)]
    public string? TaxNumber { get; set; }

    [GridColumn(Order = 6, Width = 200)]
    [FormField(Label = "Address", Type = FieldType.Textarea, MaxLength = 250, Order = 5)]
    public string? Adress { get; set; }

    [GridColumn(Order = 7, Width = 150)]
    [FormField(Label = "Tax Office", Type = FieldType.Text, MaxLength = 250, Order = 6)]
    public string? TaxAdministration { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "Province", Type = FieldType.Text, MaxLength = 250, Order = 7)]
    public string? Province { get; set; }

    [GridColumn(Order = 9, Width = 150)]
    [FormField(Label = "Unit", Type = FieldType.Select, Required = true, Order = 8)]
    public int UnitId { get; set; }

    [GridColumn(Order = 10, Width = 150)]
    public string? UnitName { get; set; }
}
```

## 🔧 Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=9999;Database=chd_stock;Username=PostgresDB_user;Password=PostgresDB_2022.*!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Program.cs

```csharp
using Chd.AutoUI.EF.Services;
using Chd.StockTracking.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<StockTrackingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Generic Repository
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

## 🧪 Test Scenarios

### 1. Product Addition and Stock Tracking

```bash
# 1. Add category
curl -X POST http://localhost:5057/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name": "Electronics", "discount": 5.0}'

# 2. Add product
curl -X POST http://localhost:5057/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "iPhone 15 Pro",
    "barcodeNumber": "0194253000000",
    "purchasePrice": 900.00,
    "price": 1199.99,
    "productCount": 50,
    "minCount": 10,
    "tax": 18.0,
    "discount": 5.0,
    "productCategoryId": 1,
    "expirationDate": null
  }'

# 3. Add stock entry
curl -X POST http://localhost:5057/api/insertings \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "insertingCount": 100,
    "price": 850.00,
    "time": "2024-03-20T10:00:00Z",
    "unitId": 1
  }'
```

### 2. Invoice and Sales Transaction

```bash
# 1. Add customer
curl -X POST http://localhost:5057/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "balance": 0,
    "phone": "+1 555 123 4567",
    "taxNumber": "1234567890",
    "adress": "New York, USA",
    "unitId": 1
  }'

# 2. Create invoice
curl -X POST http://localhost:5057/api/bills \
  -H "Content-Type: application/json" \
  -d '{
    "timeOfSelling": "2024-03-20T14:30:00Z",
    "unitId": 1,
    "customerId": 1,
    "billType": 1,
    "userId": 1
  }'

# 3. Add sale line item
curl -X POST http://localhost:5057/api/sellings \
  -H "Content-Type: application/json" \
  -d '{
    "sellingCount": 2,
    "productId": 1,
    "price": 1199.99,
    "purchasePrice": 900.00,
    "billId": 1,
    "tax": 18.0,
    "discount": 5.0
  }'
```

### 3. Customer Balance Tracking

```bash
# Update customer balance
curl -X PUT http://localhost:5057/api/customers/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "John Doe",
    "balance": 2399.98,
    "phone": "+1 555 123 4567",
    "unitId": 1
  }'

# Add payment record
curl -X POST http://localhost:5057/api/customerpayments \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "payCost": 1000.00,
    "payType": 1,
    "description": "Cash payment"
  }'
```

## 📈 Business Rules

### Stock Management
- Alert when stock drops below minimum level
- Expiration date tracking
- Purchase/sales price margin calculation

### Invoicing
- Tax calculation (default 18%)
- Discount applications
- Customer balance updates

### Reporting
- Product-based sales reports
- Customer balance reports
- Profit/loss analysis

## 🛠️ Technologies

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0.11
- PostgreSQL (Npgsql 8.0.11)
- Chd.AutoUI Framework
- Chd.AutoUI.EF (Generic Repository)

### Frontend
- React 18.2.0
- TypeScript 5.3.3
- Vite 5.1.5
- Axios 1.6.7
- React Router 6.22.0

### DevOps
- Docker (PostgreSQL container)
- Swagger/OpenAPI

## 🐛 Troubleshooting

### Migration Errors

```bash
# Reset migrations
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### ID Type Mismatch

Product, Bill, Selling, Inserting entities use `long Id`.  
Customer, User, Unit, ProductCategory entities use `int Id`.

GenericRepository supports both types with `object id`.

### Port Conflicts

```bash
# API port: launchSettings.json -> applicationUrl (5057)
# Web port: vite.config.ts -> server.port (3001)
```

## 📝 License

MIT License

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 🔗 Related Projects

- [Chd.AutoUI](../Chd.AutoUI) - Metadata Framework
- [Chd.AutoUI.EF](../Chd.AutoUI.EF) - Generic Repository
- [Chd.Pos](../Chd.Pos.Api) - POS Demo Application

## 📞 Contact

**CHD Framework Team**

---

**Note:** This project was created by converting a real SQL schema to demonstrate the Chd.AutoUI framework's ability to work across different domains.
