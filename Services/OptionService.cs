namespace WorkflowFormEngine.Services;

using Providers;
using WorkflowEngine.Models;

/// <summary>
/// Resolves IDataSourceProvider by key and delegates option loading.
/// New providers need only a DI registration — nothing changes here.
/// </summary>
public sealed class OptionService
{
    private readonly IReadOnlyDictionary<string, IDataSourceProvider> _registry;

    public OptionService(IEnumerable<IDataSourceProvider> providers) =>
        _registry = providers.ToDictionary(p => p.Key, StringComparer.OrdinalIgnoreCase);

    public async Task<IEnumerable<OptionItem>> GetOptionsAsync(
        string dataSourceKey, FormContext context)
    {
        if (!_registry.TryGetValue(dataSourceKey, out var provider))
            throw new KeyNotFoundException(
                $"No IDataSourceProvider for key '{dataSourceKey}'. " +
                $"Registered: [{string.Join(", ", _registry.Keys)}]");

        return await provider.GetOptionsAsync(context);
    }
}
