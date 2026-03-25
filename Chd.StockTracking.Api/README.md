# Chd.StockTracking - Stok Takip Sistemi

**Metadata-Driven Stock Tracking Application**

Chd.StockTracking, Chd.AutoUI framework kullanılarak geliştirilmiş kapsamlı bir stok takip sistemidir. Gerçek bir SQL şemasından dönüştürülerek oluşturulmuştur ve framework'ün farklı domainlerde çalışabilme yeteneğini gösterir.

## 🎯 Özellikler

- ✅ **Ürün Yönetimi**: Barkod, fiyat, stok, kategori, son kullanma tarihi
- ✅ **Kategori Yönetimi**: Ürün kategorileri ve indirim oranları
- ✅ **Fatura Sistemi**: Satış faturaları ve kalemler
- ✅ **Satış Detayları**: Ürün bazlı satış kalemleri, vergi, indirim
- ✅ **Stok Girişleri**: Ürün alımları ve son kullanma tarihi takibi
- ✅ **Müşteri Yönetimi**: Müşteri bilgileri, bakiye, vergi bilgileri
- ✅ **Müşteri Ödemeleri**: Ödeme kayıtları ve bakiye takibi
- ✅ **Kullanıcı Yönetimi**: Kullanıcı rolleri ve yetkilendirme
- ✅ **Birim/Şube Yönetimi**: Hiyerarşik organizasyon yapısı
- ✅ **Otomatik UI**: Türkçe etiketli metadata-driven React frontend
- ✅ **REST API**: Swagger ile dokümante edilmiş
- ✅ **PostgreSQL**: Docker ile kolay kurulum

## 🏗️ Proje Yapısı

```
Chd.StockTracking/
├── Chd.StockTracking.Core/       # Domain Layer
│   ├── Entities/                 # Entity sınıfları
│   │   ├── BaseEntity.cs         # Base entity (long Id)
│   │   └── StockEntities.cs      # Product, Bill, Selling, Customer...
│   └── DTOs/                     # Data Transfer Objects
│       └── StockDTOs.cs          # Türkçe etiketli DTO'lar (with attributes)
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
cd Chd.StockTracking.Api
dotnet ef database update

# API'yi başlat
dotnet run
```

API şu adreste çalışacak: `http://localhost:5057`  
Swagger: `http://localhost:5057/swagger`

### 3. Frontend Kurulumu

```bash
cd Chd.StockTracking.Web
npm install
npm run dev
```

Frontend şu adreste çalışacak: `http://localhost:3001`

## 🚀 Kullanım

### 1. Web Arayüzü

Tarayıcıda `http://localhost:3001` adresine gidin:

1. **Ürünler (Products)** 📦: Ürün ekleme, düzenleme, silme
2. **Kategoriler (ProductCategories)** 📁: Kategori yönetimi
3. **Faturalar (Bills)** 🧾: Satış faturaları
4. **Satışlar (Sellings)** 💰: Satış kalemleri
5. **Girişler (Insertings)** 📥: Stok girişleri
6. **Müşteriler (Customers)** 👤: Müşteri bilgileri
7. **Birimler (Units)** 🏢: Şube/birim yönetimi

### 2. API Kullanımı

#### Metadata Endpoint

```bash
# Tüm entity metadata'larını getir
curl http://localhost:5057/api/metadata

# Belirli bir entity metadata'sı
curl http://localhost:5057/api/metadata/ProductDto
```

#### Products CRUD

```bash
# Tüm ürünleri getir
curl http://localhost:5057/api/products

# Tek ürün getir
curl http://localhost:5057/api/products/1

# Yeni ürün ekle
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

# Ürün güncelle
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

# Ürün sil
curl -X DELETE http://localhost:5057/api/products/1
```

## 📊 Database Schema

### Ürünler (Products)
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

### Kategoriler (ProductCategories)
```sql
CREATE TABLE "ProductCategories" (
    "Id" integer PRIMARY KEY,
    "Name" varchar(250) NOT NULL,
    "BuilderUserId" integer NOT NULL,
    "Discount" double precision,
    "CreationDate" timestamp NOT NULL
);
```

