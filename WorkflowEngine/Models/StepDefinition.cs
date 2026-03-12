namespace WorkflowFormEngine.WorkflowEngine.Models;

public sealed class StepDefinition
{
    public string              StepKey { get; set; } = string.Empty;
    public string              Title   { get; set; } = string.Empty;
    public List<FieldDefinition> Fields { get; set; } = [];
}
