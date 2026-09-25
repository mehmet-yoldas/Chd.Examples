using Chd.Workflow.Enums;
using Chd.Workflow.Models;

namespace Chd.Workflow.Sample.Workflows;

public static class PurchaseRequestDefinition
{
    public const string Id = "satin-alma-talebi";

    public static WorkflowDefinition Create()
    {
        var approved = new Node
        {
            Id = "onaylandi",
            Name = "onaylandi",
            Title = "Approved",
            Type = NodeType.End,
            FormType = FormType.None,
            Order = 3,
            PositionX = 80,
            PositionY = 520,
        };
        var rejected = new Node
        {
            Id = "reddedildi",
            Name = "reddedildi",
            Title = "Rejected",
            Type = NodeType.End,
            FormType = FormType.None,
            Order = 4,
            PositionX = 520,
            PositionY = 520,
        };

        var amirOnayi = new Node
        {
            Id = "amir-onayi",
            Name = "amir-onayi",
            Title = "Manager review",
            Description = "Requests under 10,000 go here.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Amir", "Admin"],
            Order = 2,
            PositionX = 80,
            PositionY = 360,
            Fields =
            [
                new Field { Name = "urun", Label = "Item", Type = FieldType.Text, ReadOnly = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "tutar", Label = "Amount", Type = FieldType.Currency, ReadOnly = true, Order = 2, LayoutSpan = 6 },
                new Field { Name = "amirNotu", Label = "Manager note", Type = FieldType.TextArea, Order = 3, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition { Id = "amir-ok", Action = "approve", Label = "Approve", TargetNodeId = "onaylandi", ActionHandler = "approve-purchase", ButtonStyle = "success", RequiresConfirmation = true, Order = 1 },
                new Transition { Id = "amir-no", Action = "reject", Label = "Reject", TargetNodeId = "reddedildi", ActionHandler = "reject-purchase", ButtonStyle = "danger", RequiresComment = true, Order = 2 },
            ],
        };

        var finansOnayi = new Node
        {
            Id = "finans-onayi",
            Name = "finans-onayi",
            Title = "Finance review",
            Description = "Requests of 10,000 or more go here.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Finans", "Admin"],
            Order = 2,
            PositionX = 520,
            PositionY = 360,
            Fields =
            [
                new Field { Name = "urun", Label = "Item", Type = FieldType.Text, ReadOnly = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "tutar", Label = "Amount", Type = FieldType.Currency, ReadOnly = true, Order = 2, LayoutSpan = 6 },
                new Field { Name = "finansNotu", Label = "Finance note", Type = FieldType.TextArea, Order = 3, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition { Id = "fin-ok", Action = "approve", Label = "Approve", TargetNodeId = "onaylandi", ActionHandler = "approve-purchase", ButtonStyle = "success", RequiresConfirmation = true, Order = 1 },
                new Transition { Id = "fin-no", Action = "reject", Label = "Reject", TargetNodeId = "reddedildi", ActionHandler = "reject-purchase", ButtonStyle = "danger", RequiresComment = true, Order = 2 },
            ],
        };

        var form = new Node
        {
            Id = "talep-formu",
            Name = "talep-formu",
            Title = "Purchase form",
            Description = "Enter the item, amount, and reason.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Employee", "Admin"],
            Order = 1,
            PositionX = 300,
            PositionY = 210,
            Fields =
            [
                new Field { Name = "urun", Label = "Item / service", Type = FieldType.Text, Required = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "tutar", Label = "Amount", Type = FieldType.Currency, Required = true, Min = 1, Order = 2, LayoutSpan = 6 },
                new Field
                {
                    Name = "kategori",
                    Label = "Category",
                    Type = FieldType.Dropdown,
                    Required = true,
                    Order = 3,
                    LayoutSpan = 6,
                    Options =
                    [
                        new FieldOption { Label = "Hardware", Value = "hardware" },
                        new FieldOption { Label = "Software", Value = "software" },
                        new FieldOption { Label = "Service", Value = "service" },
                    ],
                },
                new Field { Name = "gerekce", Label = "Reason", Type = FieldType.TextArea, Required = true, Order = 4, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition
                {
                    Id = "submit-purchase",
                    Action = "submit",
                    Label = "Submit",
                    TargetNodeId = "amir-onayi",
                    ActionHandler = "satin-alma-talep-olustur",
                    GuardHandler = "satin-alma-tutar-kontrolu",
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
                new Transition { Id = "begin", Action = "start", Label = "Start", TargetNodeId = "talep-formu", ButtonStyle = "primary", Order = 1 },
            ],
        };

        start.Children = [form];
        form.ParentId = start.Id;
        form.Children = [amirOnayi, finansOnayi];
        amirOnayi.ParentId = form.Id;
        finansOnayi.ParentId = form.Id;
        amirOnayi.Children = [approved];
        finansOnayi.Children = [rejected];
        approved.ParentId = amirOnayi.Id;
        rejected.ParentId = finansOnayi.Id;

        return new WorkflowDefinition
        {
            Id = Id,
            Name = "Purchase request",
            Description = "Employee submits a form. Under 10,000 goes to manager, 10,000+ to finance.",
            Version = 1,
            IsActive = true,
            RootNode = start,
            CreatedBy = "sample",
        };
    }
}