### Faturalar (Bills)
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

### Satışlar (Sellings)
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

### Stok Girişleri (Insertings)
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

### Müşteriler (Customers)
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

### Müşteri Ödemeleri (CustomerPayments)
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

### Birimler (Units)
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

### Kullanıcılar (Users)
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

## 🎨 DTO Örnekleri

### ProductDto (Türkçe Etiketli)

```csharp
[AutoCRUD(Title = "Products", Icon = "📦", Route = "/products", Description = "Ürün listesi ve stok yönetimi")]
public class ProductDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Ürün Adı", Type = FieldType.Text, Required = true, MaxLength = 250, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Barkod", Type = FieldType.Text, Required = true, MaxLength = 250, Order = 2)]
    public string BarcodeNumber { get; set; } = string.Empty;

    [GridColumn(Order = 4, Width = 120, Format = "currency")]
    [FormField(Label = "Alış Fiyatı", Type = FieldType.Number, Required = true, Order = 3)]
    public double PurchasePrice { get; set; }

    [GridColumn(Order = 5, Width = 120, Format = "currency")]
    [FormField(Label = "Satış Fiyatı", Type = FieldType.Number, Required = true, Order = 4)]
    public double Price { get; set; }

    [GridColumn(Order = 6, Width = 100)]
    [FormField(Label = "Stok", Type = FieldType.Number, Required = true, Order = 5)]
    public double ProductCount { get; set; }

    [GridColumn(Order = 7, Width = 100)]
    [FormField(Label = "Min. Stok", Type = FieldType.Number, Order = 6)]
    public double? MinCount { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "Satılan", Type = FieldType.Number, ReadOnly = true, Order = 7)]
    public double SelledCount { get; set; }

    [GridColumn(Order = 9, Width = 100, Format = "percent")]
    [FormField(Label = "İndirim %", Type = FieldType.Number, Order = 8)]
    public double? Discount { get; set; }

    [GridColumn(Order = 10, Width = 100, Format = "percent")]
    [FormField(Label = "KDV %", Type = FieldType.Number, Order = 9)]
    public double? Tax { get; set; }

    [GridColumn(Order = 11, Width = 150)]
    [FormField(Label = "Kategori", Type = FieldType.Select, Order = 10)]
    public int? ProductCategoryId { get; set; }

    [GridColumn(Order = 12, Width = 150)]
    public string? CategoryName { get; set; }

    [GridColumn(Order = 13, Width = 120, Format = "date")]
    [FormField(Label = "Son Kullanma", Type = FieldType.Date, Order = 11)]
    public DateTime? ExpirationDate { get; set; }
}
```

### BillDto (Fatura)

```csharp
[AutoCRUD(Title = "Bills", Icon = "🧾", Route = "/bills", Description = "Satış faturaları")]
public class BillDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 150, Format = "date")]
    [FormField(Label = "Satış Tarihi", Type = FieldType.Date, Required = true, Order = 1)]
    public DateTime TimeOfSelling { get; set; }

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Birim", Type = FieldType.Select, Required = true, Order = 2)]
    public int UnitId { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    public string? UnitName { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    [FormField(Label = "Müşteri", Type = FieldType.Select, Order = 3)]
    public int? CustomerId { get; set; }

    [GridColumn(Order = 6, Width = 150)]
    public string? CustomerName { get; set; }

    [GridColumn(Order = 7, Width = 100)]
    [FormField(Label = "Fatura Tipi", Type = FieldType.Select, Order = 4)]
    public int? BillType { get; set; }

    [GridColumn(Order = 8, Width = 150)]
    [FormField(Label = "Kullanıcı", Type = FieldType.Select, Order = 5)]
    public int? UserId { get; set; }

    [GridColumn(Order = 9, Width = 150)]
    public string? UserName { get; set; }
}
```

### CustomerDto (Müşteri)

