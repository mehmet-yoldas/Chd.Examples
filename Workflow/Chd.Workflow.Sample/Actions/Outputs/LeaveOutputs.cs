namespace Chd.Workflow.Sample.Actions.Outputs;

public sealed class LeaveApprovedOutput
{
    public DateTime LeaveApprovedAt { get; set; }
    public string? LeaveApprovedBy { get; set; }
}

public sealed class LeaveRejectedOutput
{
    public DateTime LeaveRejectedAt { get; set; }
    public string? LeaveRejectedBy { get; set; }
    public string? RejectComment { get; set; }
}
