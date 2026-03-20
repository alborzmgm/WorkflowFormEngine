namespace FormFlow.Core.Services;

using System.Text.RegularExpressions;
using FormFlow.Core.WorkflowEngine;
using FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Stateless rule evaluator for ValidationRule lists.
/// Registered as singleton — no per-request state.
///
/// Supported rule types:
///   Scalar fields:         required | minLength | maxLength | min | max | regex | email
///   List fields:           required | minItems  | maxItems
///   Repeater fields:       required | minEntries | maxEntries
/// </summary>
public sealed class ValidationService
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Validates one field value; returns an empty list when valid.</summary>
    public IReadOnlyList<string> ValidateField(FieldDefinition field, object? value)
    {
        var errors = new List<string>();

        // ── Repeater field ────────────────────────────────────────────────
        if (field.FieldType == "repeater" || value is List<Dictionary<string, object?>>)
        {
            var entryList = value as List<Dictionary<string, object?>> ?? [];

            if (field.Required && entryList.Count == 0)
            {
                var requiredMsg = field.ValidationRules
                    .FirstOrDefault(r => string.Equals(r.Type, "required", StringComparison.OrdinalIgnoreCase))
                    ?.Message;
                errors.Add(requiredMsg ?? $"Please add at least one {field.Label} entry.");
                return errors;
            }

            // Per-entry, per-subfield validation — only track whether any entry has errors;
            // the detailed per-field messages are displayed inline inside each entry card
            // and should not be duplicated at the top-level / step-banner level.
            bool anyEntryErrors = false;
            for (int i = 0; i < entryList.Count; i++)
            {
                var entry    = entryList[i];
                var entryCtx = new FormContext();
                foreach (var kv in entry) entryCtx.SetValue(kv.Key, kv.Value);

                foreach (var subField in field.SubFields.OrderBy(sf => sf.Order))
                {
                    if (!ConditionEvaluator.IsVisible(subField.VisibleWhen, entryCtx)) continue;

                    var subValue  = entry.TryGetValue(subField.Key, out var sv) ? sv : null;
                    var subErrors = ValidateField(subField, subValue);
                    if (subErrors.Count > 0) { anyEntryErrors = true; break; }
                }

                if (anyEntryErrors) break;
            }

            if (anyEntryErrors)
                errors.Add("Some entries have validation errors — please review them below.");

            foreach (var rule in field.ValidationRules)
            {
                var msg = EvaluateRepeaterRule(rule, field.Label, entryList.Count);
                if (msg is not null) errors.Add(msg);
            }

            return errors;
        }

        // ── Checkbox list field ────────────────────────────────────────────

        // Resolve whether this is a list value (CheckboxListField) or a scalar.
        var isList     = value is List<string>;
        var list       = value as List<string>;
        var listEmpty  = isList && (list is null || list.Count == 0);

        var str        = isList ? string.Join(", ", list ?? []) : value?.ToString() ?? string.Empty;
        var scalarEmpty = !isList && string.IsNullOrWhiteSpace(str);
        var empty       = isList ? listEmpty : scalarEmpty;

        // Honour the Required shorthand — bail early when field is empty.
        if (field.Required && empty)
        {
            errors.Add(isList
                ? $"Please select at least one {field.Label} option."
                : $"{field.Label} is required.");
            return errors;
        }

        foreach (var rule in field.ValidationRules)
        {
            var msg = isList
                ? EvaluateListRule(rule, field.Label, list ?? [], listEmpty)
                : EvaluateScalarRule(rule, field.Label, str, scalarEmpty);

            if (msg is not null) errors.Add(msg);
        }

        return errors;
    }

    /// <summary>
    /// Validates all visible fields in a step.
    /// Hidden fields (not in visibleFieldKeys) are excluded — they must not block navigation.
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

    // ── Scalar rule evaluation ────────────────────────────────────────────────

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
                           decimal.TryParse(val, out var n)  && n < lo
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

            // List-only rules applied to a scalar field are silently ignored.
            "minitems" or "maxitems" => null,

            _ => throw new NotSupportedException(
                     $"Unknown ValidationRule type '{rule.Type}'.")
        };

    // ── List rule evaluation (CheckboxListField) ──────────────────────────────

    private static string? EvaluateListRule(
        ValidationRule rule, string label, List<string> list, bool empty) =>

        rule.Type.ToLowerInvariant() switch
        {
            "required"  => empty
                           ? rule.Message ?? $"Please select at least one {label} option."
                           : null,

            "minitems"  => int.TryParse(rule.Value, out var min) && list.Count < min
                           ? rule.Message ?? $"Please select at least {min} {label} option(s)."
                           : null,

            "maxitems"  => int.TryParse(rule.Value, out var max) && list.Count > max
                           ? rule.Message ?? $"You may select at most {max} {label} option(s)."
                           : null,

            // Scalar-only rules applied to a list field are silently ignored.
            "minlength" or "maxlength" or "min" or "max" or "regex" or "email" => null,

            _ => throw new NotSupportedException(
                     $"Unknown ValidationRule type '{rule.Type}'.")
        };

    // ── Repeater rule evaluation (RepeaterField) ──────────────────────────────

    private static string? EvaluateRepeaterRule(
        ValidationRule rule, string label, int entryCount) =>

        rule.Type.ToLowerInvariant() switch
        {
            "required"    => entryCount == 0
                             ? rule.Message ?? $"Please add at least one {label} entry."
                             : null,

            "minentries"  => int.TryParse(rule.Value, out var min) && entryCount < min
                             ? rule.Message ?? $"Please add at least {min} {label} {Entries(min)}."
                             : null,

            "maxentries"  => int.TryParse(rule.Value, out var max) && entryCount > max
                             ? rule.Message ?? $"You may add at most {max} {label} {Entries(max)}."
                             : null,

            // Rules for other field types are silently ignored on repeater fields.
            "minitems" or "maxitems" or
            "minlength" or "maxlength" or
            "min" or "max" or "regex" or "email" => null,

            _ => throw new NotSupportedException(
                     $"Unknown ValidationRule type '{rule.Type}'.")
        };

    private static string Entries(int count) => count == 1 ? "entry" : "entries";
}
