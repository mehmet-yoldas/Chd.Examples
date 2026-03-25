using Chd.AutoUI.Attributes;

namespace Chd.StockTracking.Core.DTOs;

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
    [FormField(Label = "Stok Miktarı", Type = FieldType.Number, Required = true, Order = 5)]
    public double ProductCount { get; set; }

    [GridColumn(Order = 7, Width = 100)]
    [FormField(Label = "Min Stok", Type = FieldType.Number, Order = 6)]
    public double? MinCount { get; set; }

    [GridColumn(Order = 8, Width = 120)]
    [FormField(Label = "Satılan Miktar", Type = FieldType.Number, ReadOnly = true, Order = 7)]
    public double SelledCount { get; set; }

    [GridColumn(Order = 9, Width = 100)]
    [FormField(Label = "İndirim %", Type = FieldType.Number, Order = 8)]
    public double? Discount { get; set; }

    [GridColumn(Order = 10, Width = 100)]
    [FormField(Label = "KDV %", Type = FieldType.Number, Order = 9)]
    public double? Tax { get; set; }

    [GridColumn(Order = 11, Width = 150)]
    [FormField(Label = "Kategori", Type = FieldType.Dropdown, Order = 10)]
    public int? ProductCategoryId { get; set; }

    [GridColumn(Order = 12, Width = 150)]
    public string? CategoryName { get; set; }

    [GridColumn(Order = 13, Width = 150, Format = "date")]
    [FormField(Label = "Son Kullanma Tarihi", Type = FieldType.Date, Order = 11)]
    public DateTime? ExpirationDate { get; set; }
}

[AutoCRUD(Title = "Categories", Icon = "📁", Route = "/categories")]
public class ProductCategoryDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Kategori Adı", Type = FieldType.Text, Required = true, MaxLength = 250, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 100)]
    [FormField(Label = "İndirim %", Type = FieldType.Number, Order = 2)]
    public double? Discount { get; set; }
}

[AutoCRUD(Title = "Bills", Icon = "🧾", Route = "/bills", Description = "Fatura listesi")]
public class BillDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 180, Format = "datetime")]
    [FormField(Label = "Satış Zamanı", Type = FieldType.DateTime, Required = true, Order = 1)]
    public DateTime TimeOfSelling { get; set; }

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Birim", Type = FieldType.Dropdown, Required = true, Order = 2)]
    public int UnitId { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    public string? UnitName { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    [FormField(Label = "Müşteri", Type = FieldType.Dropdown, Order = 3)]
    public int? CustomerId { get; set; }

    [GridColumn(Order = 6, Width = 150)]
    public string? CustomerName { get; set; }

    [GridColumn(Order = 7, Width = 120)]
    [FormField(Label = "Fatura Tipi", Type = FieldType.Dropdown, Order = 4)]
    public int? BillType { get; set; }

    [GridColumn(Order = 8, Width = 150)]
    [FormField(Label = "Kullanıcı", Type = FieldType.Dropdown, Order = 5)]
    public int? UserId { get; set; }

    [GridColumn(Order = 9, Width = 150)]
    public string? UserName { get; set; }
}

[AutoCRUD(Title = "Sellings", Icon = "💰", Route = "/sellings", Description = "Satış detayları")]
public class SellingDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 150)]
    [FormField(Label = "Ürün", Type = FieldType.Dropdown, Required = true, Order = 1)]
    public long ProductId { get; set; }

    [GridColumn(Order = 3, Width = 200)]
    public string? ProductName { get; set; }

    [GridColumn(Order = 4, Width = 100)]
    [FormField(Label = "Miktar", Type = FieldType.Number, Required = true, Order = 2)]
    public double SellingCount { get; set; }

    [GridColumn(Order = 5, Width = 120, Format = "currency")]
    [FormField(Label = "Fiyat", Type = FieldType.Number, Required = true, Order = 3)]
    public double Price { get; set; }

    [GridColumn(Order = 6, Width = 120, Format = "currency")]
    [FormField(Label = "Alış Fiyatı", Type = FieldType.Number, Required = true, Order = 4)]
    public double PurchasePrice { get; set; }

    [GridColumn(Order = 7, Width = 100)]
    [FormField(Label = "İndirim", Type = FieldType.Number, Order = 5)]
    public double? Discount { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "KDV", Type = FieldType.Number, Order = 6)]
    public double? Tax { get; set; }

    [GridColumn(Order = 9, Width = 180, Format = "datetime")]
    [FormField(Label = "Okuma Zamanı", Type = FieldType.DateTime, Required = true, Order = 7)]
    public DateTime ReadingTime { get; set; }

    [GridColumn(Order = 10, Width = 150)]
    [FormField(Label = "Fatura", Type = FieldType.Dropdown, Required = true, Order = 8)]
    public long BillId { get; set; }
}

