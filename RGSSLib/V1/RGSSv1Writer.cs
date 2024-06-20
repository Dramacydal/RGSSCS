using System.Text;

namespace RGSSLib.V1;

public class RGSSv1Writer : AbstractArchiveWriter
{
    private const uint KeyMagic = 0xDEADCAFE;
    
    public RGSSv1Writer(BinaryWriter writer) : base(writer, ArchiveVersion.V1)
    {
    }

    protected override void EncryptFiles(IEnumerable<string> files, string basePath, ProgressDelegate? progress = null)
    {
        _aborted = false;
        
        var rnd = KeyMagic;

        var total = files.Count();
        var index = 0;
        foreach (var file in files)
        {
            if (_aborted)
                break;
            
            var entry = MakeEntry(file, basePath);

            WriteString(entry.Path, ref rnd);
            WriteInt(entry.Size, ref rnd);
            WriteFileContent(File.ReadAllBytes(file), rnd);

            progress?.Invoke(++index, total, entry.Path);
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
