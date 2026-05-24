# RGSSCS

A .NET solution for reading, writing, and managing RPG Maker RGSS archive files (`.rgssad`, `.rgss2a`, `.rgss3a`). It provides a reusable library, a desktop GUI, and a command-line tool.

## Projects

| Project | Description |
|---------|-------------|
| [RGSSLib](RGSSLib/README.md) | Core library — archive reading, writing, and encryption |
| [RGSSGui](RGSSGui/README.md) | Windows WPF desktop application |
| [RGSSCli](RGSSCli/README.md) | Cross-platform command-line tool |

## Supported Formats

| File extension | RPG Maker version | Format version |
|---------------|-------------------|----------------|
| `.rgssad`     | XP                | V1             |
| `.rgss2a`     | VX                | V1             |
| `.rgss3a`     | VX Ace            | V3             |

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later

## Building

```
dotnet build
```

To build a specific project in Release mode:

```
dotnet build RGSSLib/RGSSLib.csproj -c Release
dotnet build RGSSCli/RGSSCli.csproj -c Release
dotnet build RGSSGui/RGSSGui.csproj -c Release
```

## Quick Start

**Extract an archive via CLI:**
```
RGSSCli.exe extract Game.rgss3a --out ./extracted
```

**Create an archive via CLI:**
```
RGSSCli.exe encrypt ./extracted Game.rgss3a --version V3
```

**Use the library in your project:**
```csharp
using RGSSLib;

using var reader = ArchiveReader.Open("Game.rgss3a");
foreach (var entry in reader.Table)
    Console.WriteLine($"{entry.Path}  ({entry.Size} bytes)");

reader.ExtractAll("./output");
```

## License

See [LICENSE](LICENSE) for details.
