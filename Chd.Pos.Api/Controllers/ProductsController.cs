using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chd.Pos.Api.Data;
using Chd.Pos.Core.Entities;
using Chd.Pos.Core.DTOs;
using System.Text.Json;
using System.Text;
using System.IO;
using System.Linq;
using Chd.AutoUI.Attributes;
using Microsoft.Extensions.Logging;

namespace Chd.Pos.Api.Controllers;

[ApiController]
[Route("api/products")]
//[Microsoft.AspNetCore.Authorization.Authorize] // Authorization temporarily removed for testing
public class ProductsController : ControllerBase
{
    private readonly PosDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(PosDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
        _logger.LogInformation("Fetching all products.");
        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Barcode = p.Barcode,
                SKU = p.SKU,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                MinStockLevel = p.MinStockLevel,
                Unit = p.Unit,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                IsTaxable = p.IsTaxable,
                ImageUrl = p.ImageUrl,
                Tags = p.Tags,
                ExpiryDate = p.ExpiryDate,
                ManufacturingDate = p.ManufacturingDate
            })
            .ToListAsync();

        _logger.LogInformation("Fetched {Count} products.", products.Count);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        _logger.LogInformation("Fetching product with ID {Id}.", id);
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Barcode = p.Barcode,
                SKU = p.SKU,
                Price = p.Price,
                CostPrice = p.CostPrice,
                StockQuantity = p.StockQuantity,
                MinStockLevel = p.MinStockLevel,
                Unit = p.Unit,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : null,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                IsTaxable = p.IsTaxable,
                ImageUrl = p.ImageUrl,
                Tags = p.Tags,
                ExpiryDate = p.ExpiryDate,
                ManufacturingDate = p.ManufacturingDate
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            _logger.LogWarning("Product with ID {Id} not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Fetched product with ID {Id}.", id);
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductDto dto)
    {
        _logger.LogInformation("Creating a new product.");
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid product data provided.");
            return BadRequest(ModelState);
        }

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Barcode = dto.Barcode,
            SKU = dto.SKU,
            Price = dto.Price,
            CostPrice = dto.CostPrice,
            StockQuantity = dto.StockQuantity,
            MinStockLevel = dto.MinStockLevel,
            Unit = dto.Unit,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            Status = dto.Status ?? "Active",
            IsFeatured = dto.IsFeatured,
            IsTaxable = dto.IsTaxable,
            ImageUrl = dto.ImageUrl,
            Tags = dto.Tags,
            ExpiryDate = dto.ExpiryDate,
            ManufacturingDate = dto.ManufacturingDate,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created product with ID {Id}.", product.Id);
        dto.Id = product.Id;
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
    }

    [HttpPut("{id}")]
    // [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Manager")] // Temporarily disabled for testing
    public async Task<IActionResult> Update(int id, [FromBody] System.Text.Json.JsonElement body)
    {
        _logger.LogInformation("Update method called for Product ID {Id}.", id);

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product with ID {Id} not found.", id);
            return NotFound();
        }

        var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        System.Text.Json.Nodes.JsonNode? node = null;
        try
        {
            node = System.Text.Json.Nodes.JsonNode.Parse(body.GetRawText()) ?? new System.Text.Json.Nodes.JsonObject();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error parsing JSON body: {Message}", ex.Message);
            node = new System.Text.Json.Nodes.JsonObject();
        }

        try
        {
            var tagsKey = node.AsObject().FirstOrDefault(p => string.Equals(p.Key, "tags", StringComparison.OrdinalIgnoreCase)).Key;
            if (!string.IsNullOrEmpty(tagsKey))
            {
                var tagsNode = node[tagsKey];
                if (tagsNode is System.Text.Json.Nodes.JsonArray arr)
                {
                    var items = arr.Select(x => x?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s));
                    node[tagsKey] = string.Join(',', items);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing tags: {Message}", ex.Message);
        }

        var dto = System.Text.Json.JsonSerializer.Deserialize<ProductDto>(node.ToJsonString(), jsonOptions) ?? new ProductDto();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Barcode = dto.Barcode;
        product.SKU = dto.SKU;
        product.Price = dto.Price;
        product.CostPrice = dto.CostPrice;
        product.StockQuantity = dto.StockQuantity;
        product.MinStockLevel = dto.MinStockLevel;
        product.Unit = dto.Unit;
        product.CategoryId = dto.CategoryId;
        product.SupplierId = dto.SupplierId;
        product.Status = dto.Status ?? "Active";
        product.IsFeatured = dto.IsFeatured;
        product.IsTaxable = dto.IsTaxable;
        product.ImageUrl = dto.ImageUrl;
        product.Tags = dto.Tags;
        product.ExpiryDate = dto.ExpiryDate;
        product.ManufacturingDate = dto.ManufacturingDate;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Product with ID {Id} updated successfully.", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    // [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")] // Temporarily disabled for testing
    public async Task<IActionResult> Delete(int id)
    {
        // Use a direct SQL update for soft-delete to avoid potential EF tracking/locking issues
        try
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlInterpolatedAsync($"UPDATE \"Products\" SET \"IsDeleted\" = TRUE, \"UpdatedAt\" = {now} WHERE \"Id\" = {id}");
            if (rows == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            try
            {
                var debugPath = Path.Combine(Directory.GetCurrentDirectory(), "last_delete_error.txt");
                var msg = $"Delete failed for Product.Id={id}: {ex.Message} -- {ex.InnerException?.Message}";
                System.IO.File.WriteAllText(debugPath, msg);
            }
            catch { }
            return StatusCode(500, new { message = "Error deleting product", error = ex.Message });
        }
    }
}
