using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chd.Pos.Api.Data;
using Chd.Pos.Core.DTOs;
using Chd.Pos.Core.Entities;

namespace Chd.Pos.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly PosDbContext _context;

    public SuppliersController(PosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetAll()
    {
        var suppliers = await _context.Set<Supplier>()
            .Where(s => !s.IsDeleted)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                TaxNumber = s.TaxNumber
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetById(int id)
    {
        var supplier = await _context.Set<Supplier>()
            .Where(s => s.Id == id && !s.IsDeleted)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                TaxNumber = s.TaxNumber
            })
            .FirstOrDefaultAsync();

        if (supplier == null)
            return NotFound();

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create([FromBody] SupplierDto dto)
    {
        try
        {
            var supplier = new Supplier
            {
                Name = dto.Name,
                ContactPerson = dto.ContactPerson,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                TaxNumber = dto.TaxNumber,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Set<Supplier>().Add(supplier);
            await _context.SaveChangesAsync();

            dto.Id = supplier.Id;
            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Tedarikçi oluşturulamadı", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SupplierDto dto)
    {
        try
        {
            var supplier = await _context.Set<Supplier>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (supplier == null)
                return NotFound();

            supplier.Name = dto.Name;
            supplier.ContactPerson = dto.ContactPerson;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;
            supplier.TaxNumber = dto.TaxNumber;
            supplier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Tedarikçi güncellenemedi", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var supplier = await _context.Set<Supplier>()
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (supplier == null)
                return NotFound();

            supplier.IsDeleted = true;
            supplier.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Tedarikçi silinemedi", error = ex.Message });
        }
    }
}
