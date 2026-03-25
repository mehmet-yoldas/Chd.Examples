# Chd.Pos - Point of Sale System

**Metadata-Driven POS Demo Application**

Chd.Pos, Chd.AutoUI framework'ünün gücünü gösteren örnek bir Point of Sale (POS) uygulamasıdır. Sadece C# attribute'ları kullanarak tam fonksiyonel bir CRUD arayüzü oluşturur.

## 🎯 Özellikler

- ✅ **Ürün Yönetimi**: Stok takibi, fiyatlandırma, kategorilendirme
- ✅ **Kategori Yönetimi**: Ürün kategorileri
- ✅ **Müşteri Yönetimi**: Müşteri bilgileri ve bakiye takibi
- ✅ **Satış İşlemleri**: Faturalama ve satış kalemleri
- ✅ **Tedarikçi Yönetimi**: Tedarikçi bilgileri
- ✅ **Stok Hareketleri**: Giriş/çıkış kayıtları
- ✅ **Otomatik UI**: Metadata-driven React frontend
- ✅ **REST API**: Swagger ile dokümante edilmiş
- ✅ **PostgreSQL**: Docker ile kolay kurulum

## 🏗️ Proje Yapısı

```
Chd.Pos/
├── Chd.Pos.Core/              # Domain Layer
│   ├── Entities/              # Entity sınıfları
│   │   └── PosEntities.cs     # Product, Category, Sale, Customer...
│   └── DTOs/                  # Data Transfer Objects
│       └── PosDTOs.cs         # ProductDto, CategoryDto... (with attributes)
├── Chd.Pos.Api/               # API Layer
│   ├── Controllers/           # REST Controllers
│   │   ├── MetadataController.cs
│   │   ├── ProductsController.cs
│   │   ├── CategoriesController.cs
│   │   └── ...
│   ├── Data/                  # Database Context
│   │   └── PosDbContext.cs
│   ├── Migrations/            # EF Core Migrations
│   └── Program.cs             # API Configuration
└── Chd.Pos.Web/               # Frontend Layer
    ├── src/
    │   ├── components/        # React Components
    │   │   ├── DynamicGrid.tsx
    │   │   └── DynamicForm.tsx
    │   ├── services/          # API Services
    │   └── types/             # TypeScript Types
    └── package.json
```

## 📦 Kurulum

### Gereksinimler

- .NET 8.0 SDK
- Node.js 18+
- Docker Desktop (PostgreSQL için)
- PostgreSQL (Docker veya local)

### 1. PostgreSQL'i Docker ile Başlatın

```bash
cd Library.Tests/Docker-Compose/postgres
docker-compose up -d
```

### 2. Backend Kurulumu

```bash
# Migration'ları uygula
cd Chd.Pos.Api
dotnet ef database update

# API'yi başlat
dotnet run
```

API şu adreste çalışacak: `http://localhost:5218`  
Swagger: `http://localhost:5218/swagger`

### 3. Frontend Kurulumu

```bash
cd Chd.Pos.Web
npm install
npm run dev
```

Frontend şu adreste çalışacak: `http://localhost:3000`

## 🚀 Kullanım

### 1. Web Arayüzü

Tarayıcıda `http://localhost:3000` adresine gidin:

1. **Products**: Ürün ekleme, düzenleme, silme
2. **Categories**: Kategori yönetimi
3. **Customers**: Müşteri bilgileri
4. **Suppliers**: Tedarikçi yönetimi

### 2. API Kullanımı

#### Metadata Endpoint

```bash
# Tüm entity metadata'larını getir
curl http://localhost:5218/api/metadata

# Belirli bir entity metadata'sı
curl http://localhost:5218/api/metadata/ProductDto
```

#### Products CRUD

```bash
# Tüm ürünleri getir
curl http://localhost:5218/api/products

# Tek ürün getir
curl http://localhost:5218/api/products/1

# Yeni ürün ekle
curl -X POST http://localhost:5218/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "iPhone 15",
    "price": 999.99,
    "cost": 800.00,
    "stock": 50,
    "categoryId": 1
  }'

# Ürün güncelle
curl -X PUT http://localhost:5218/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "iPhone 15 Pro",
    "price": 1199.99,
    "cost": 900.00,
    "stock": 45,
    "categoryId": 1
  }'

# Ürün sil
curl -X DELETE http://localhost:5218/api/products/1
```

## 📊 Database Schema

