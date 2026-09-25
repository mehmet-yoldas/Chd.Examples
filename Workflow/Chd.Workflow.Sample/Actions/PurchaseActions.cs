using Chd.Workflow.Attributes;
using Chd.Workflow.Interfaces;
using Chd.Workflow.Models;
using Chd.Workflow.Sample.Actions.Outputs;

namespace Chd.Workflow.Sample.Actions;

[WorkflowGuard(
    "satin-alma-tutar-kontrolu",
    DisplayName = "Purchase amount check",
    Category = "Purchase",
    Description = "Routes requests of 10,000 or more to finance.",
    OutputType = typeof(PurchaseAmountGuardOutput))]
public sealed class PurchaseAmountGuard : IWorkflowGuard
{
    private const decimal FinanceThreshold = 10000m;

    public Task<WorkflowGuardResult> EvaluateAsync(WorkflowGuardContext ctx)
    {
        var amount = ReadDecimal(ctx.AllData, "tutar");
        var output = new PurchaseAmountGuardOutput
        {
            Amount = amount,
            Threshold = FinanceThreshold,
            FinanceApprovalRequired = amount >= FinanceThreshold,
        };

        if (amount <= 0)
            return Task.FromResult(WorkflowGuardResult.Deny("Amount must be greater than 0.", output));
        if (output.FinanceApprovalRequired)
            return Task.FromResult(WorkflowGuardResult.Route("finans-onayi", output));
        return Task.FromResult(WorkflowGuardResult.Allowed(output));
    }

    private static decimal ReadDecimal(IDictionary<string, object?> data, string key)
    {
        if (!data.TryGetValue(key, out var raw) || raw is null) return 0;
        if (raw is System.Text.Json.JsonElement json)
        {
            return json.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Number => json.GetDecimal(),
                System.Text.Json.JsonValueKind.String => decimal.TryParse(json.GetString(), out var parsed) ? parsed : 0,
                _ => 0,
            };
        }
        return Convert.ToDecimal(raw);
    }
}

[WorkflowAction(
    "satin-alma-talep-olustur",
    DisplayName = "Create purchase request",
    Category = "Purchase",
    OutputType = typeof(PurchaseCreatedOutput))]
public sealed class CreatePurchaseAction : IWorkflowAction
{
    private static int _nextId = 2000;

    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var id = Interlocked.Increment(ref _nextId);
        var output = new PurchaseCreatedOutput
        {
            TalepNumarasi = $"SA-{DateTime.Now:yyyyMMdd}-{id}",
            Durum = "Beklemede",
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}

[WorkflowAction(
    "approve-purchase",
    DisplayName = "Approve purchase",
    Category = "Purchase",
    OutputType = typeof(PurchaseApprovedOutput))]
public sealed class ApprovePurchaseAction : IWorkflowAction
{
    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var output = new PurchaseApprovedOutput
        {
            ApprovedAt = DateTime.UtcNow,
            ApprovedBy = context.UserId,
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}

[WorkflowAction(
    "reject-purchase",
    DisplayName = "Reject purchase",
    Category = "Purchase",
    OutputType = typeof(PurchaseRejectedOutput))]
public sealed class RejectPurchaseAction : IWorkflowAction
{
    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var output = new PurchaseRejectedOutput
        {
            RejectedAt = DateTime.UtcNow,
            RejectedBy = context.UserId,
            RejectComment = context.Comment,
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}
