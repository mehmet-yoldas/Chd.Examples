using Chd.Workflow.Enums;
using Chd.Workflow.Models;

namespace Chd.Workflow.Sample.Workflows;

public static class SupportTicketDefinition
{
    public const string Id = "destek-sikayet";

    public static WorkflowDefinition Create()
    {
        var closed = new Node
        {
            Id = "kapatildi",
            Name = "kapatildi",
            Title = "Closed",
            Type = NodeType.End,
            FormType = FormType.None,
            Order = 3,
            PositionX = 300,
            PositionY = 520,
        };

        var operasyon = new Node
        {
            Id = "operasyon",
            Name = "operasyon",
            Title = "Operations",
            Description = "Normal-priority tickets are resolved here.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Operasyon", "Admin"],
            Order = 2,
            PositionX = 80,
            PositionY = 360,
            Fields =
            [
                new Field { Name = "konu", Label = "Subject", Type = FieldType.Text, ReadOnly = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "cozumNotu", Label = "Resolution note", Type = FieldType.TextArea, Required = true, Order = 2, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition { Id = "ops-close", Action = "complete", Label = "Close", TargetNodeId = "kapatildi", ActionHandler = "destek-kapat", ButtonStyle = "success", Order = 1 },
            ],
        };

        var yonetici = new Node
        {
            Id = "yonetici-inceleme",
            Name = "yonetici-inceleme",
            Title = "Supervisor review",
            Description = "High-priority tickets go here.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Yonetici", "Admin"],
            Order = 2,
            PositionX = 520,
            PositionY = 360,
            Fields =
            [
                new Field { Name = "konu", Label = "Subject", Type = FieldType.Text, ReadOnly = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "yoneticiNotu", Label = "Supervisor note", Type = FieldType.TextArea, Required = true, Order = 2, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition { Id = "mgr-close", Action = "complete", Label = "Close", TargetNodeId = "kapatildi", ActionHandler = "destek-kapat", ButtonStyle = "success", Order = 1 },
                new Transition { Id = "mgr-ops", Action = "assign", Label = "Send to operations", TargetNodeId = "operasyon", ButtonStyle = "primary", Order = 2 },
            ],
        };

        var form = new Node
        {
            Id = "destek-formu",
            Name = "destek-formu",
            Title = "Support form",
            Description = "Enter the subject, priority, and details.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Employee", "Admin"],
            Order = 1,
            PositionX = 300,
            PositionY = 210,
            Fields =
            [
                new Field { Name = "konu", Label = "Subject", Type = FieldType.Text, Required = true, Order = 1, LayoutSpan = 12 },
                new Field
                {
                    Name = "oncelik",
                    Label = "Priority",
                    Type = FieldType.Dropdown,
                    Required = true,
                    Order = 2,
                    LayoutSpan = 6,
                    Options =
                    [
                        new FieldOption { Label = "Low", Value = "low" },
                        new FieldOption { Label = "Normal", Value = "normal" },
                        new FieldOption { Label = "High", Value = "high" },
                    ],
                },
                new Field { Name = "aciklama", Label = "Details", Type = FieldType.TextArea, Required = true, Order = 3, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition
                {
                    Id = "submit-ticket",
                    Action = "submit",
                    Label = "Submit",
                    TargetNodeId = "operasyon",
                    ActionHandler = "destek-kayit-olustur",
                    GuardHandler = "destek-oncelik-kontrolu",
                    ButtonStyle = "primary",
                    Order = 1,
                },
            ],
        };

        var start = new Node
        {
            Id = "baslangic",
            Name = "baslangic",
            Title = "Start",
            Type = NodeType.Start,
            FormType = FormType.None,
            Order = 0,
            PositionX = 300,
            PositionY = 70,
            Transitions =
            [
                new Transition { Id = "begin", Action = "start", Label = "Start", TargetNodeId = "destek-formu", ButtonStyle = "primary", Order = 1 },
            ],
        };

        start.Children = [form];
        form.ParentId = start.Id;
        form.Children = [operasyon, yonetici];
        operasyon.ParentId = form.Id;
        yonetici.ParentId = form.Id;
        operasyon.Children = [closed];
        closed.ParentId = operasyon.Id;

        return new WorkflowDefinition
        {
            Id = Id,
            Name = "Support ticket",
            Description = "High priority goes to a supervisor. Others go to operations.",
            Version = 1,
            IsActive = true,
            RootNode = start,
            CreatedBy = "sample",
        };
    }
}
