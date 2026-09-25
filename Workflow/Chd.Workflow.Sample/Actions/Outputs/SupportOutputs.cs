namespace Chd.Workflow.Sample.Actions.Outputs;

public sealed class SupportPriorityGuardOutput
{
    public string Priority { get; set; } = "";
    public bool SupervisorRequired { get; set; }
}

public sealed class SupportTicketCreatedOutput
{
    public string TicketNo { get; set; } = "";
    public string Konu { get; set; } = "";
}

public sealed class SupportTicketClosedOutput
{
    public DateTime KapanisTarihi { get; set; }
    public string? Kapatan { get; set; }
}
