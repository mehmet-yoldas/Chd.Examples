using Chd.AutoUI.EF.Services;
using Chd.AutoUI.Services;
using Chd.StockTracking.Api.Data;
using Chd.StockTracking.Core.DTOs;
using Chd.StockTracking.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Chd.StockTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IGenericRepository<Product> _repository;
    private readonly StockTrackingDbContext _context;

    public ProductsController(IGenericRepository<Product> repository, StockTrackingDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _repository.GetAllAsync();
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            BarcodeNumber = p.BarcodeNumber,
            PurchasePrice = p.PurchasePrice,
            Price = p.Price,
            ProductCount = p.ProductCount,
            MinCount = p.MinCount,
            SelledCount = p.SelledCount,
            Discount = p.Discount,
            Tax = p.Tax,
            ProductCategoryId = p.ProductCategoryId,
            CategoryName = p.ProductCategory?.Name,
            ExpirationDate = p.ExpirationDate
        });
        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            BarcodeNumber = product.BarcodeNumber,
            PurchasePrice = product.PurchasePrice,
            Price = product.Price,
            ProductCount = product.ProductCount,
            MinCount = product.MinCount,
            SelledCount = product.SelledCount,
            Discount = product.Discount,
            Tax = product.Tax,
            ProductCategoryId = product.ProductCategoryId,
            CategoryName = product.ProductCategory?.Name,
            ExpirationDate = product.ExpirationDate
        };
        return Ok(productDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductDto dto)
    {
        try
        {
            var product = new Product
            {
                Name = dto.Name,
                BarcodeNumber = dto.BarcodeNumber,
                PurchasePrice = dto.PurchasePrice,
                Price = dto.Price,
                ProductCount = dto.ProductCount,
                MinCount = dto.MinCount,
                SelledCount = dto.SelledCount,
                Discount = dto.Discount,
                Tax = dto.Tax,
                ProductCategoryId = dto.ProductCategoryId,
                ExpirationDate = dto.ExpirationDate,
                BuilderUserId = 1,
                CreationDate = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(product);
            dto.Id = created.Id;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Ürün oluşturma hatası", 
                error = ex.Message,
                innerError = ex.InnerException?.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, ProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return NotFound();

        product.Name = dto.Name;
        product.BarcodeNumber = dto.BarcodeNumber;
        product.PurchasePrice = dto.PurchasePrice;
        product.Price = dto.Price;
        product.ProductCount = dto.ProductCount;
        product.MinCount = dto.MinCount;
        product.SelledCount = dto.SelledCount;
        product.Discount = dto.Discount;
        product.Tax = dto.Tax;
        product.ProductCategoryId = dto.ProductCategoryId;
        product.ExpirationDate = dto.ExpirationDate;

        await _repository.UpdateAsync(product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }
}
