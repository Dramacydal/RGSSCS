using System.Collections;

namespace RGSSLib;

public class FileTable : IEnumerable<TableEntry>
{
    private readonly Dictionary<string, TableEntry> _entries = new();

    public bool HasEntry(string path) => _entries.ContainsKey(path.ToLowerInvariant());

    public void Add(TableEntry entry)
    {
        _entries[entry.Path.ToLowerInvariant()] = entry;
    }

    public TableEntry? GetEntry(string path) => _entries.GetValueOrDefault(path.ToLowerInvariant());

    public IEnumerator<TableEntry> GetEnumerator() => _entries.Select(e => e.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
