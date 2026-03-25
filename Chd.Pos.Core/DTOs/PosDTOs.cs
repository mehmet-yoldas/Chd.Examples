using Chd.AutoUI.Attributes;

namespace Chd.Pos.Core.DTOs;

[AutoCRUD(Title = "Products", Icon = "shopping-bag", Route = "/products", Description = "Manage your product inventory")]
public class ProductDto
{
    [GridColumn(Order = 1, Width = 80, Sortable = false)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Product Name", Type = FieldType.Text, Required = true, MaxLength = 100, Placeholder = "Enter product name", Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 300)]
    [FormField(Label = "Description", Type = FieldType.TextArea, MaxLength = 500, Placeholder = "Product description", Order = 2)]
    public string? Description { get; set; }

    [GridColumn(Order = 4, Width = 120)]
    [FormField(Label = "Barcode", Type = FieldType.Text, MaxLength = 50, Order = 3)]
    public string? Barcode { get; set; }

    [GridColumn(Order = 5, Width = 100)]
    [FormField(Label = "SKU", Type = FieldType.Text, MaxLength = 50, Order = 4)]
    public string? SKU { get; set; }

    [GridColumn(Order = 6, Width = 120, Format = "currency")]
    [FormField(Label = "Price", Type = FieldType.Number, Required = true, Order = 5)]
    public decimal Price { get; set; }

    [GridColumn(Order = 7, Width = 120, Format = "currency")]
    [FormField(Label = "Cost Price", Type = FieldType.Number, Order = 6)]
    public decimal? CostPrice { get; set; }

    [GridColumn(Order = 8, Width = 100)]
    [FormField(Label = "Stock Quantity", Type = FieldType.Number, Required = true, Order = 7)]
    public int StockQuantity { get; set; }

    [GridColumn(Order = 9, Width = 100)]
    [FormField(Label = "Min Stock Level", Type = FieldType.Number, Order = 8)]
    public int? MinStockLevel { get; set; }

    [GridColumn(Order = 10, Width = 80)]
    [FormField(Label = "Unit", Type = FieldType.Text, MaxLength = 20, Placeholder = "pcs, kg, liter", Order = 9)]
    public string? Unit { get; set; }

    // DROPDOWN EXAMPLE - Foreign key (auto-detected)
    [GridColumn(Order = 11, Width = 150)]
    [FormField(Label = "Category", Type = FieldType.Dropdown, Order = 10, RelatedEntity = "categories")]
    public int? CategoryId { get; set; }

    [GridColumn(Order = 12, Width = 150)]
    public string? CategoryName { get; set; }

    // DROPDOWN EXAMPLE - Foreign key for supplier (auto-detected)
    [GridColumn(Order = 13, Width = 150)]
    [FormField(Label = "Supplier", Type = FieldType.Dropdown, Order = 11, RelatedEntity = "suppliers")]
    public int? SupplierId { get; set; }

    [GridColumn(Order = 14, Width = 150)]
    public string? SupplierName { get; set; }

    // RADIO EXAMPLE - Static options
    [GridColumn(Order = 15, Width = 120)]
    [FormField(Label = "Status", Type = FieldType.Radio, Required = true, Order = 12, 
        Options = new[] { "Active", "Inactive", "Discontinued" })]
    public string Status { get; set; } = "Active";

    // CHECKBOX EXAMPLES
    [GridColumn(Order = 16, Width = 100)]
    [FormField(Label = "Featured Product", Type = FieldType.Checkbox, Order = 13)]
    public bool IsFeatured { get; set; }

    [GridColumn(Order = 17, Width = 100)]
    [FormField(Label = "Taxable", Type = FieldType.Checkbox, Order = 14)]
    public bool IsTaxable { get; set; } = true;

    // FILE UPLOAD EXAMPLE
    [GridColumn(Order = 18, Width = 200)]
    [FormField(Label = "Product Image", Type = FieldType.File, Order = 15, Accept = "image/*")]
    public string? ImageUrl { get; set; }

    // MULTISELECT EXAMPLE (for now using comma-separated string)
    [FormField(Label = "Tags", Type = FieldType.MultiSelect, Order = 16, Placeholder = "Select tags",
        Options = new[] { "Organic", "Vegan", "Gluten-Free", "Dairy-Free", "Sugar-Free", "Local" })]
    public string? Tags { get; set; }

    // DATETIME EXAMPLES
    [GridColumn(Order = 19, Width = 150, Format = "datetime")]
    [FormField(Label = "Expiry Date", Type = FieldType.DateTime, Order = 17)]
    public DateTime? ExpiryDate { get; set; }

    [FormField(Label = "Manufacturing Date", Type = FieldType.DateTime, Order = 18)]
    public DateTime? ManufacturingDate { get; set; }
}

[AutoCRUD(Title = "Categories", Icon = "folder", Route = "/categories")]
public class CategoryDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Category Name", Type = FieldType.Text, Required = true, MaxLength = 100, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 300)]
    [FormField(Label = "Description", Type = FieldType.TextArea, MaxLength = 500, Order = 2)]
    public string? Description { get; set; }

