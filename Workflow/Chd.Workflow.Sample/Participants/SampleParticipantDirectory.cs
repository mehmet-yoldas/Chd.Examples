using Chd.Workflow.Models;

namespace Chd.Workflow.Sample.Participants;

/// <summary>
/// Demo identity store. Replace with AD / ASP.NET Identity in a real host.
/// </summary>
public sealed class SampleParticipantDirectory : WorkflowParticipantDirectoryBase
{
    public override Task<IReadOnlyList<WorkflowGroup>> ListGroupsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WorkflowGroup> groups =
        [
            new() { Id = "Employee", Name = "Employee" },
            new() { Id = "Manager", Name = "Manager" },
            new() { Id = "Amir", Name = "Amir" },
            new() { Id = "Finans", Name = "Finans" },
            new() { Id = "Operasyon", Name = "Operasyon" },
            new() { Id = "Yonetici", Name = "Supervisor" },
            new() { Id = "Admin", Name = "Admin" },
        ];
        return Task.FromResult(groups);
    }

    public override Task<IReadOnlyList<string>> GetRoleNamesForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<string> roles = userId.Trim().ToLowerInvariant() switch
        {
            "employee" => ["Employee"],
            "manager" => ["Manager", "Amir", "Operasyon"],
            "admin" => ["Admin", "Finans", "Yonetici"],
            _ => [],
        };
        return Task.FromResult(roles);
    }

    public override Task<WorkflowUser?> GetUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        WorkflowUser? user = userId.Trim().ToLowerInvariant() switch
        {
            "employee" => new() { Id = "employee", DisplayName = "Employee", Email = "employee@example.com" },
            "manager" => new() { Id = "manager", DisplayName = "Manager", Email = "manager@example.com" },
            "admin" => new() { Id = "admin", DisplayName = "Admin", Email = "admin@example.com" },
            _ => null,
        };
        return Task.FromResult(user);
    }

    public override Task<IReadOnlyList<WorkflowUser>> ResolveUsersInGroupsAsync(
        IReadOnlyList<string> groupOrRoleNames,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WorkflowUser> users =
        [
            new() { Id = "manager", DisplayName = "Manager", Email = "manager@example.com" },
        ];
        return Task.FromResult(users);
    }
}
