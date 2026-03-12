namespace WorkflowFormEngine.WorkflowEngine.Models;

public sealed class WorkflowDefinition
{
    public string              WorkflowKey { get; set; } = string.Empty;
    public string              Name        { get; set; } = string.Empty;
    public List<StepDefinition> Steps      { get; set; } = [];
}
