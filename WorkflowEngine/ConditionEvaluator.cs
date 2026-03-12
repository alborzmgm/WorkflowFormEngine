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
    /// </summary>
    public static bool IsVisible(ConditionRule? condition, FormContext context)
    {
        if (condition is null) return true;

        var raw    = context.GetValue(condition.Field)?.ToString() ?? string.Empty;
        var target = condition.Value ?? string.Empty;

        return condition.Operator.ToLowerInvariant() switch
        {
            "equals"      => string.Equals(raw, target, StringComparison.OrdinalIgnoreCase),
            "notequals"   => !string.Equals(raw, target, StringComparison.OrdinalIgnoreCase),
            "hasvalue"    => !string.IsNullOrWhiteSpace(raw),
            "isempty"     => string.IsNullOrWhiteSpace(raw),
            "contains"    => raw.Contains(target, StringComparison.OrdinalIgnoreCase),
            "greaterthan" => decimal.TryParse(raw, out var a) &&
                             decimal.TryParse(target, out var b) && a > b,
            "lessthan"    => decimal.TryParse(raw, out var c) &&
                             decimal.TryParse(target, out var d) && c < d,
            _ => throw new NotSupportedException(
                     $"Unknown ConditionRule operator '{condition.Operator}'.")
        };
    }
}
