namespace WorkflowFormEngine.Services;

using System.Text.RegularExpressions;
using WorkflowEngine.Models;

/// <summary>
/// Stateless rule evaluator for ValidationRule lists.
/// Registered as singleton — no per-request state.
///
/// Supported rule types by field category:
///   All fields    : required
///   Scalar fields : minLength | maxLength | min | max | regex | email
///   Checkbox list : minItems  | maxItems
///   Repeater      : minRows   | maxRows
///   (Sub-field rules are the same as scalar rules applied per-row)
/// </summary>
public sealed class ValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates a single field's top-level value.
    /// For repeaters this covers required / minRows / maxRows only.
    /// Per-row sub-field errors are returned by ValidateRepeaterRows().
    /// </summary>
    public IReadOnlyList<string> ValidateField(FieldDefinition field, object? value)
    {
        // ── Repeater top-level ────────────────────────────────────────────────
        if (field.FieldType == "repeater")
            return ValidateRepeaterTopLevel(field, value as List<Dictionary<string, object?>>);

        // ── Checkbox list ─────────────────────────────────────────────────────
        if (value is List<string> listVal)
            return ValidateListField(field, listVal);

        // ── Scalar (text, number, date, select) ───────────────────────────────
        return ValidateScalarField(field, value);
    }

    /// <summary>
    /// Validates all visible fields in a step.
    /// Hidden fields are excluded so they never block step navigation.
    /// </summary>
    public Dictionary<string, IReadOnlyList<string>> ValidateStep(
        StepDefinition step,
        FormContext context,
        IEnumerable<string> visibleFieldKeys)
    {
        var visible = new HashSet<string>(visibleFieldKeys, StringComparer.OrdinalIgnoreCase);
        var result  = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in step.Fields)
        {
            if (!visible.Contains(field.Key)) continue;
            var errors = ValidateField(field, context.GetValue(field.Key));
            if (errors.Count > 0) result[field.Key] = errors;
        }

        return result;
    }

    /// <summary>
    /// Validates sub-fields within every repeater row.
    /// Returns a map of rowIndex → (subFieldKey → error list).
    /// Only rows/fields with errors appear in the result.
    /// Called by RepeaterField directly so per-row errors can be shown inline.
    /// </summary>
    public Dictionary<int, Dictionary<string, IReadOnlyList<string>>> ValidateRepeaterRows(
        FieldDefinition repeaterField,
        List<Dictionary<string, object?>> rows)
    {
        var result = new Dictionary<int, Dictionary<string, IReadOnlyList<string>>>();

        for (int i = 0; i < rows.Count; i++)
        {
            var rowErrors = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var sub in repeaterField.SubFields)
            {
                rows[i].TryGetValue(sub.Key, out var cellValue);
                var errors = ValidateScalarField(sub, cellValue);
                if (errors.Count > 0)
                    rowErrors[sub.Key] = errors;
            }

            if (rowErrors.Count > 0)
                result[i] = rowErrors;
        }

        return result;
    }

    /// <summary>
    /// Returns true when the step has no validation errors AND no repeater row errors.
    /// </summary>
    public bool IsStepValid(
        StepDefinition step,
        FormContext context,
        IEnumerable<string> visibleFieldKeys)
    {
        var visibleList  = visibleFieldKeys.ToList();
        var topErrors    = ValidateStep(step, context, visibleList);
        if (topErrors.Count > 0) return false;

        // Also check each repeater's rows
        foreach (var field in step.Fields.Where(f => f.FieldType == "repeater"))
        {
            if (context.GetValue(field.Key) is List<Dictionary<string, object?>> rows &&
                ValidateRepeaterRows(field, rows).Count > 0)
                return false;
        }

        return true;
    }

    // ── Repeater top-level ────────────────────────────────────────────────────

    private IReadOnlyList<string> ValidateRepeaterTopLevel(
        FieldDefinition field,
        List<Dictionary<string, object?>>? rows)
    {
        rows ??= [];
        var errors = new List<string>();
        var empty  = rows.Count == 0;

        if (field.Required && empty)
        {
            errors.Add($"Please add at least one {field.Label} entry.");
            return errors;
        }

        foreach (var rule in field.ValidationRules)
        {
            var msg = rule.Type.ToLowerInvariant() switch
            {
                "required" => empty
                    ? rule.Message ?? $"Please add at least one {field.Label} entry."
                    : null,

                "minrows"  => int.TryParse(rule.Value, out var min) && rows.Count < min
                    ? rule.Message ?? $"Please add at least {min} {field.Label} row(s)."
                    : null,

                "maxrows"  => int.TryParse(rule.Value, out var max) && rows.Count > max
                    ? rule.Message ?? $"You may add at most {max} {field.Label} row(s)."
                    : null,

                _ => null // silently ignore inapplicable rule types
            };

            if (msg is not null) errors.Add(msg);
        }

        return errors;
    }

    // ── Checkbox list ─────────────────────────────────────────────────────────

    private static IReadOnlyList<string> ValidateListField(
        FieldDefinition field, List<string> list)
    {
        var errors = new List<string>();
        var empty  = list.Count == 0;

        if (field.Required && empty)
        {
            errors.Add($"Please select at least one {field.Label} option.");
            return errors;
        }

        foreach (var rule in field.ValidationRules)
        {
            var msg = rule.Type.ToLowerInvariant() switch
            {
                "required" => empty
                    ? rule.Message ?? $"Please select at least one {field.Label} option."
                    : null,
                "minitems" => int.TryParse(rule.Value, out var min) && list.Count < min
                    ? rule.Message ?? $"Please select at least {min} {field.Label} option(s)."
                    : null,
                "maxitems" => int.TryParse(rule.Value, out var max) && list.Count > max
                    ? rule.Message ?? $"You may select at most {max} {field.Label} option(s)."
                    : null,
                _ => null
            };

            if (msg is not null) errors.Add(msg);
        }

        return errors;
    }

    // ── Scalar field (text, number, date, select, repeater sub-fields) ────────

    private static IReadOnlyList<string> ValidateScalarField(
        FieldDefinition field, object? value)
    {
        var errors = new List<string>();
        var str    = value?.ToString() ?? string.Empty;
        var empty  = string.IsNullOrWhiteSpace(str);

        if (field.Required && empty)
        {
            errors.Add($"{field.Label} is required.");
            return errors;
        }

        foreach (var rule in field.ValidationRules)
        {
            var msg = EvaluateScalarRule(rule, field.Label, str, empty);
            if (msg is not null) errors.Add(msg);
        }

        return errors;
    }

    private static string? EvaluateScalarRule(
        ValidationRule rule, string label, string val, bool empty) =>
        rule.Type.ToLowerInvariant() switch
        {
            "required"  => empty
                ? rule.Message ?? $"{label} is required."
                : null,

            "minlength" => int.TryParse(rule.Value, out var min) && val.Length < min
                ? rule.Message ?? $"{label} must be at least {min} characters."
                : null,

            "maxlength" => int.TryParse(rule.Value, out var max) && val.Length > max
                ? rule.Message ?? $"{label} must not exceed {max} characters."
                : null,

            "min"       => !empty &&
                           decimal.TryParse(rule.Value, out var lo) &&
                           decimal.TryParse(val, out var n) && n < lo
                ? rule.Message ?? $"{label} must be at least {lo}."
                : null,

            "max"       => !empty &&
                           decimal.TryParse(rule.Value, out var hi) &&
                           decimal.TryParse(val, out var n2) && n2 > hi
                ? rule.Message ?? $"{label} must be no greater than {hi}."
                : null,

            "regex"     => !empty && rule.Pattern is { Length: > 0 } &&
                           !Regex.IsMatch(val, rule.Pattern)
                ? rule.Message ?? $"{label} format is invalid."
                : null,

            "email"     => !empty && !EmailRegex.IsMatch(val)
                ? rule.Message ?? $"{label} must be a valid email address."
                : null,

            // Silently ignore list/repeater rules applied to scalar fields
            "minitems" or "maxitems" or "minrows" or "maxrows" => null,

            _ => throw new NotSupportedException(
                     $"Unknown ValidationRule type '{rule.Type}'.")
        };
}
