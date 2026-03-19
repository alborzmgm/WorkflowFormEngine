namespace WorkflowFormEngine.WorkflowEngine.Models;

/// <summary>
/// Defines a single column (sub-field) inside a "repeatablelist" field.
/// Deserialized from workflow JSON alongside <see cref="FieldDefinition"/>.
/// </summary>
public sealed class ColumnDefinition
{
    public string Key   { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;

    /// <summary>"text" | "number" | "date" — controls the HTML input type rendered per cell.</summary>
    public string ColumnType { get; set; } = "text";

    /// <summary>When true the cell must be non-empty before the step can advance.</summary>
    public bool Required { get; set; }

    public string? Placeholder { get; set; }
}
