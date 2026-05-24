# RGSSLib

Core library for reading and writing RPG Maker RGSS archives. It handles decryption and encryption for all supported archive versions.

## Supported Formats

| File extension | RPG Maker version | Format version |
|---------------|-------------------|----------------|
| `.rgssad`     | XP                | V1             |
| `.rgss2a`     | VX                | V1             |
| `.rgss3a`     | VX Ace            | V3             |

## Installation

Reference the project directly or build it and reference the resulting DLL:

```xml
<ProjectReference Include="..\RGSSLib\RGSSLib.csproj" />
```

## Reading Archives

### Open an archive

```csharp
using RGSSLib;

using var reader = ArchiveReader.Open("Game.rgss3a");
// or from a BinaryReader:
using var br = new BinaryReader(File.OpenRead("Game.rgss3a"));
using var reader = ArchiveReader.Open(br);
```

`ArchiveReader.Open` detects the format version automatically from the file header.

### Browse the file table

```csharp
Console.WriteLine($"Version: {reader.Version}");   // V1 or V3
Console.WriteLine($"Files:   {reader.Table.Size}");

foreach (var entry in reader.Table)
    Console.WriteLine($"{entry.Path}  ({entry.Size} bytes)");
```

### Read a single file

```csharp
// As a byte span:
ReadOnlySpan<byte> data = reader.GetFileContent("Audio/BGM/theme.ogg");

// As a MemoryStream:
using var stream = reader.GetFileStream("Graphics/Titles/title.png");
```

You can also use a `TableEntry` directly:

```csharp
var entry = reader.Table.GetEntry("Graphics/Titles/title.png");
if (entry != null)
{
    var data = reader.GetFileContent(entry);
    var stream = reader.GetFileStream(entry);
}
```

### Extract files

Extract a single file (preserving directory structure under `destDir`):

```csharp
var entry = reader.Table.GetEntry("Graphics/Titles/title.png")!;
reader.Extract(entry, "./output");
// writes to ./output/Graphics/Titles/title.png
```

Extract all files with an optional progress callback:

```csharp
reader.ExtractAll("./output", (current, total, path) =>
{
    Console.WriteLine($"[{current}/{total}] {path}");
});
```

Cancel an in-progress extraction from another thread:

```csharp
reader.Abort();
```

## Writing Archives

### Create an archive from a directory

```csharp
using RGSSLib;

ArchiveWriter.Encrypt(
    directory:   "./GameFiles",
    outFile:     "Game.rgss3a",
    version:     ArchiveVersion.V3,
    progress:    (current, total, path) => Console.WriteLine($"[{current}/{total}] {path}"),
    excludeFile: null   // optional: skip a file by absolute path
);
```

The method returns the number of files written. Pass `ArchiveVersion.V1` to produce a V1 archive (`.rgssad` / `.rgss2a`).

## API Reference

### `ArchiveVersion`

```csharp
public enum ArchiveVersion
{
    V1 = 1,  // RPG Maker XP / VX
    V3 = 3   // RPG Maker VX Ace
}
```

### `ArchiveReader` (static)

| Member | Description |
|--------|-------------|
| `Open(string path)` | Opens an archive from a file path |
| `Open(BinaryReader reader)` | Opens an archive from a `BinaryReader` |
| `string HeaderMagic` | File magic constant: `"RGSSAD"` |

### `AbstractArchiveReader`

| Member | Description |
|--------|-------------|
| `FileTable Table` | Lazily-loaded file index |
| `ArchiveVersion Version` | Detected archive version |
| `GetFileContent(string path)` | Returns decrypted file bytes |
| `GetFileContent(TableEntry entry)` | Returns decrypted file bytes |
| `GetFileStream(string path)` | Returns decrypted file as `MemoryStream` |
| `GetFileStream(TableEntry entry)` | Returns decrypted file as `MemoryStream` |
| `Extract(TableEntry entry, string destDir)` | Extracts one file to `destDir` |
| `ExtractAll(string destDir, ProgressDelegate? progress)` | Extracts all files |
| `Abort()` | Signals an in-progress extraction to stop |
| `Dispose()` | Releases underlying stream |

### `ArchiveWriter` (static)

| Member | Description |
|--------|-------------|
| `Encrypt(string directory, string outFile, ArchiveVersion version, ProgressDelegate? progress, string? excludeFile)` | Creates an archive from a directory; returns the file count |

### `FileTable`

| Member | Description |
|--------|-------------|
| `int Size` | Number of entries |
| `GetEntry(string path)` | Returns `TableEntry` or `null` |
| `HasEntry(string path)` | Returns `true` if the path exists |
| `GetEntriesInPath(string path)` | Returns all entries under a directory |
| `Add(TableEntry entry)` | Adds or replaces an entry |
| `NormalizePath(string path)` | Converts to lowercase with backslash separators |
| Implements `IEnumerable<TableEntry>` | Enumerate all entries |

### `TableEntry`

| Property | Type | Description |
|----------|------|-------------|
| `Path` | `string` | Relative file path inside the archive |
| `Size` | `int` | Unencrypted file size in bytes |
| `Offset` | `long` | Position in archive (V3 only) |
| `Key` | `uint` | Per-file encryption key (V3 only) |

### `ProgressDelegate`

```csharp
delegate void ProgressDelegate(int current, int total, string path);
```

## Archive Format Details

### V1 (XP / VX)

```
Magic:   "RGSSAD\0" + 0x01
Key:     0xDEADCAFE (shared for the entire file)

Per file:
  [encrypted int]   name length
  [encrypted bytes] name (UTF-8)
  [encrypted int]   data length
  [encrypted bytes] file data

Key rotation after each field: key = key * 7 + 3
Byte encryption: byte ^ (key >> shift & 0xFF)
```

### V3 (VX Ace)

```
Magic:   "RGSSAD\0" + 0x03
Seed:    [4 bytes] random value
Key:     seed * 9 + 3

Index block (one record per file):
  [encrypted int]   data offset
  [encrypted int]   data size
  [encrypted int]   per-file key
  [encrypted bytes] name (UTF-8)
End marker: offset = 0

Data block:
  Each file encrypted independently with its own key
  (no shared key rotation between files)
```
