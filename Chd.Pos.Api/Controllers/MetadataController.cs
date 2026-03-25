using Microsoft.AspNetCore.Mvc;
using Chd.AutoUI.Services;
using Chd.Pos.Core.DTOs;
using System.Reflection;

namespace Chd.Pos.Api.Controllers;

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
    public IActionResult GetAllMetadata()
    {
        var assembly = typeof(ProductDto).Assembly;
        var metadata = _metadataGenerator.ScanAssemblyForEntities(assembly);
        return Ok(metadata);
    }

    [HttpGet("{entityName}")]
    public IActionResult GetMetadata(string entityName)
    {
        var assembly = typeof(ProductDto).Assembly;
        var type = assembly.GetType($"Chd.Pos.Core.DTOs.{entityName}");
        
        if (type == null)
            return NotFound($"Entity '{entityName}' not found");

        var metadata = _metadataGenerator.GenerateMetadata(type);
        return Ok(metadata);
    }
}
