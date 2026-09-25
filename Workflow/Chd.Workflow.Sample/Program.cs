using Chd.Workflow.Extensions;
using Chd.Workflow.Interfaces;
using Chd.Workflow.Sample.Participants;
using Chd.Workflow.Sample.Workflows;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddWorkflow<SampleParticipantDirectory>(o =>
{
    o.DatabaseProvider = "InMemory";
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
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

await app.UseChdWorkflowAsync();

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

app.MapGet("/swagger-ui", () => Results.Redirect("/swagger"));

app.Run();