[AutoCRUD(Title = "Insertings", Icon = "📥", Route = "/insertings", Description = "Stok giriş hareketleri")]
public class InsertingDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 150)]
    [FormField(Label = "Ürün", Type = FieldType.Dropdown, Required = true, Order = 1)]
    public long ProductId { get; set; }

    [GridColumn(Order = 3, Width = 200)]
    public string? ProductName { get; set; }

    [GridColumn(Order = 4, Width = 100)]
    [FormField(Label = "Miktar", Type = FieldType.Number, Required = true, Order = 2)]
    public double InsertingCount { get; set; }

    [GridColumn(Order = 5, Width = 120, Format = "currency")]
    [FormField(Label = "Fiyat", Type = FieldType.Number, Required = true, Order = 3)]
    public double Price { get; set; }

    [GridColumn(Order = 6, Width = 180, Format = "datetime")]
    [FormField(Label = "Giriş Zamanı", Type = FieldType.DateTime, Required = true, Order = 4)]
    public DateTime Time { get; set; }

    [GridColumn(Order = 7, Width = 150)]
    [FormField(Label = "Birim", Type = FieldType.Dropdown, Required = true, Order = 5)]
    public int UnitId { get; set; }

    [GridColumn(Order = 8, Width = 150)]
    public string? UnitName { get; set; }

    [GridColumn(Order = 9, Width = 150, Format = "date")]
    [FormField(Label = "Son Kullanma Tarihi", Type = FieldType.Date, Order = 6)]
    public DateTime? ExpirationDate { get; set; }
}

[AutoCRUD(Title = "Customers", Icon = "👤", Route = "/customers", Description = "Müşteri listesi")]
public class CustomerDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Müşteri Adı", Type = FieldType.Text, Required = true, MaxLength = 100, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 120)]
    [FormField(Label = "Telefon", Type = FieldType.Text, MaxLength = 50, Order = 2)]
    public string? Phone { get; set; }

    [GridColumn(Order = 4, Width = 200)]
    [FormField(Label = "E-posta", Type = FieldType.Email, MaxLength = 100, Order = 3)]
    public string? Email { get; set; }

    [GridColumn(Order = 5, Width = 120, Format = "currency")]
    [FormField(Label = "Bakiye", Type = FieldType.Number, ReadOnly = true, Order = 4)]
    public double Balance { get; set; }

    [GridColumn(Order = 6, Width = 120)]
    [FormField(Label = "Vergi No", Type = FieldType.Text, MaxLength = 50, Order = 5)]
    public string? TaxNumber { get; set; }

    [GridColumn(Order = 7, Width = 200)]
    [FormField(Label = "Vergi Dairesi", Type = FieldType.Text, MaxLength = 250, Order = 6)]
    public string? TaxAdministration { get; set; }

    [GridColumn(Order = 8, Width = 250)]
    [FormField(Label = "Adres", Type = FieldType.TextArea, MaxLength = 250, Order = 7)]
    public string? Adress { get; set; }

    [GridColumn(Order = 9, Width = 120)]
    [FormField(Label = "İl", Type = FieldType.Text, MaxLength = 250, Order = 8)]
    public string? Province { get; set; }

    [GridColumn(Order = 10, Width = 150)]
    [FormField(Label = "Birim", Type = FieldType.Dropdown, Required = true, Order = 9)]
    public int UnitId { get; set; }

    [GridColumn(Order = 11, Width = 150)]
    public string? UnitName { get; set; }
}

[AutoCRUD(Title = "Units", Icon = "🏢", Route = "/units", Description = "Birim/Şube yönetimi")]
public class UnitDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public long Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Birim Adı", Type = FieldType.Text, Required = true, MaxLength = 250, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 120)]
    [FormField(Label = "Aktif", Type = FieldType.Checkbox, Order = 2)]
    public bool IsActive { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    [FormField(Label = "Üst Birim", Type = FieldType.Dropdown, Order = 3)]
    public int? ParentUnitId { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    public string? ParentUnitName { get; set; }
}
