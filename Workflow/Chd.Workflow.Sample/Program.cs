using Chd.Workflow.Extensions;
using Chd.Workflow.Interfaces;
using Chd.Workflow.Sample.Infrastructure;
using Chd.Workflow.Sample.Participants;
using Chd.Workflow.Sample.Workflows;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException("ConnectionStrings:PostgreSQL is missing.");

await SampleDatabaseBootstrap.EnsureAsync(connectionString, builder.Environment.ContentRootPath);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Chd.Workflow.Sample",
        Version = "v1"
    });
});

builder.AddWorkflow<SampleParticipantDirectory>(o =>
{
    o.DatabaseProvider = "PostgreSQL";
    o.ConnectionString = connectionString;
    o.AutoMigrate = true;
    o.WorkflowDatabaseSchema = "Workflow";
    o.RoutePrefix = "api/workflow";
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
            throw;

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Chd.Workflow.Sample v1");
    options.RoutePrefix = "swagger";
});

await app.UseChdWorkflowAsync();
await SampleDatabaseBootstrap.EnsureCurrentColumnsAsync(connectionString);

using (var scope = app.Services.CreateScope())
{
    var repository = scope.ServiceProvider.GetRequiredService<IWorkflowRepository>();
    foreach (var definition in new[]
    {
        LeaveRequestDefinition.Create(),
        PurchaseRequestDefinition.Create(),
        SupportTicketDefinition.Create(),
    })
    {
        await repository.SaveDefinitionAsync(definition);
    }
}

app.MapGet("/swagger-ui", () => Results.Redirect("/swagger/index.html"))
    .ExcludeFromDescription();

app.Run();
