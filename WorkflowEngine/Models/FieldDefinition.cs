namespace WorkflowFormEngine.WorkflowEngine.Models;

/// <summary>
/// Declarative metadata for a single form field.
/// Deserialized directly from workflow JSON — contains NO business logic.
///
/// Supported fieldType values:
///   text | number | date | select | checkboxlist | repeater
/// </summary>
public sealed class FieldDefinition
{
    public string Key       { get; set; } = string.Empty;
    public string Label     { get; set; } = string.Empty;

    /// <summary>"text" | "number" | "date" | "select" | "checkboxlist" | "repeater"</summary>
    public string FieldType { get; set; } = string.Empty;

    /// <summary>IDataSourceProvider key — used by select and checkboxlist.</summary>
    public string? DataSource { get; set; }

    /// <summary>Parent field key for dependent option loading and cascade reset.</summary>
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
    /// Sub-field definitions for "repeater" fields only.
    /// Each entry describes one column in every repeater row.
    /// Sub-fields support the same fieldType values as top-level fields
    /// except "repeater" itself (no nesting).
    /// </summary>
    public List<FieldDefinition> SubFields { get; set; } = [];

    /// <summary>
    /// Hints the grid column width for this field when rendered inside a
    /// repeater card. Ignored for top-level fields.
    ///
    /// Values (map to Bootstrap col classes):
    ///   "full"       → col-12
    ///   "half"       → col-12 col-sm-6   (default)
    ///   "third"      → col-12 col-sm-4
    ///   "two-thirds" → col-12 col-sm-8
    ///   "quarter"    → col-12 col-sm-3
    /// </summary>
    public string Width { get; set; } = "half";
    public int Rows { get; set; } = 4;

}
