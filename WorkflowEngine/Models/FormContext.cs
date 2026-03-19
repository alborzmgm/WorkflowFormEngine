namespace WorkflowFormEngine.WorkflowEngine.Models;

/// <summary>
/// Runtime value bag for all field values in the current workflow session.
/// Passed to IDataSourceProvider so dependent providers can filter by parent values.
/// </summary>
public sealed class FormContext
{
    private readonly Dictionary<string, object?> _values =
        new(StringComparer.OrdinalIgnoreCase);

    public object? GetValue(string key) =>
        _values.TryGetValue(key, out var val) ? val : null;

    public void SetValue(string key, object? value) =>
        _values[key] = value;

    public void Reset(string key) =>
        _values.Remove(key);

    /// <summary>
    /// Returns true when the key exists and has a meaningful value.
    /// Handles scalar values, List&lt;string&gt; (CheckboxListField), and
    /// List&lt;Dictionary&lt;string, string&gt;&gt; (RepeatableListField).
    /// </summary>
    public bool HasValue(string key)
    {
        if (!_values.TryGetValue(key, out var val) || val is null)
            return false;

        // Checkbox list stores its selections as List<string>
        if (val is List<string> list)
            return list.Count > 0;

        // Repeatable list stores rows as List<Dictionary<string, string>>
        if (val is List<Dictionary<string, string>> rows)
            return rows.Any(r => IsRepeatableListRowNonEmpty(r));

        return val.ToString() is { Length: > 0 };
    }

    public IReadOnlyDictionary<string, object?> AllValues => _values;

    /// <summary>
    /// Returns true when a repeatable-list row contains at least one non-empty cell.
    /// Used by <see cref="HasValue"/>, <see cref="WorkflowEngine.ConditionEvaluator"/>,
    /// <see cref="Services.ValidationService"/>, and the RepeatableListField component
    /// so that the definition of "non-empty row" is identical everywhere.
    /// </summary>
    public static bool IsRepeatableListRowNonEmpty(IReadOnlyDictionary<string, string> row) =>
        row.Values.Any(v => !string.IsNullOrWhiteSpace(v));
}
