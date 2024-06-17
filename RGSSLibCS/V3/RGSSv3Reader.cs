using System.Text;

namespace RGSSLibCS.V3;

public class RGSSv3Reader : AbstractArchiveReader
{
    public RGSSv3Reader(BinaryReader reader) : base(reader)
    {
    }

    protected override string ReadString(ref uint key)
    {
        var bytes = reader.ReadBytes(DecryptInt(reader.ReadInt32(), ref key));

        for (var i = 0; i < bytes.Length; ++i)
            bytes[i] ^= (byte)((key >> 8 * i) & 0xFF);

        return Encoding.UTF8.GetString(bytes);
    }

    protected override FileTable ReadTable()
    {
        var key = reader.ReadUInt32() * 9 + 3;

        FileTable table = new();
        for (;;)
        {
            var offset = ReadInt(ref key);
            if (offset == 0)
                break;

            TableEntry entry = new();
            entry.Offset = offset;
            entry.Size = ReadInt(ref key);
            entry.Key = (uint)ReadInt(ref key);
            entry.Path = ReadString(ref key);

            table.Add(entry);
        }

        return table;
    }

    protected override int DecryptInt(int value, ref uint key)
    {
        long val = value ^ key;

        return (int)val;
    }
}
