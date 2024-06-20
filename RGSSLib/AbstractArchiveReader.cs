namespace RGSSLib;

public abstract class AbstractArchiveReader : IDisposable
{
    private bool _disposed;
    protected readonly BinaryReader reader;
    private FileTable? _table;
    private bool _aborted;

    public delegate void ProgressDelegate(int index, int total, string path);
    
    public FileTable Table
    {
        get { return _table ??= ReadTable(); }
    }

    public ArchiveVersion Version { get; init; }

    protected AbstractArchiveReader(BinaryReader reader, ArchiveVersion version)
    {
        this.reader = reader;
        Version = version;
    }

    protected abstract string ReadString(ref uint u);

    protected int ReadInt(ref uint key) => DecryptInt(reader.ReadInt32(), ref key);

    protected abstract FileTable ReadTable();

    protected abstract int DecryptInt(int value, ref uint key);

    protected void RotateKey(ref uint key) => key = key * 7 + 3;

    public IEnumerable<byte> GetFileContent(string path)
    {
        var entry = Table.GetEntry(path);

        if (entry == null)
            return Array.Empty<byte>();

        return GetFileContent(entry);
    }

    public IEnumerable<byte> GetFileContent(TableEntry entry)
    {
        reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

        var key = entry.Key;

        for (var i = 0; i < entry.Size; ++i)
        {
            if ((i % 4) == 0 && i != 0)
                RotateKey(ref key);

            yield return (byte)(reader.ReadByte() ^ (byte)((key >> 8 * (i % 4)) & 0xFF));
        }
    }

    public Stream GetFileStream(string path) => new MemoryStream(GetFileContent(path).ToArray());

    public Stream GetFileStream(TableEntry entry) => new MemoryStream(GetFileContent(entry).ToArray());

    public void ExtractAll(string path, ProgressDelegate? progress = null)
    {
        _aborted = false;
        
        int index = 0;
        foreach (var entry in Table)
        {
            if (_aborted)
                break;
            
            progress?.Invoke(++index, Table.Size, entry.Path);
            Extract(entry, path);
        }
    }

    private HashSet<string> cachedDirectories = new();

    private bool DirectoryExists(string path)
    {
        if (cachedDirectories.Contains(path))
            return true;

        var res = Directory.Exists(path);
        if (res)
            cachedDirectories.Add(path);

        return res;
    }

    public void Extract(TableEntry entry, string path)
    {
        if (!DirectoryExists(path))
            throw new Exception($"Directory '{path}' does not exist");

        var directory = Path.GetDirectoryName(entry.Path);
        if (!string.IsNullOrEmpty(directory))
            path = Path.Combine(path, directory);

        if (!DirectoryExists(path))
            Directory.CreateDirectory(path);

        File.WriteAllBytes(Path.Combine(path, Path.GetFileName(entry.Path)), GetFileContent(entry).ToArray());
    }

    public void Abort() => _aborted = true;

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        reader.Dispose();
    }
}
