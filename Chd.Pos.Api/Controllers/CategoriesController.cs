using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Chd.Pos.Api.Data;
using Chd.Pos.Core.Entities;
using Chd.Pos.Core.DTOs;

namespace Chd.Pos.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly PosDbContext _context;

    public CategoriesController(PosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _context.Categories
            .Include(c => c.ParentCategory)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryDto dto)
    {
        try
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ParentCategoryId = dto.ParentCategoryId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            dto.Id = category.Id;
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, dto);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Kategori oluşturulamadı", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Returns hierarchical tree of categories
    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<TreeNode>>> GetTree()
    {
        // Load raw items and normalize parent ids to avoid self-references or invalid parents
        var raw = await _context.Categories
            .Where(c => !c.IsDeleted)
            .Select(c => new { c.Id, c.Name, ParentId = c.ParentCategoryId })
            .ToListAsync();

        var items = raw
            .Select(i => new { i.Id, i.Name, ParentId = (i.ParentId.HasValue && i.ParentId.Value != i.Id) ? i.ParentId : (int?)null })
            .ToList();

        var lookup = items.ToDictionary(i => i.Id, i => new TreeNode { Id = i.Id, Label = i.Name });

        foreach (var it in items)
        {
            if (it.ParentId.HasValue && lookup.ContainsKey(it.ParentId.Value))
            {
                // avoid adding a node as a child of itself
                if (it.ParentId.Value != it.Id)
                {
                    lookup[it.ParentId.Value].Children.Add(lookup[it.Id]);
                }
            }
        }

        var roots = lookup.Values.Where(n => !items.Any(i => i.Id == n.Id && i.ParentId.HasValue)).ToList();
        return Ok(roots);
    }

    public class TreeNode
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public List<TreeNode> Children { get; set; } = new();
    }
}
