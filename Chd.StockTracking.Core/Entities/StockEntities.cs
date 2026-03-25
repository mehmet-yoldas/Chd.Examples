namespace Chd.StockTracking.Core.Entities;

public class Product : BaseEntity
{
    public int? ProductCategoryId { get; set; }
    public long? PictureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double PurchasePrice { get; set; }
    public string BarcodeNumber { get; set; } = string.Empty;
    public double Price { get; set; }
    public double ProductCount { get; set; }
    public double? MinCount { get; set; }
    public double SelledCount { get; set; }
    public int BuilderUserId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int? ExpirationAlert { get; set; }
    public bool? Exp { get; set; }
    public bool? IsDeleted { get; set; }
    public double? Discount { get; set; }
    public DateTime? ModificationDate { get; set; }
    public double? Tax { get; set; }
    public int? Desi { get; set; }
    public string? Code { get; set; }

    public ProductCategory? ProductCategory { get; set; }
    public ICollection<Selling> Sellings { get; set; } = new List<Selling>();
    public ICollection<Inserting> Insertings { get; set; } = new List<Inserting>();
}

public class ProductCategory
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public string Name { get; set; } = string.Empty;
    public int BuilderUserId { get; set; }
    public double? Discount { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Bill : BaseEntity
{
    public DateTime TimeOfSelling { get; set; }
    public int UnitId { get; set; }
    public int? CustomerId { get; set; }
    public int? BillType { get; set; }
    public int? UserId { get; set; }

    public Unit Unit { get; set; } = null!;
    public Customer? Customer { get; set; }
    public User? User { get; set; }
    public ICollection<Selling> Sellings { get; set; } = new List<Selling>();
}

public class Selling : BaseEntity
{
    public double SellingCount { get; set; }
    public long ProductId { get; set; }
    public double Price { get; set; }
    public double PurchasePrice { get; set; }
    public DateTime ReadingTime { get; set; }
    public long BillId { get; set; }
    public double? Tax { get; set; }
    public int? Status { get; set; }
    public double? Discount { get; set; }

    public Product Product { get; set; } = null!;
    public Bill Bill { get; set; } = null!;
}

public class Inserting : BaseEntity
{
    public long ProductId { get; set; }
    public double InsertingCount { get; set; }
    public double Price { get; set; }
    public DateTime Time { get; set; }
    public int UnitId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public byte? IntertingType { get; set; }

    public Product Product { get; set; } = null!;
    public Unit Unit { get; set; } = null!;
}

public class Customer
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public int UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Balance { get; set; }
    public string? Description { get; set; }
    public string? TaxNumber { get; set; }
    public string? Adress { get; set; }
    public string? Phone { get; set; }
    public string? TaxAdministration { get; set; }
    public string? Province { get; set; }

    public Unit Unit { get; set; } = null!;
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<CustomerPayment> Payments { get; set; } = new List<CustomerPayment>();
}

public class CustomerPayment : BaseEntity
{
    public int CustomerId { get; set; }
    public double PayCost { get; set; }
    public byte PayType { get; set; }
    public string? Description { get; set; }
    public string? SignCode { get; set; }

    public Customer Customer { get; set; } = null!;
}

public class User
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public int BuilderUserId { get; set; }
    public int UnitId { get; set; }
    public bool IsActive { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short UserTypeId { get; set; }
    public string? DeviceId { get; set; }
    public string? Culture { get; set; }
    public bool? IsLisanced { get; set; }
    public DateTime? LisanceExpirationDate { get; set; }
    public string? MobilePhone { get; set; }
    public short? ProvinceId { get; set; }

    public Unit Unit { get; set; } = null!;
    public UserType UserType { get; set; } = null!;
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
}

public class UserType
{
    public short Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}

public class Unit
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public int BuilderUserId { get; set; }
    public int? ParentUnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<Inserting> Insertings { get; set; } = new List<Inserting>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
