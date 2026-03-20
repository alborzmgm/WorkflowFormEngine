namespace WorkflowFormEngine.Core.Providers;

using WorkflowFormEngine.Core.WorkflowEngine.Models;

/// <summary>
/// Contract for all dynamic data sources.
/// Key must match the "dataSource" value in the workflow JSON.
/// Providers receive FormContext so dependent fields can filter by parent values.
/// </summary>
public interface IDataSourceProvider
{
    string Key { get; }
    Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context);
}
