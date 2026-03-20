namespace FormFlow.App.Providers;

using FormFlow.Core.Providers;
using FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Provides preferred contact time options filtered by the selected contact methods.
///
/// Demonstrates a provider that reads a List&lt;string&gt; (from CheckboxListField)
/// to return context-aware options for a dependent select field.
///
/// Rules:
///   • Phone / SMS / WhatsApp selected  → time-of-day slots are relevant
///   • Email / Post only               → show day-of-week options instead
///   • Mix                             → show both sets merged
/// </summary>
public sealed class ContactTimeProvider : IDataSourceProvider
{
    public string Key => "ContactTime";

    private static readonly IReadOnlyList<OptionItem> _timeSlots =
    [
        new("morning",   "Morning (8 am – 12 pm)"),
        new("afternoon", "Afternoon (12 pm – 5 pm)"),
        new("evening",   "Evening (5 pm – 8 pm)"),
    ];

    private static readonly IReadOnlyList<OptionItem> _days =
    [
        new("weekdays",  "Weekdays (Mon – Fri)"),
        new("weekend",   "Weekends (Sat – Sun)"),
        new("any",       "Any day"),
    ];

    private static readonly HashSet<string> _realtimeMethods =
        new(["phone", "sms", "whatsapp"], StringComparer.OrdinalIgnoreCase);

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        var selected = context.GetValue("PreferredContactMethods") as List<string> ?? [];

        if (selected.Count == 0)
            return Task.FromResult(Enumerable.Empty<OptionItem>());

        var hasRealtime = selected.Any(m => _realtimeMethods.Contains(m));
        var hasAsync    = selected.Any(m => !_realtimeMethods.Contains(m)); // email, post

        IEnumerable<OptionItem> result = [];

        if (hasRealtime) result = result.Concat(_timeSlots);
        if (hasAsync)    result = result.Concat(_days);

        return Task.FromResult(result);
    }
}
