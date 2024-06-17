using RGSSLibCS.V1;
using RGSSLibCS.V3;

namespace RGSSLibCS;

public abstract class ArchiveWriter
{
    public static void Encrypt(string directory, string outFile, ArchiveVersion version)
    {
        var outDir = Path.GetDirectoryName(outFile);
        if (!string.IsNullOrEmpty(outDir) && !Directory.Exists(outDir))
            throw new Exception($"Output directory '{outDir}' does not exist");

        using var writer = new BinaryWriter(File.Create(outFile));
        
        AbstractArchiveWriter aw;
        switch (version)
        {
            case ArchiveVersion.V1:
                aw = new RGSSv1Writer(writer);
                break;
            case ArchiveVersion.V3:
                aw = new RGSSv3Writer(writer);
                break;
            default:
                throw new Exception($"Unknown archive version {version}");
        }
        
        aw.EncryptDirectory(directory);
    }
}
