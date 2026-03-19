namespace WorkflowFormEngine.Components;

using Microsoft.AspNetCore.Components;
using Services;
using WorkflowEngine.Models;

/// <summary>
/// Shared abstract base for all dynamically rendered field components.
/// DynamicComponent receives a single Dictionary&lt;string, object?&gt; whose
/// keys match these parameter names exactly.
///
/// All field types share the same parameter surface so BuildParameters()
/// in DynamicForm never needs branching per type. Components use only
/// the parameters they need and ignore the rest.
/// </summary>
public abstract class FieldComponentBase : ComponentBase
{
    [Parameter, EditorRequired]
    public FieldDefinition Field { get; set; } = default!;

    [Parameter]
    public object? Value { get; set; }

    [Parameter]
    public EventCallback<object?> OnValueChanged { get; set; }

    [Parameter]
    public FormContext? FormContext { get; set; }

    [Parameter]
    public OptionService? OptionService { get; set; }

    /// <summary>
    /// Top-level validation errors for this field (e.g. "required", minRows, maxRows).
    /// Set by DynamicForm after a validation pass. Empty when pristine or valid.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// Per-row, per-sub-field validation errors — used exclusively by RepeaterField.
    /// All other field types receive an empty dictionary and ignore it.
    /// Key: row index → (sub-field key → error list).
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<int, IReadOnlyDictionary<string, IReadOnlyList<string>>> RowErrors { get; set; }
        = new Dictionary<int, IReadOnlyDictionary<string, IReadOnlyList<string>>>();
}
