using Chd.Workflow.Attributes;
using Chd.Workflow.Interfaces;
using Chd.Workflow.Sample.Actions.Outputs;

namespace Chd.Workflow.Sample.Actions;

[WorkflowAction(
    "approve-leave",
    DisplayName = "Approve leave",
    Category = "HR",
    OutputType = typeof(LeaveApprovedOutput))]
public sealed class ApproveLeaveAction : IWorkflowAction
{
    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var output = new LeaveApprovedOutput
        {
            LeaveApprovedAt = DateTime.UtcNow,
            LeaveApprovedBy = context.UserId,
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}

[WorkflowAction(
    "reject-leave",
    DisplayName = "Reject leave",
    Category = "HR",
    OutputType = typeof(LeaveRejectedOutput))]
public sealed class RejectLeaveAction : IWorkflowAction
{
    public Task<WorkflowActionResult> ExecuteAsync(WorkflowActionContext context)
    {
        var output = new LeaveRejectedOutput
        {
            LeaveRejectedAt = DateTime.UtcNow,
            LeaveRejectedBy = context.UserId,
            RejectComment = context.Comment,
        };
        return Task.FromResult(WorkflowActionResult.Ok(output));
    }
}
