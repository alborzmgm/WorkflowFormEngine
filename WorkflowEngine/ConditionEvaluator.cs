namespace WorkflowFormEngine.WorkflowEngine;

using Models;

/// <summary>
/// Pure static evaluator for ConditionRule visibility expressions.
/// No state, no DI — safe to call directly from Razor components.
/// </summary>
public static class ConditionEvaluator
{
    /// <summary>
    /// Returns true when the field should be visible.
    /// A null condition always returns true (no rule = always visible).
    ///
    /// List fields (CheckboxListField) are handled explicitly:
    ///   hasValue / isEmpty  — delegate to FormContext.HasValue which is list-aware.
    ///   contains            — checks whether the list contains the target value.
    ///   equals / notEquals  — unsupported on list fields; always return false / true.
    /// </summary>
    public static bool IsVisible(ConditionRule? condition, FormContext context)
    {
        if (condition is null) return true;

        var raw    = context.GetValue(condition.Field);
        var target = condition.Value ?? string.Empty;

        // For list-valued fields (CheckboxListField), route operators correctly.
        if (raw is List<string> list)
        {
            return condition.Operator.ToLowerInvariant() switch
            {
                "hasvalue"  => list.Count > 0,
                "isempty"   => list.Count == 0,
                "contains"  => list.Any(v => string.Equals(v, target, StringComparison.OrdinalIgnoreCase)),
                "equals"    => false,   // list ≠ scalar by definition
                "notequals" => true,
                _ => throw new NotSupportedException(
                         $"Operator '{condition.Operator}' is not supported for list-valued fields.")
            };
        }

        // For repeatable-list fields, only hasValue / isEmpty make sense.
        if (raw is List<Dictionary<string, string>> rows)
        {
            bool hasData = rows.Any(r => FormContext.IsRepeatableListRowNonEmpty(r));
            return condition.Operator.ToLowerInvariant() switch
            {
                "hasvalue"  => hasData,
                "isempty"   => !hasData,
                "equals"    => false,
                "notequals" => true,
                _ => throw new NotSupportedException(
                         $"Operator '{condition.Operator}' is not supported for repeatable-list fields.")
            };
        }

        // For repeater fields, only hasValue / isEmpty make sense.
        if (raw is List<Dictionary<string, object?>> entries)
        {
            return condition.Operator.ToLowerInvariant() switch
            {
                "hasvalue"  => entries.Count > 0,
                "isempty"   => entries.Count == 0,
                "equals"    => false,
                "notequals" => true,
                _ => throw new NotSupportedException(
                         $"Operator '{condition.Operator}' is not supported for repeater fields.")
            };
        }

        var str = raw?.ToString() ?? string.Empty;

        return condition.Operator.ToLowerInvariant() switch
        {
            "equals"      => string.Equals(str, target, StringComparison.OrdinalIgnoreCase),
            "notequals"   => !string.Equals(str, target, StringComparison.OrdinalIgnoreCase),
            "hasvalue"    => !string.IsNullOrWhiteSpace(str),
            "isempty"     => string.IsNullOrWhiteSpace(str),
            "contains"    => str.Contains(target, StringComparison.OrdinalIgnoreCase),
            "greaterthan" => decimal.TryParse(str, out var a) &&
                             decimal.TryParse(target, out var b) && a > b,
            "lessthan"    => decimal.TryParse(str, out var c) &&
                             decimal.TryParse(target, out var d) && c < d,
            _ => throw new NotSupportedException(
                     $"Unknown ConditionRule operator '{condition.Operator}'.")
        };
    }
}
