using System.Text;

namespace RGSSLibCS.V3;

public class RGSSv3Writer : AbstractArchiveWriter
{
    private readonly Random _rand;

    public RGSSv3Writer(BinaryWriter writer) : base(writer, ArchiveVersion.V3)
    {
    }

    protected override void EncryptFiles(IEnumerable<string> files, string basePath)
    {
        var rand = new Random((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            
        var seed = (uint)rand.Next();

        writer.Write(seed);

        seed = seed * 9 + 3;

        List<Tuple<string, TableEntry>> entries = new();
        List<Tuple<long, TableEntry>> offsetOffsets = new();

        foreach (var file in files)
        {
            var entry = MakeEntry(file, basePath);

            offsetOffsets.Add(new(writer.BaseStream.Position, entry));
            WriteInt(0, ref seed);

            WriteInt(entry.Size, ref seed);
            entry.Key = (uint)rand.Next();
            WriteInt((int)entry.Key, ref seed);
            WriteString(entry.Path, ref seed);

            entries.Add(new(file, entry));
        }

        WriteInt(0, ref seed);

        var dataStartOffset = writer.BaseStream.Position;

        foreach (var (file, entry) in entries)
            WriteFileContent(File.ReadAllBytes(file), entry.Key);

        var dataOffset = 0;
        foreach (var (offset, entry) in offsetOffsets)
        {
            writer.BaseStream.Seek(offset, SeekOrigin.Begin);

            WriteInt((int)dataStartOffset + dataOffset, ref seed);
            dataOffset += entry.Size;
        }
    }

    protected override void WriteString(string value, ref uint key)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        
        writer.Write(EncryptInt(bytes.Length, ref key));

        for (var i = 0; i < bytes.Length; ++i)
            bytes[i] ^= (byte)((key >> 8 * i) & 0xFF);
        
        writer.Write(bytes);
    }

    protected override int EncryptInt(int value, ref uint key)
    {
        long val = value ^ key;

        return (int)val;
    }
}
