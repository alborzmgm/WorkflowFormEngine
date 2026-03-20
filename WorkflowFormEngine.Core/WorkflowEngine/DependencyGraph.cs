namespace WorkflowFormEngine.Core.WorkflowEngine;

using WorkflowFormEngine.Core.WorkflowEngine.Models;

/// <summary>
/// Directed dependency graph built from FieldDefinition.DependsOn metadata.
/// BFS traversal supports multi-level cascades: A → B → C.
/// </summary>
public sealed class DependencyGraph
{
    private readonly Dictionary<string, List<string>> _edges =
        new(StringComparer.OrdinalIgnoreCase);

    public void Build(IEnumerable<FieldDefinition> fields)
    {
        _edges.Clear();
        foreach (var field in fields)
        {
            if (field.DependsOn is null) continue;
            if (!_edges.TryGetValue(field.DependsOn, out var deps))
                _edges[field.DependsOn] = deps = [];
            deps.Add(field.Key);
        }
    }

    public IReadOnlyList<string> GetDirectDependents(string key) =>
        _edges.TryGetValue(key, out var d) ? d : [];

    /// <summary>BFS — returns all downstream field keys in cascade order.</summary>
    public IEnumerable<string> GetAllDownstream(string key)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue   = new Queue<string>(GetDirectDependents(key));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current)) continue;   // cycle guard
            yield return current;
            foreach (var next in GetDirectDependents(current))
                queue.Enqueue(next);
        }
    }
}
