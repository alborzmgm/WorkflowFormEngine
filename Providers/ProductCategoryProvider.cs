namespace WorkflowFormEngine.Providers;

using WorkflowEngine.Models;

/// <summary>
/// Provides product category options for the repeater Category sub-field.
/// In production, replace with an API or database call.
/// </summary>
public sealed class ProductCategoryProvider : IDataSourceProvider
{
    public string Key => "ProductCategories";

    private static readonly IReadOnlyList<OptionItem> _options =
    [
        new("electronics",    "Electronics"),
        new("software",       "Software & Licenses"),
        new("hardware",       "Hardware & Equipment"),
        new("consumables",    "Consumables & Supplies"),
        new("services",       "Professional Services"),
        new("subscriptions",  "Subscriptions"),
    ];

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context) =>
        Task.FromResult<IEnumerable<OptionItem>>(_options);
}
