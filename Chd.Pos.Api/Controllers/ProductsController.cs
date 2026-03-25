using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chd.Pos.Api.Data;
using Chd.Pos.Core.Entities;
using Chd.Pos.Core.DTOs;

namespace Chd.Pos.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly PosDbContext _context;

    public ProductsController(PosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll()
    {
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

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
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
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
    {
        try
        {
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

            dto.Id = product.Id;
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Error creating product", 
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

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
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
