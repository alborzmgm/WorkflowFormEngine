namespace WorkflowFormEngine.WorkflowEngine.Models;

/// <summary>
/// Runtime value bag for all field values in the current workflow session.
/// Passed to IDataSourceProvider so dependent providers can filter by parent values.
///
/// Supported value types stored per field:
///   string                              — text, select, date
///   decimal                             — number
///   List&lt;string&gt;                        — checkboxlist
///   List&lt;Dictionary&lt;string, object?&gt;&gt;   — repeater
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
    /// Returns true when the key has a meaningful value.
    /// Handles scalar strings, List&lt;string&gt; (checkboxlist), and
    /// List&lt;Dictionary&gt; (repeater — at least one row with data).
    /// </summary>
    public bool HasValue(string key)
    {
        if (!_values.TryGetValue(key, out var val) || val is null)
            return false;

        return val switch
        {
            List<string> list                        => list.Count > 0,
            List<Dictionary<string, object?>> rows   => rows.Count > 0,
            _                                        => val.ToString() is { Length: > 0 }
        };
    }

    public IReadOnlyDictionary<string, object?> AllValues => _values;
}
