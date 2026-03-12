namespace WorkflowFormEngine.Providers;

using WorkflowEngine.Models;

/// <summary>
/// Provides options for the "Primary Interest" select field.
///
/// KEY ARCHITECTURAL DEMO:
/// This provider reads the List&lt;string&gt; stored by CheckboxListField ("Interests")
/// from FormContext and returns ONLY those selected items as select options.
/// This shows that DependsOn + provider filtering works identically whether the
/// parent field is a scalar select or a multi-value checkboxlist.
///
/// JSON wires it up with:
///   "dependsOn": "Interests"
///   "dataSource": "PrimaryInterest"
/// </summary>
public sealed class PrimaryInterestProvider : IDataSourceProvider
{
    public string Key => "PrimaryInterest";

    // Master label lookup — mirrors InterestsProvider values exactly.
    private static readonly IReadOnlyDictionary<string, string> _labels =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["tech"]        = "Technology",
            ["health"]      = "Health & Wellness",
            ["finance"]     = "Finance",
            ["travel"]      = "Travel",
            ["culture"]     = "Arts & Culture",
            ["sports-nfl"]  = "NFL",
            ["sports-nba"]  = "NBA",
            ["sports-epl"]  = "Premier League",
            ["cricket"]     = "Cricket",
            ["afl"]         = "Australian Football",
            ["bundesliga"]  = "Bundesliga",
            ["auto"]        = "Automotive",
            ["ligue1"]      = "Ligue 1",
            ["cuisine"]     = "Cuisine & Gastronomy",
        };

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        // Read the List<string> written by CheckboxListField for "Interests"
        var selected = context.GetValue("Interests") as List<string> ?? [];

        if (selected.Count == 0)
            return Task.FromResult(Enumerable.Empty<OptionItem>());

        var options = selected
            .Where(v => _labels.ContainsKey(v))
            .Select(v => new OptionItem(v, _labels[v]));

        return Task.FromResult(options);
    }
}
