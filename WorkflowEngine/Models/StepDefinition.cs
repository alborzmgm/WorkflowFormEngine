namespace WorkflowFormEngine.WorkflowEngine.Models;

/// <summary>
/// A single step within a workflow.
///
/// StepType values:
///   "form"   (default) — standard data-entry step with fields
///   "review" — auto-generated read-only summary of all previous steps;
///              Fields list is ignored for review steps (engine builds the view)
/// </summary>
public sealed class StepDefinition
{
    public string StepKey { get; set; } = string.Empty;
    public string Title   { get; set; } = string.Empty;

    /// <summary>Defaults to "form". Use "review" for the confirmation step.</summary>
    public string StepType { get; set; } = "form";

    public List<FieldDefinition> Fields { get; set; } = [];

    public bool IsReview => StepType.Equals("review", StringComparison.OrdinalIgnoreCase);
}
