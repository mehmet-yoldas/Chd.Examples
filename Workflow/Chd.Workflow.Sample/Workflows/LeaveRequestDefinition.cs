using Chd.Workflow.Enums;
using Chd.Workflow.Models;

namespace Chd.Workflow.Sample.Workflows;

public static class LeaveRequestDefinition
{
    public const string Id = "leave-request";

    public static WorkflowDefinition Create()
    {
        var start = new Node
        {
            Id = "start",
            Name = "start",
            Title = "Start",
            Type = NodeType.Start,
            FormType = FormType.None,
            Order = 0,
            PositionX = 160,
            PositionY = 80,
            Transitions =
            [
                new Transition
                {
                    Id = "start-form",
                    Action = "start",
                    Label = "Start request",
                    TargetNodeId = "request",
                    Order = 1,
                    ButtonStyle = "primary",
                },
            ],
        };

        var request = new Node
        {
            Id = "request",
            Name = "request",
            Title = "Leave request",
            Description = "Employee fills days and reason.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Employee", "Admin"],
            Order = 1,
            PositionX = 160,
            PositionY = 220,
            Fields =
            [
                new Field { Name = "employeeName", Label = "Employee", Type = FieldType.Text, Required = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "days", Label = "Days", Type = FieldType.Number, Required = true, Min = 1, Max = 30, Order = 2, LayoutSpan = 6 },
                new Field { Name = "reason", Label = "Reason", Type = FieldType.TextArea, Required = true, Order = 3, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition
                {
                    Id = "submit",
                    Action = "submit",
                    Label = "Submit",
                    TargetNodeId = "review",
                    Guard = "days > 0",
                    GuardFailureMessage = "Days must be greater than 0.",
                    Order = 1,
                    ButtonStyle = "primary",
                    RequiresConfirmation = true,
                },
            ],
        };

        var review = new Node
        {
            Id = "review",
            Name = "review",
            Title = "Manager review",
            Description = "Only Manager or Admin can decide.",
            Type = NodeType.Task,
            FormType = FormType.Dynamic,
            AllowedRoles = ["Manager", "Admin"],
            Order = 2,
            PositionX = 160,
            PositionY = 380,
            Fields =
            [
                new Field { Name = "employeeName", Label = "Employee", Type = FieldType.Text, ReadOnly = true, Order = 1, LayoutSpan = 12 },
                new Field { Name = "days", Label = "Days", Type = FieldType.Number, ReadOnly = true, Order = 2, LayoutSpan = 6 },
                new Field { Name = "reason", Label = "Reason", Type = FieldType.TextArea, ReadOnly = true, Order = 3, LayoutSpan = 12 },
            ],
            Transitions =
            [
                new Transition
                {
                    Id = "approve",
                    Action = "approve",
                    Label = "Approve",
                    TargetNodeId = "approved",
                    ActionHandler = "approve-leave",
                    Order = 1,
                    ButtonStyle = "success",
                    RequiresConfirmation = true,
                },
                new Transition
                {
                    Id = "reject",
                    Action = "reject",
                    Label = "Reject",
                    TargetNodeId = "rejected",
                    ActionHandler = "reject-leave",
                    Order = 2,
                    ButtonStyle = "danger",
                    RequiresComment = true,
                },
            ],
        };

        var approved = new Node
        {
            Id = "approved",
            Name = "approved",
            Title = "Approved",
            Type = NodeType.End,
            FormType = FormType.None,
            Order = 3,
            PositionX = 40,
            PositionY = 540,
        };

        var rejected = new Node
        {
            Id = "rejected",
            Name = "rejected",
            Title = "Rejected",
            Type = NodeType.End,
            FormType = FormType.None,
            Order = 4,
            PositionX = 280,
            PositionY = 540,
        };

        start.Children = [request];
        request.ParentId = start.Id;
        request.Children = [review];
        review.ParentId = request.Id;
        review.Children = [approved, rejected];
        approved.ParentId = review.Id;
        rejected.ParentId = review.Id;

        return new WorkflowDefinition
        {
            Id = Id,
            Name = "Leave request",
            Description = "Employee submits days and reason. Manager approves or rejects.",
            Version = 1,
            IsActive = true,
            RootNode = start,
            CreatedBy = "sample",
        };
    }
}
