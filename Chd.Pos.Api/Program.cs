using Chd.AutoUI.Extensions;
using Chd.Common.Entities;
using Chd.Pos.Api.Data;
using Chd.Pos.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("C:\\Temp\\logs\\application-log.txt", rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5218", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Add AutoUI services and configure it to scan the assembly containing ProductDto for UI generation and
// use UserRepoesitory.GetUserTokenInfo for user token information.
var app = builder.UseAutoUI<UserRepoesitory>(typeof(ProductDto).Assembly);
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();

logger.Information("Application started and logging is configured.");

app.Run();
