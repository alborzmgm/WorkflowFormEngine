namespace WorkflowFormEngine.Components;

using Microsoft.AspNetCore.Components;
using WorkflowFormEngine.Core.Services;
using WorkflowFormEngine.Core.WorkflowEngine.Models;

/// <summary>
/// Shared abstract base for all dynamically rendered field components.
/// DynamicComponent receives a single Dictionary&lt;string, object?&gt; whose
/// keys match these parameter names exactly — all field types share the same
/// surface so BuildParameters() never needs branching per type.
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
    /// Per-field validation errors, set by DynamicForm after a validation pass.
    /// Empty list when the field is pristine or valid.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string> ValidationErrors { get; set; } = [];
}
