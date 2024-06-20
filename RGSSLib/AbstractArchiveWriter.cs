using System.Text;

namespace RGSSLib;

public abstract class AbstractArchiveWriter
{
    protected readonly BinaryWriter writer;
    private readonly ArchiveVersion _version;
    protected bool _aborted;

    public delegate void ProgressDelegate(int index, int total, string path);
    
    protected AbstractArchiveWriter(BinaryWriter writer, ArchiveVersion version)
    {
        this.writer = writer;
        _version = version;
    }

    class PathComparer : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            var xFile = Path.GetFileName(x);
            var yFile = Path.GetFileName(y);

            var pX = Path.GetDirectoryName(x).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var pY = Path.GetDirectoryName(y).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            for (var i = 0; i < Math.Min(pX.Length, pY.Length); ++i)
            {
                var res = string.CompareOrdinal(pX[i], pY[i]);
                if (res != 0)
                    return res;
            }

            if (pX.Length != pY.Length)
                return pY.Length - pX.Length;

            return string.CompareOrdinal(xFile, yFile);
        }
    }

    public int EncryptDirectory(string path, ProgressDelegate? progress = null)
    {
        if (!Directory.Exists(path))
            throw new Exception($"Source path '{path}' does not exist");

        writer.Write(Encoding.ASCII.GetBytes(ArchiveReader.HeaderMagic));
        writer.Write((byte)0);
        writer.Write((byte)_version);
        
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).OrderBy(_ => _, new PathComparer()).ToArray();

        EncryptFiles(files, path, progress);

        return files.Length;
    }

    protected abstract void EncryptFiles(IEnumerable<string> files, string basePath, ProgressDelegate? progress = null);

    protected TableEntry MakeEntry(string filePath, string basePath)
    {
        return new()
        {
            Path = Path.GetRelativePath(basePath, filePath),
            Size = (int)new FileInfo(filePath).Length,
        };
    }

    protected abstract void WriteString(string value, ref uint key);

    protected void WriteInt(int value, ref uint key) => writer.Write(EncryptInt(value, ref key));

    protected abstract int EncryptInt(int value, ref uint key);

    protected void RotateKey(ref uint key) => key = key * 7 + 3;

    protected void WriteFileContent(byte[] bytes, uint key)
    {
        for (var i = 0; i < bytes.Length; ++i)
        {
            if ((i % 4) == 0 && i != 0)
                RotateKey(ref key);

            bytes[i] ^= (byte)((key >> 8 * (i % 4)) & 0xFF);
        }
        
        writer.Write(bytes);
    }

    public void Abort() => _aborted = true;
}
