namespace RGSSLibCS;

public abstract class AbstractArchiveReader
{
    protected readonly BinaryReader reader;
    private readonly FileTable _table;

    protected AbstractArchiveReader(BinaryReader reader)
    {
        this.reader = reader;
        _table = ReadTable();
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

    public byte[] GetFileContent(string path)
    {
        var entry = _table.GetEntry(path);

        if (entry == null)
            return Array.Empty<byte>();

        return GetFileContent(entry);
    }

    public byte[] GetFileContent(TableEntry entry)
    {
        reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

        var bytes = reader.ReadBytes(entry.Size);

        var key = entry.Key;

        for (var i = 0; i < bytes.Length; ++i)
        {
            if ((i % 4) == 0 && i != 0)
                RotateKey(ref key);

            bytes[i] ^= (byte)((key >> 8 * (i % 4)) & 0xFF);
        }

        return bytes;
    }

    public Stream GetFileStream(string path)
    {
        return new MemoryStream(GetFileContent(path));
    }
    
    public Stream GetFileStream(TableEntry entry)
    {
        return new MemoryStream(GetFileContent(entry));
    }

    public void ExtractAll(string path)
    {
        foreach (var entry in _table)
            Extract(entry, path);
    }

    public void Extract(TableEntry entry, string path)
    {
        if (!Directory.Exists(path))
            throw new Exception($"Directory {path} does not exist");

        var bytes = GetFileContent(entry);

        var directory = Path.GetDirectoryName(entry.Path);
        if (!string.IsNullOrEmpty(directory))
            path = Path.Combine(path, directory);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        File.WriteAllBytes(Path.Combine(path, Path.GetFileName(entry.Path)), bytes);
    }
}