```csharp
[AutoCRUD(Title = "Customers", Icon = "👤", Route = "/customers", Description = "Müşteri listesi ve bakiye takibi")]
public class CustomerDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Müşteri Adı", Type = FieldType.Text, Required = true, MaxLength = 100, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 120, Format = "currency")]
    [FormField(Label = "Bakiye", Type = FieldType.Number, Required = true, Order = 2)]
    public double Balance { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    [FormField(Label = "Telefon", Type = FieldType.Text, MaxLength = 50, Order = 3)]
    public string? Phone { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    [FormField(Label = "Vergi No", Type = FieldType.Text, MaxLength = 50, Order = 4)]
    public string? TaxNumber { get; set; }

    [GridColumn(Order = 6, Width = 200)]
    [FormField(Label = "Adres", Type = FieldType.Textarea, MaxLength = 250, Order = 5)]
    public string? Adress { get; set; }

    [GridColumn(Order = 7, Width = 150)]
    [FormField(Label = "Vergi Dairesi", Type = FieldType.Text, MaxLength = 250, Order = 6)]
    public string? TaxAdministration { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "İl", Type = FieldType.Text, MaxLength = 250, Order = 7)]
    public string? Province { get; set; }

    [GridColumn(Order = 9, Width = 150)]
    [FormField(Label = "Birim", Type = FieldType.Select, Required = true, Order = 8)]
    public int UnitId { get; set; }

    [GridColumn(Order = 10, Width = 150)]
    public string? UnitName { get; set; }
}
```

## 🔧 Yapılandırma

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

## 🧪 Test Senaryoları

### 1. Ürün Ekleme ve Stok Takibi

```bash
# 1. Kategori ekle
curl -X POST http://localhost:5057/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name": "Elektronik", "discount": 5.0}'

# 2. Ürün ekle
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

# 3. Stok girişi yap
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

### 2. Fatura ve Satış İşlemi

```bash
# 1. Müşteri ekle
curl -X POST http://localhost:5057/api/customers \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Ahmet Yılmaz",
    "balance": 0,
    "phone": "0532 111 22 33",
    "taxNumber": "1234567890",
    "adress": "İstanbul, Türkiye",
    "unitId": 1
  }'

# 2. Fatura oluştur
curl -X POST http://localhost:5057/api/bills \
  -H "Content-Type: application/json" \
  -d '{
    "timeOfSelling": "2024-03-20T14:30:00Z",
    "unitId": 1,
    "customerId": 1,
    "billType": 1,
    "userId": 1
  }'

# 3. Satış kalemi ekle
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

### 3. Müşteri Bakiye Takibi

```bash
# Müşteri bakiyesini güncelle
curl -X PUT http://localhost:5057/api/customers/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Ahmet Yılmaz",
    "balance": 2399.98,
    "phone": "0532 111 22 33",
    "unitId": 1
  }'

# Ödeme kaydı ekle
curl -X POST http://localhost:5057/api/customerpayments \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "payCost": 1000.00,
    "payType": 1,
    "description": "Nakit ödeme"
  }'
```

## 📈 İş Kuralları

### Stok Yönetimi
- Minimum stok seviyesinin altına düşünce uyarı
- Son kullanma tarihi takibi
- Alış/satış fiyatı kar marjı hesaplama

### Faturalama
- KDV hesaplama (varsayılan %18)
- İndirim uygulamaları
- Müşteri bakiye güncelleme

### Raporlama
- Ürün bazlı satış raporları
- Müşteri bakiye raporları
- Kar/zarar analizi

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
# Migration'ları sıfırla
dotnet ef database drop -f
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### ID Type Uyumsuzluğu

Product, Bill, Selling, Inserting entityleri `long Id` kullanır.  
Customer, User, Unit, ProductCategory entityleri `int Id` kullanır.

GenericRepository `object id` ile her iki tipi de destekler.

### Port Çakışması

```bash
# API portu: launchSettings.json -> applicationUrl (5057)
# Web portu: vite.config.ts -> server.port (3001)
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
- [Chd.Pos](../Chd.Pos.Api) - POS Demo Application

## 📞 İletişim

**CHD Framework Team**

---

**Not:** Bu proje, gerçek bir SQL şemasından dönüştürülerek Chd.AutoUI framework'ünün farklı domainlerde çalışabilme yeteneğini göstermek için oluşturulmuştur.
