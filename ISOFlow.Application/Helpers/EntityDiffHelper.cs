using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ISOFlow.Application.Helpers;

/// <summary>
/// Reflection-based snapshot serialization and before/after diffing shared by the audit pipeline.
/// Works against any plain entity POCO, so no per-entity diff/mapping code is needed to audit it.
/// Any property whose name contains a sensitive token (password, secret, token, hash, apikey,
/// credential) is redacted wherever it appears — full snapshots and changed-field values alike —
/// so audit rows can never leak credentials even if a caller passes the whole entity in.
/// </summary>
public static class EntityDiffHelper
{
    private const string Redacted = "***REDACTED***";

    private static readonly string[] SensitiveTokens =
        { "password", "secret", "token", "hash", "apikey", "credential" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static bool IsSensitiveProperty(string propertyName) =>
        SensitiveTokens.Any(t => propertyName.Contains(t, StringComparison.OrdinalIgnoreCase));

    /// <summary>Serializes a full entity snapshot to JSON, with sensitive properties redacted.</summary>
    public static string? ToRedactedJson(object? entity)
    {
        if (entity == null) return null;

        var redacted = new Dictionary<string, object?>();
        foreach (var prop in ReadableProperties(entity.GetType()))
        {
            redacted[prop.Name] = IsSensitiveProperty(prop.Name) ? Redacted : prop.GetValue(entity);
        }

        return JsonSerializer.Serialize(redacted, JsonOptions);
    }

    /// <summary>
    /// Compares two same-shaped snapshots property-by-property and returns only the fields whose
    /// value actually changed. Properties present on only one side are ignored (nothing to diff).
    /// </summary>
    public static Dictionary<string, (object? Old, object? New)> Diff(object? oldEntity, object? newEntity)
    {
        var changes = new Dictionary<string, (object? Old, object? New)>();
        if (oldEntity == null || newEntity == null) return changes;

        var oldType = oldEntity.GetType();

        foreach (var prop in ReadableProperties(newEntity.GetType()))
        {
            var oldProp = oldType.GetProperty(prop.Name);
            if (oldProp == null) continue;

            var oldValue = oldProp.GetValue(oldEntity);
            var newValue = prop.GetValue(newEntity);
            if (ValuesEqual(oldValue, newValue)) continue;

            changes[prop.Name] = IsSensitiveProperty(prop.Name) ? (Redacted, Redacted) : (oldValue, newValue);
        }

        return changes;
    }

    /// <summary>
    /// Value equality that treats two structurally-identical collections (e.g. List&lt;string&gt;)
    /// fetched as separate object instances as equal, instead of always "changed" under plain
    /// reference equality — every list-typed entity property would otherwise appear to change on
    /// every single Update, regardless of whether its contents actually differ.
    /// </summary>
    private static bool ValuesEqual(object? oldValue, object? newValue)
    {
        if (Equals(oldValue, newValue)) return true;
        if (oldValue is string || newValue is string) return false;

        if (oldValue is System.Collections.IEnumerable oldSequence && newValue is System.Collections.IEnumerable newSequence)
        {
            return oldSequence.Cast<object?>().SequenceEqual(newSequence.Cast<object?>());
        }

        return false;
    }

    /// <summary>Serializes a diff as {"field": {"old": ..., "new": ...}} for the ChangedFields JSONB column.</summary>
    public static string ToJson(Dictionary<string, (object? Old, object? New)> changes)
    {
        var shaped = changes.ToDictionary(kv => kv.Key, kv => new { old = kv.Value.Old, @new = kv.Value.New });
        return JsonSerializer.Serialize(shaped, JsonOptions);
    }

    private static IEnumerable<PropertyInfo> ReadableProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
}
