namespace WorkflowFormEngine.App.Providers;

using WorkflowFormEngine.Core.Providers;
using WorkflowFormEngine.Core.WorkflowEngine.Models;

/// <summary>Static options for how often the user wishes to be contacted.</summary>
public sealed class NotificationFrequencyProvider : IDataSourceProvider
{
    public string Key => "NotificationFrequency";

    private static readonly IReadOnlyList<OptionItem> _options =
    [
        new("realtime",  "Real-time (as it happens)"),
        new("daily",     "Daily digest"),
        new("weekly",    "Weekly summary"),
        new("monthly",   "Monthly newsletter"),
        new("minimal",   "Important updates only"),
    ];

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context) =>
        Task.FromResult<IEnumerable<OptionItem>>(_options);
}
