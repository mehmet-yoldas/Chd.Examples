using Chd.AutoUI.Services;
using Chd.StockTracking.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Chd.StockTracking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly MetadataGenerator _metadataGenerator;

    public MetadataController()
    {
        _metadataGenerator = new MetadataGenerator();
    }

    [HttpGet]
    public IActionResult GetAllEntities()
    {
        var assembly = typeof(ProductDto).Assembly;
        var entities = _metadataGenerator.ScanAssemblyForEntities(assembly);
        return Ok(entities);
    }

    [HttpGet("{entityName}")]
    public IActionResult GetEntityMetadata(string entityName)
    {
        var assembly = typeof(ProductDto).Assembly;
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

        if (type == null)
            return NotFound($"Entity '{entityName}' not found");

        var metadata = _metadataGenerator.GenerateMetadata(type);
        return Ok(metadata);
    }
}
