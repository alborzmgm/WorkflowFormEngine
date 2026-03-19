namespace WorkflowFormEngine.WorkflowEngine.Models;

/// <summary>
/// Declarative metadata for a single form field.
/// Deserialized directly from workflow JSON — contains NO business logic.
/// </summary>
public sealed class FieldDefinition
{
    public string Key       { get; set; } = string.Empty;
    public string Label     { get; set; } = string.Empty;

    /// <summary>"text" | "number" | "select"</summary>
    public string FieldType { get; set; } = string.Empty;

    /// <summary>IDataSourceProvider key — only used when FieldType = "select".</summary>
    public string? DataSource { get; set; }

    /// <summary>Parent field key for dependent option loading.</summary>
    public string? DependsOn { get; set; }

    /// <summary>When present, the field is hidden unless this condition is true.</summary>
    public ConditionRule? VisibleWhen { get; set; }

    /// <summary>Declarative validation constraints evaluated by ValidationService.</summary>
    public List<ValidationRule> ValidationRules { get; set; } = [];

    /// <summary>Shorthand — adds an implicit "required" rule at validation time.</summary>
    public bool Required { get; set; }

    public int     Order       { get; set; }
    public string? Placeholder { get; set; }

    /// <summary>
    /// Number of visible text rows for "textarea" field type. Defaults to 4.
    /// Ignored by all other field types.
    /// </summary>
    public int Rows { get; set; } = 4;

    /// <summary>
    /// Column definitions for "repeatablelist" field type.
    /// Each entry describes one sub-field that the user fills in per row.
    /// Ignored by all other field types.
    /// </summary>
    public List<ColumnDefinition> Columns { get; set; } = [];

    /// <summary>
    /// Sub-field definitions for the "repeater" field type.
    /// Each entry is a full <see cref="FieldDefinition"/> — every field type,
    /// validation rule, and visibility condition is supported within an entry.
    /// Ignored by all other field types.
    /// </summary>
    public List<FieldDefinition> SubFields { get; set; } = [];
}
