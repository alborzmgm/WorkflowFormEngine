namespace WorkflowFormEngine.App.Providers;

using WorkflowFormEngine.Core.Providers;
using WorkflowFormEngine.Core.WorkflowEngine.Models;

public sealed class CountryProvider : IDataSourceProvider
{
    public string Key => "Countries";

    private static readonly IReadOnlyList<OptionItem> _countries =
    [
        new("US", "United States"),
        new("GB", "United Kingdom"),
        new("DE", "Germany"),
        new("FR", "France"),
        new("AU", "Australia"),
    ];

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context) =>
        Task.FromResult<IEnumerable<OptionItem>>(_countries);
}
