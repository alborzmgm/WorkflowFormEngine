namespace FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Declarative validation constraint attached to a FieldDefinition.
/// Evaluated by ValidationService — zero logic lives here.
///
/// Supported types: required | minLength | maxLength | min | max | regex | email
/// </summary>
public sealed class ValidationRule
{
    /// <summary>Rule discriminator (case-insensitive).</summary>
    public string  Type    { get; set; } = string.Empty;

    /// <summary>Constraint value — parsed by ValidationService per rule type.</summary>
    public string? Value   { get; set; }

    /// <summary>Regex pattern string — used when Type = "regex".</summary>
    public string? Pattern { get; set; }

    /// <summary>Custom user-facing error message. Falls back to a generated default.</summary>
    public string? Message { get; set; }
}