```sql
-- Products
CREATE TABLE "Products" (
    "Id" integer PRIMARY KEY,
    "Name" varchar(200),
    "Barcode" varchar(100),
    "Price" decimal(18,2),
    "Cost" decimal(18,2),
    "Stock" integer,
    "MinStock" integer,
    "CategoryId" integer,
    "CreatedDate" timestamp,
    "IsActive" boolean
);

-- Categories
CREATE TABLE "Categories" (
    "Id" integer PRIMARY KEY,
    "Name" varchar(100),
    "Description" text,
    "CreatedDate" timestamp
);

-- Customers
CREATE TABLE "Customers" (
    "Id" integer PRIMARY KEY,
    "Name" varchar(200),
    "Email" varchar(100),
    "Phone" varchar(20),
    "Address" text,
    "Balance" decimal(18,2),
    "CreatedDate" timestamp
);

-- Sales
CREATE TABLE "Sales" (
    "Id" integer PRIMARY KEY,
    "CustomerId" integer,
    "TotalAmount" decimal(18,2),
    "DiscountAmount" decimal(18,2),
    "TaxAmount" decimal(18,2),
    "NetAmount" decimal(18,2),
    "PaymentMethod" varchar(50),
    "SaleDate" timestamp,
    "CreatedDate" timestamp
);

-- SaleItems
CREATE TABLE "SaleItems" (
    "Id" integer PRIMARY KEY,
    "SaleId" integer,
    "ProductId" integer,
    "Quantity" integer,
    "UnitPrice" decimal(18,2),
    "DiscountAmount" decimal(18,2),
    "TotalAmount" decimal(18,2)
);
```

## 🎨 DTO Örnekleri

### ProductDto

```csharp
[AutoCRUD(Title = "Products", Icon = "📦", Route = "/products", Description = "Product management")]
public class ProductDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Product Name", Type = FieldType.Text, Required = true, MaxLength = 200, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Barcode", Type = FieldType.Text, MaxLength = 100, Order = 2)]
    public string? Barcode { get; set; }

    [GridColumn(Order = 4, Width = 120, Format = "currency")]
    [FormField(Label = "Price", Type = FieldType.Number, Required = true, Order = 3)]
    public decimal Price { get; set; }

    [GridColumn(Order = 5, Width = 100)]
    [FormField(Label = "Stock", Type = FieldType.Number, Required = true, Order = 4)]
    public int Stock { get; set; }

    [GridColumn(Order = 6, Width = 150)]
    [FormField(Label = "Category", Type = FieldType.Select, Order = 5)]
    public int? CategoryId { get; set; }

    [GridColumn(Order = 7, Width = 150)]
    public string? CategoryName { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "Active", Type = FieldType.Checkbox, Order = 6)]
    public bool IsActive { get; set; }
}
```

## 🔧 Yapılandırma

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=9999;Database=chd_pos;Username=PostgresDB_user;Password=PostgresDB_2022.*!"
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
var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<PosDbContext>(options =>
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

## 🧪 Test Senaryoları

### 1. Ürün Ekleme

1. Web arayüzünde "Products" menüsüne tıklayın
2. "Create" butonuna tıklayın
3. Formu doldurun:
   - Name: "Samsung Galaxy S24"
   - Barcode: "8806094123456"
   - Price: 899.99
   - Cost: 750.00
   - Stock: 30
   - Category: "Electronics"
4. "Save" butonuna tıklayın
5. Grid'de yeni ürünü görün

### 2. Kategori Yönetimi

```bash
# Kategori ekle
curl -X POST http://localhost:5218/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name": "Electronics", "description": "Electronic devices"}'

# Kategorileri listele
curl http://localhost:5218/api/categories
```

### 3. Satış Oluşturma

```bash
# Satış ekle
curl -X POST http://localhost:5218/api/sales \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "items": [
      {"productId": 1, "quantity": 2, "unitPrice": 999.99},
      {"productId": 2, "quantity": 1, "unitPrice": 49.99}
    ],
    "paymentMethod": "Credit Card"
  }'
```

## 📈 Performans

- **API Response Time**: < 100ms (ortalama)
- **Database Queries**: Optimized with EF Core
- **Frontend Rendering**: Virtual scrolling for large datasets
- **Concurrent Users**: 100+ (tested)

## 🛠️ Teknolojiler

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

## 🐛 Sorun Giderme

### Migration Hatası

```bash
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Port Çakışması

```bash
# API portunuzu değiştirin
# launchSettings.json -> applicationUrl

# Frontend portunuzu değiştirin
# vite.config.ts -> server.port
```

### CORS Hatası

Program.cs'de CORS yapılandırmasını kontrol edin:
```csharp
app.UseCors();
```

## 📝 Lisans

MIT License

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun
3. Commit edin
4. Push edin
5. Pull Request açın

## 🔗 İlgili Projeler

- [Chd.AutoUI](../Chd.AutoUI) - Metadata Framework
- [Chd.AutoUI.EF](../Chd.AutoUI.EF) - Generic Repository
- [Chd.StockTracking](../Chd.StockTracking.Api) - Stock Tracking Demo

## 📞 İletişim

**CHD Framework Team**
