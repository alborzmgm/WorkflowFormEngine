namespace WorkflowFormEngine.Providers;

using WorkflowEngine.Models;

/// <summary>Static newsletter topic options for step 3.</summary>
public sealed class NewsletterTopicsProvider : IDataSourceProvider
{
    public string Key => "NewsletterTopics";

    private static readonly IReadOnlyList<OptionItem> _options =
    [
        new("product-updates", "Product Updates"),
        new("industry-news",   "Industry News"),
        new("tips-tricks",     "Tips & Best Practices"),
        new("events",          "Events & Webinars"),
        new("case-studies",    "Case Studies"),
    ];

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context) =>
        Task.FromResult<IEnumerable<OptionItem>>(_options);
}
