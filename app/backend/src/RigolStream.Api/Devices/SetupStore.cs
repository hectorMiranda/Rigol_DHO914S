using System.Collections.Concurrent;
using RigolStream.Api.Models;

namespace RigolStream.Api.Devices;

/// <summary>
/// In-memory store of named setups, registered as a singleton. (Functions storage
/// is ephemeral; a durable store would back this with Blob/Table storage.)
/// </summary>
public sealed class SetupStore
{
    private readonly ConcurrentDictionary<string, Setup> _setups = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<SetupSummary> List() =>
        _setups.Values
            .OrderBy(s => s.Name)
            .Select(s => new SetupSummary(s.Name, s.SavedAt, s.Channels.Count))
            .ToList();

    public Setup? Get(string name) => _setups.TryGetValue(name, out var s) ? s : null;

    public Setup Save(Setup setup)
    {
        _setups[setup.Name] = setup;
        return setup;
    }

    public bool Delete(string name) => _setups.TryRemove(name, out _);
}
