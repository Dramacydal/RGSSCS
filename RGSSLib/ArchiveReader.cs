﻿using System.Text;
using RGSSLib.V1;
using RGSSLib.V3;

namespace RGSSLib;

public static class ArchiveReader
{
    public static AbstractArchiveReader Open(string path)
    {
        var reader = new BinaryReader(File.OpenRead(path));

        return Open(reader);
    }

    public static AbstractArchiveReader Open(BinaryReader reader)
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
