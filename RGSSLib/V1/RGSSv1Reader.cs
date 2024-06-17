using System.Text;

namespace RGSSLib.V1;

public class RGSSv1Reader : AbstractArchiveReader
{
    private const uint KeyMagic = 0xDEADCAFE;
        
    public RGSSv1Reader(BinaryReader reader) : base(reader)
    {
    }
    
    protected override string ReadString(ref uint key)
    {
        var bytes = reader.ReadBytes(DecryptInt(reader.ReadInt32(), ref key));

        for (var i = 0; i < bytes.Length; ++i)
        {
            bytes[i] ^= (byte)(key & 0xFF);
            
            RotateKey(ref key);
        }

        return Encoding.UTF8.GetString(bytes);
    }

    protected override FileTable ReadTable()
    {
        var key = KeyMagic;

        FileTable table = new();
        for (; reader.BaseStream.Position != reader.BaseStream.Length;)
        {
            TableEntry entry = new();
            entry.Path = ReadString(ref key);
            entry.Size = ReadInt(ref key);
            entry.Offset = reader.BaseStream.Position;
            entry.Key = key;

            table.Add(entry);

            reader.BaseStream.Seek(entry.Size, SeekOrigin.Current);
        }

        return table;
    }

    protected override int DecryptInt(int value, ref uint key)
    {
        long val = value ^ key;

        RotateKey(ref key);

        return (int)val;
    }
}