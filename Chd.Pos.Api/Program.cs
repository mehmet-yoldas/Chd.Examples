using Microsoft.EntityFrameworkCore;
using Chd.Pos.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// SPA Proxy disabled - npm install required
// if (app.Environment.IsDevelopment())
// {
//     app.UseSpa(spa =>
//     {
//         spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
//     });
// }

app.Run();
