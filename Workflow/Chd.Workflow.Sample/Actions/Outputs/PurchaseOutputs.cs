namespace Chd.Workflow.Sample.Actions.Outputs;

public sealed class PurchaseAmountGuardOutput
{
    public decimal Amount { get; set; }
    public decimal Threshold { get; set; }
    public bool FinanceApprovalRequired { get; set; }
}

public sealed class PurchaseCreatedOutput
{
    public string TalepNumarasi { get; set; } = "";
    public string Durum { get; set; } = "";
}

public sealed class PurchaseApprovedOutput
{
    public DateTime ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
}

public sealed class PurchaseRejectedOutput
{
    public DateTime RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectComment { get; set; }
}
