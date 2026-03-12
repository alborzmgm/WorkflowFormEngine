namespace WorkflowFormEngine.WorkflowEngine.Models;

public sealed class StepDefinition
{
    public string StepKey { get; set; } = string.Empty;
    public string Title   { get; set; } = string.Empty;

    /// <summary>
    /// Short description shown below the step title in the card header.
    /// Defined in workflow JSON — no hardcoding in UI components.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Bootstrap Icons class name (e.g. "bi-geo-alt-fill") for the step card header.
    /// Defined in workflow JSON so the UI is fully data-driven.
    /// </summary>
    public string Icon { get; set; } = "bi-pencil-square";

    public List<FieldDefinition> Fields { get; set; } = [];
}
