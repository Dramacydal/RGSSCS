using System.Text;
using RGSSLib.V1;
using RGSSLib.V3;

namespace RGSSLib;

public abstract class ArchiveReader
{
    public static AbstractArchiveReader Decrypt(BinaryReader reader)
    {
        var headerMagic = reader.ReadBytes(6);
        if (Encoding.ASCII.GetString(headerMagic) != HeaderMagic)
            throw new Exception("Wrong header magic");
        
        // null termination
        reader.ReadByte();
        
        var version = (ArchiveVersion)reader.ReadByte();
        AbstractArchiveReader ar;
        switch (version)
        {
            case ArchiveVersion.V1:
                ar = new RGSSv1Reader(reader);
                break;
            case ArchiveVersion.V3:
                ar = new RGSSv3Reader(reader);
                break;
            default:
                throw new Exception($"Unknown archive version {version}");
        }

        return ar;
    }

    public static string HeaderMagic => "RGSSAD";
}
