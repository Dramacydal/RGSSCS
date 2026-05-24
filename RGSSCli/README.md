# RGSSCli

Command-line tool for extracting, listing, and creating RPG Maker RGSS archives. Runs on any platform supported by .NET 10.

## Supported Formats

| File extension | RPG Maker version |
|---------------|-------------------|
| `.rgssad`     | XP                |
| `.rgss2a`     | VX                |
| `.rgss3a`     | VX Ace            |

## Usage

```
RGSSCli [command] [options]
```

Global options:

| Option | Description |
|--------|-------------|
| `--version` | Print tool version |
| `-h`, `-?`, `--help` | Show help |

---

## Commands

### `extract`

Extract all files from an archive to a directory.

```
RGSSCli extract <archive> [--out <directory>]
```

| Argument / Option | Description | Default |
|-------------------|-------------|---------|
| `<archive>` | Path to the `.rgssad`, `.rgss2a`, or `.rgss3a` file | required |
| `--out <directory>` | Destination directory (created if it does not exist) | `Extract` |

**Examples:**

```
# Extract to a specific folder
RGSSCli extract Game.rgss3a --out ./output

# Extract to default folder named "Extract"
RGSSCli extract Game.rgss3a
```

Output:
```
Extracted 1247 files to './output'
```

---

### `list`

Print the contents of an archive.

```
RGSSCli list <archive>
```

| Argument | Description |
|----------|-------------|
| `<archive>` | Path to the archive file |

**Example:**

```
RGSSCli list Game.rgss3a
```

Output:
```
Entries count: 1247, Version: V3
12840 Audio/BGM/theme.ogg
 4096 Graphics/Titles/title.png
...
```

Each line shows the unencrypted file size in bytes followed by the path inside the archive.

---

### `encrypt`

Create an archive from the contents of a directory.

```
RGSSCli encrypt <path> <outfile> [--version <V1|V3>]
```

| Argument / Option | Description | Default |
|-------------------|-------------|---------|
| `<path>` | Source directory | required |
| `<outfile>` | Output archive file path | required |
| `--version` | Archive version: `V1` or `V3` | `V3` |

Use `V3` for RPG Maker VX Ace (`.rgss3a`) and `V1` for XP / VX (`.rgssad` / `.rgss2a`).

**Examples:**

```
# Create a VX Ace archive
RGSSCli encrypt ./GameFiles Game.rgss3a --version V3

# Create an XP archive
RGSSCli encrypt ./GameFiles Game.rgssad --version V1
```

Output:
```
Total files encrypted: 1247
```

---

## Exit Codes

| Code | Meaning |
|------|---------|
| `0`  | Success |
| `1`  | Error (message printed to stdout) |

---

## Notes

- The `--out` path for `extract` defaults to a folder named `Extract` in the current working directory. The folder is created automatically if it does not exist, but its parent must already exist.
- The parent directory of `<outfile>` for `encrypt` must exist before running the command.
- All operations run synchronously; there is no progress output for large archives in the default configuration.