    [GridColumn(Order = 4, Width = 150)]
    [FormField(Label = "Parent Category", Type = FieldType.Dropdown, Order = 3, RelatedEntity = "categories")]
    public int? ParentCategoryId { get; set; }

    [GridColumn(Order = 5, Width = 150)]
    public string? ParentCategoryName { get; set; }
}

[AutoCRUD(Title = "Customers", Icon = "users", Route = "/customers")]
public class CustomerDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 150)]
    [FormField(Label = "First Name", Type = FieldType.Text, Required = true, MaxLength = 50, Order = 1)]
    public string FirstName { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Last Name", Type = FieldType.Text, Required = true, MaxLength = 50, Order = 2)]
    public string LastName { get; set; } = string.Empty;

    [GridColumn(Order = 4, Width = 200)]
    [FormField(Label = "Email", Type = FieldType.Email, MaxLength = 100, Order = 3)]
    public string? Email { get; set; }

    [GridColumn(Order = 5, Width = 130)]
    [FormField(Label = "Phone", Type = FieldType.Text, MaxLength = 20, Order = 4)]
    public string? Phone { get; set; }

    [GridColumn(Order = 6, Width = 250)]
    [FormField(Label = "Address", Type = FieldType.TextArea, MaxLength = 300, Order = 5)]
    public string? Address { get; set; }

    [GridColumn(Order = 7, Width = 120)]
    [FormField(Label = "Tax Number", Type = FieldType.Text, MaxLength = 20, Order = 6)]
    public string? TaxNumber { get; set; }
}

[AutoCRUD(Title = "Suppliers", Icon = "truck", Route = "/suppliers")]
public class SupplierDto
{
    [GridColumn(Order = 1, Width = 80)]
    [FormField(ReadOnly = true)]
    public int Id { get; set; }

    [GridColumn(Order = 2, Width = 200)]
    [FormField(Label = "Company Name", Type = FieldType.Text, Required = true, MaxLength = 100, Order = 1)]
    public string Name { get; set; } = string.Empty;

    [GridColumn(Order = 3, Width = 150)]
    [FormField(Label = "Contact Person", Type = FieldType.Text, MaxLength = 100, Order = 2)]
    public string? ContactPerson { get; set; }

    [GridColumn(Order = 4, Width = 200)]
    [FormField(Label = "Email", Type = FieldType.Email, MaxLength = 100, Order = 3)]
    public string? Email { get; set; }

    [GridColumn(Order = 5, Width = 130)]
    [FormField(Label = "Phone", Type = FieldType.Text, MaxLength = 20, Order = 4)]
    public string? Phone { get; set; }

    [GridColumn(Order = 6, Width = 250)]
    [FormField(Label = "Address", Type = FieldType.TextArea, MaxLength = 300, Order = 5)]
    public string? Address { get; set; }

    [GridColumn(Order = 7, Width = 120)]
    [FormField(Label = "Tax Number", Type = FieldType.Text, MaxLength = 20, Order = 6)]
    public string? TaxNumber { get; set; }
}
