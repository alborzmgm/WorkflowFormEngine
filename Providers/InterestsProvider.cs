namespace WorkflowFormEngine.Providers;

using WorkflowEngine.Models;

/// <summary>
/// Provides interest / topic options.
/// Demonstrates that a CheckboxListField can also use a context-aware
/// provider — options here are filtered by the selected country, showing
/// that the DependsOn + provider pattern works identically for checkboxlists.
/// </summary>
public sealed class InterestsProvider : IDataSourceProvider
{
    public string Key => "Interests";

    // Common options shown for every country
    private static readonly IReadOnlyList<OptionItem> _common =
    [
        new("tech",    "Technology"),
        new("health",  "Health & Wellness"),
        new("finance", "Finance"),
        new("travel",  "Travel"),
        new("culture", "Arts & Culture"),
    ];

    // Additional region-specific options
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<OptionItem>> _extra =
        new Dictionary<string, IReadOnlyList<OptionItem>>(StringComparer.OrdinalIgnoreCase)
        {
            ["US"] = [new("sports-nfl", "NFL"), new("sports-nba", "NBA")],
            ["GB"] = [new("sports-epl", "Premier League"), new("cricket", "Cricket")],
            ["AU"] = [new("afl", "Australian Football"), new("cricket", "Cricket")],
            ["DE"] = [new("bundesliga", "Bundesliga"), new("auto", "Automotive")],
            ["FR"] = [new("ligue1", "Ligue 1"), new("cuisine", "Cuisine & Gastronomy")],
        };

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        var country = context.GetValue("CountryId")?.ToString();

        IEnumerable<OptionItem> result = _common;

        if (!string.IsNullOrWhiteSpace(country) &&
            _extra.TryGetValue(country, out var extras))
        {
            result = result.Concat(extras);
        }

        return Task.FromResult(result);
    }
}
