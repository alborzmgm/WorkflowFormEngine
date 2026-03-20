namespace FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Declarative visibility condition attached to a FieldDefinition.
/// Evaluated by ConditionEvaluator — zero logic lives here.
///
/// Supported operators (case-insensitive):
///   equals | notEquals | hasValue | isEmpty | contains | greaterThan | lessThan
///
/// Example JSON:
///   "visibleWhen": { "field": "CountryId", "operator": "equals", "value": "US" }
/// </summary>
public sealed class ConditionRule
{
    /// <summary>Key of the controlling field whose value is tested.</summary>
    public string  Field    { get; set; } = string.Empty;

    /// <summary>Comparison operator. Defaults to "equals".</summary>
    public string  Operator { get; set; } = "equals";

    /// <summary>Expected value (required for equals / notEquals / contains / greaterThan / lessThan).</summary>
    public string? Value    { get; set; }
}
