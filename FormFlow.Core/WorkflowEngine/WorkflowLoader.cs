namespace FormFlow.Core.WorkflowEngine;

using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using FormFlow.Core.WorkflowEngine.Models;

/// <summary>
/// Deserializes a WorkflowDefinition from a JSON file in wwwroot/workflows/.
///
/// Uses IWebHostEnvironment to read the file directly from disk — no HTTP
/// round-trip, works reliably on Blazor Server without a running HttpClient.
/// </summary>
public sealed class WorkflowLoader(IWebHostEnvironment env)
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas         = true,
        ReadCommentHandling         = JsonCommentHandling.Skip,
    };

    /// <summary>Loads a workflow JSON file from wwwroot/workflows/{fileName}.</summary>
    public async Task<WorkflowDefinition> LoadAsync(string fileName)
    {
        var path = Path.Combine(env.WebRootPath, "workflows", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Workflow file not found: {path}. " +
                $"Ensure it is placed under wwwroot/workflows/.");

        var json = await File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<WorkflowDefinition>(json, Options)
               ?? throw new InvalidOperationException(
                      $"Workflow JSON '{fileName}' deserialized to null.");
    }
}
