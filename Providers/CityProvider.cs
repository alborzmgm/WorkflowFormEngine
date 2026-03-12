namespace WorkflowFormEngine.Providers;

using WorkflowEngine.Models;

public sealed class CityProvider : IDataSourceProvider
{
    public string Key => "Cities";

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<OptionItem>> _cities =
        new Dictionary<string, IReadOnlyList<OptionItem>>(StringComparer.OrdinalIgnoreCase)
        {
            ["US"] = [new("NYC","New York"),    new("LA","Los Angeles"),  new("CHI","Chicago")],
            ["GB"] = [new("LON","London"),       new("MAN","Manchester"),  new("BIR","Birmingham")],
            ["DE"] = [new("BER","Berlin"),       new("MUN","Munich"),      new("HAM","Hamburg")],
            ["FR"] = [new("PAR","Paris"),        new("LYO","Lyon"),        new("MAR","Marseille")],
            ["AU"] = [new("SYD","Sydney"),       new("MEL","Melbourne"),   new("BRI","Brisbane")],
        };

    public Task<IEnumerable<OptionItem>> GetOptionsAsync(FormContext context)
    {
        var countryId = context.GetValue("CountryId")?.ToString();

        if (string.IsNullOrWhiteSpace(countryId) ||
            !_cities.TryGetValue(countryId, out var cities))
            return Task.FromResult(Enumerable.Empty<OptionItem>());

        return Task.FromResult<IEnumerable<OptionItem>>(cities);
    }
}
