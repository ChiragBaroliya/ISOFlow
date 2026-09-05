namespace ISOFlow.Api.Auditing;

/// <summary>
/// Central place where every auditable entity registers "how do I fetch my current state by id".
/// This is the one bit of per-entity wiring the automatic audit pipeline needs — a single line per
/// entity type in <see cref="AuditSnapshotRegistrations"/>, pointing at that entity's own repository
/// GetById method. Controllers themselves stay completely free of audit code.
/// </summary>
public interface IAuditSnapshotRegistry
{
    void Register(string entityName, Func<IServiceProvider, string, int?, Task<object?>> resolver);
    bool IsRegistered(string entityName);
    Task<object?> ResolveAsync(IServiceProvider serviceProvider, string entityName, string entityId, int? organizationId);
}

public class AuditSnapshotRegistry : IAuditSnapshotRegistry
{
    private readonly Dictionary<string, Func<IServiceProvider, string, int?, Task<object?>>> _resolvers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string entityName, Func<IServiceProvider, string, int?, Task<object?>> resolver) =>
        _resolvers[entityName] = resolver;

    public bool IsRegistered(string entityName) => _resolvers.ContainsKey(entityName);

    public Task<object?> ResolveAsync(IServiceProvider serviceProvider, string entityName, string entityId, int? organizationId)
    {
        return _resolvers.TryGetValue(entityName, out var resolver)
            ? resolver(serviceProvider, entityId, organizationId)
            : Task.FromResult<object?>(null);
    }
}
