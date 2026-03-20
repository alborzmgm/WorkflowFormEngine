namespace FormFlow.App.Providers;

using FormFlow.Core.Providers;
using FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Provides preferred contact method options for the CheckboxListField demo.
/// Replace mock data with an API / database call in production.
/// </summary>
public sealed class ContactMethodsProvider : IDataSourceProvider
{
    public string Key => "ContactMethods";

    private static readonly IReadOnlyList<OptionItem> _options =
    [
        new("email",     "Email"),
        new("phone",     "Phone Call"),
        new("sms",       "SMS / Text"),
        new("whatsapp",  "WhatsApp"),
        new("post",      "Postal Mail"),
    ];

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context) =>
        Task.FromResult<IEnumerable<OptionItem>>(_options);
}
