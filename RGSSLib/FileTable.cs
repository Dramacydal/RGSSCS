using System.Collections;

namespace RGSSLib;

public class FileTable : IEnumerable<TableEntry>
{
    private readonly Dictionary<string, TableEntry> _entries = new();

    public int Size => _entries.Count;

    public string NormalizePath(string path)
    {
        return path.Replace('/', '\\').ToLowerInvariant();
    }

    public IEnumerable<TableEntry> GetEntriesInPath(string path)
    {
        path = NormalizePath(path);
        
        return _entries.Where(p => p.Key.StartsWith(path)).Select(p => p.Value);
    }
    
    public bool HasEntry(string path) => _entries.ContainsKey(NormalizePath(path));

    public void Add(TableEntry entry) => _entries[NormalizePath(entry.Path)] = entry;

    public TableEntry? GetEntry(string path) => _entries.GetValueOrDefault(NormalizePath(path));

    public IEnumerator<TableEntry> GetEnumerator() => _entries.Select(p => p.Value).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
