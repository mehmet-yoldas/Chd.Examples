using Chd.Workflow.Attributes;
using Chd.Workflow.Interfaces;
using Chd.Workflow.Models;
using Chd.Workflow.Sample.Actions.Outputs;

namespace Chd.Workflow.Sample.Actions;

[WorkflowGuard(
    "destek-oncelik-kontrolu",
    DisplayName = "Support priority check",
    Category = "Support",
    Description = "Routes high-priority tickets to a supervisor.",
    OutputType = typeof(SupportPriorityGuardOutput))]
public sealed class SupportPriorityGuard : IWorkflowGuard
{
    public Task<WorkflowGuardResult> EvaluateAsync(WorkflowGuardContext ctx)
    {
        var priority = ReadString(ctx.AllData, "oncelik").ToLowerInvariant();
        var output = new SupportPriorityGuardOutput
        {
            Priority = priority,
            SupervisorRequired = priority is "high" or "yuksek" or "yüksek",
        };

        if (string.IsNullOrWhiteSpace(priority))
            return Task.FromResult(WorkflowGuardResult.Deny("Priority is required.", output));
        if (output.SupervisorRequired)
            return Task.FromResult(WorkflowGuardResult.Route("yonetici-inceleme", output));
        return Task.FromResult(WorkflowGuardResult.Allowed(output));
    }

    private static string ReadString(IDictionary<string, object?> data, string key)
    {
        if (!data.TryGetValue(key, out var raw) || raw is null) return "";
        if (raw is System.Text.Json.JsonElement json)
            return json.ValueKind == System.Text.Json.JsonValueKind.String ? json.GetString() ?? "" : json.ToString();
        return raw.ToString() ?? "";
    }
}

[WorkflowAction(
    "destek-kayit-olustur",
    DisplayName = "Create support ticket",
    Category = "Support",
    OutputType = typeof(SupportTicketCreatedOutput))]
public sealed class CreateSupportTicketAction : IWorkflowAction
{
    private static int _nextId = 5000;

    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var subject = context.FormData.TryGetValue("konu", out var value) ? value?.ToString() ?? "" : "";
        var id = Interlocked.Increment(ref _nextId);
        var output = new SupportTicketCreatedOutput
        {
            TicketNo = $"DST-{DateTime.Now:yyyyMMdd}-{id}",
            Konu = subject,
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}

[WorkflowAction(
    "destek-kapat",
    DisplayName = "Close support ticket",
    Category = "Support",
    OutputType = typeof(SupportTicketClosedOutput))]
public sealed class CloseSupportTicketAction : IWorkflowAction
{
    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var output = new SupportTicketClosedOutput
        {
            KapanisTarihi = DateTime.UtcNow,
            Kapatan = context.UserId,
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}
