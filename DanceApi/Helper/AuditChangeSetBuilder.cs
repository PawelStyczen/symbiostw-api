using DanceApi.Model;

namespace DanceApi.Helper;

public class AuditChangeSetBuilder
{
    private readonly Dictionary<string, AuditFieldChange> _changes = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _changes.Count;

    public AuditChangeSetBuilder Add(string fieldName, object? oldValue, object? newValue)
    {
        var normalizedOldValue = NormalizeValue(oldValue);
        var normalizedNewValue = NormalizeValue(newValue);

        if (string.Equals(normalizedOldValue, normalizedNewValue, StringComparison.Ordinal))
            return this;

        _changes[fieldName] = new AuditFieldChange
        {
            OldValue = normalizedOldValue,
            NewValue = normalizedNewValue
        };

        return this;
    }

    public AuditChangeSetBuilder AddCreated(string fieldName, object? newValue)
    {
        return Add(fieldName, null, newValue);
    }

    public IReadOnlyDictionary<string, AuditFieldChange> Build()
    {
        return _changes;
    }

    private static string? NormalizeValue(object? value)
    {
        if (value == null)
            return null;

        return value switch
        {
            bool boolValue => boolValue ? "true" : "false",
            DateTime dateTimeValue => dateTimeValue.ToUniversalTime().ToString("O"),
            DateTimeOffset dateTimeOffsetValue => dateTimeOffsetValue.ToUniversalTime().ToString("O"),
            Enum enumValue => enumValue.ToString(),
            _ => value.ToString()
        };
    }
}
