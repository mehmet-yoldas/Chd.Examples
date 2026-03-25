namespace Chd.Pos.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public string? SKU { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? MinStockLevel { get; set; }
    public string? Unit { get; set; }

    // Dropdown example - Foreign key
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // Dropdown example - Foreign key for supplier
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    // Radio/Dropdown example - Static options (Status enum will be string)
    public string Status { get; set; } = "Active"; // Active, Inactive, Discontinued

    // Checkbox example
    public bool IsFeatured { get; set; }
    public bool IsTaxable { get; set; } = true;

    // File upload example
    public string? ImageUrl { get; set; }

    // MultiSelect example - Tags (comma separated for now)
    public string? Tags { get; set; } // e.g., "organic,vegan,gluten-free"

    // DateTime example
    public DateTime? ExpiryDate { get; set; }
    public DateTime? ManufacturingDate { get; set; }
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

public class Sale : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string Status { get; set; } = "Completed";
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
}

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string MovementType { get; set; } = "In";
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}
