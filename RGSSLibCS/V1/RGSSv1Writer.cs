using System.Text;

namespace RGSSLibCS.V1;

public class RGSSv1Writer : AbstractArchiveWriter
{
    private const uint KeyMagic = 0xDEADCAFE;
    
    public RGSSv1Writer(BinaryWriter writer) : base(writer, ArchiveVersion.V1)
    {
    }

    protected override void EncryptFiles(IEnumerable<string> files, string basePath)
    {
        var rnd = KeyMagic;

        List<Tuple<string, TableEntry>> entries = new();
        List<Tuple<long, TableEntry>> offsetOffsets = new();

        foreach (var file in files)
        {
            var entry = MakeEntry(file, basePath);

            WriteString(entry.Path, ref rnd);
            WriteInt(entry.Size, ref rnd);
            WriteFileContent(File.ReadAllBytes(file), rnd);
        }
    }

    protected override void WriteString(string value, ref uint key)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        
        writer.Write(EncryptInt(bytes.Length, ref key));

        for (var i = 0; i < bytes.Length; ++i)
        {
            bytes[i] ^= (byte)(key & 0xFF);
            RotateKey(ref key);
        }

        writer.Write(bytes);
    }

    protected override int EncryptInt(int value, ref uint key)
    {
        long val = value ^ key;

        RotateKey(ref key);

        return (int)val;
    }
}
