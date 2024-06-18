namespace RGSSLib;

public abstract class AbstractArchiveReader : IDisposable
{
    protected readonly BinaryReader reader;
    public FileTable Table { get; init; }
    public ArchiveVersion Version { get; init; }

    protected AbstractArchiveReader(BinaryReader reader, ArchiveVersion version)
    {
        this.reader = reader;
        Version = version;
        Table = ReadTable();
    }

    protected abstract string ReadString(ref uint u);

    protected int ReadInt(ref uint key)
    {
        return DecryptInt(reader.ReadInt32(), ref key);
    }

    protected abstract FileTable ReadTable();

    protected abstract int DecryptInt(int value, ref uint key);

    protected void RotateKey(ref uint key)
    {
        key = key * 7 + 3;
    }

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

    public Stream GetFileStream(string path)
    {
        return new MemoryStream(GetFileContent(path).ToArray());
    }

    public Stream GetFileStream(TableEntry entry)
    {
        return new MemoryStream(GetFileContent(entry).ToArray());
    }

    public void ExtractAll(string path)
    {
        foreach (var entry in Table)
            Extract(entry, path);
    }

    public void Extract(TableEntry entry, string path)
    {
        if (!Directory.Exists(path))
            throw new Exception($"Directory {path} does not exist");

        var directory = Path.GetDirectoryName(entry.Path);
        if (!string.IsNullOrEmpty(directory))
            path = Path.Combine(path, directory);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        File.WriteAllBytes(Path.Combine(path, Path.GetFileName(entry.Path)), GetFileContent(entry).ToArray());
    }

    public void Dispose()
    {
        reader.Dispose();
    }
}
