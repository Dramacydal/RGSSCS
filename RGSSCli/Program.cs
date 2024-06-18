using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using RGSSLib;

namespace RGSSCli;

internal static class Program
{
    public static readonly Argument<string> InFile = new(
        name: "archive",
        description: "Path to RGSS archive. *.rgssa, *.rgss2a, *.rgss3a  supported"
    );

    public static readonly Argument<string> OutFile = new(
        name: "outfile",
        description: "Output RGSS archive"
    );

    public static readonly Argument<string> InDirectory = new(
        name: "path",
        description: "Path to files that should be archived"
    );

    public static readonly Option<string> OutputPath = new(
        name: "--out",
        description: "Extraction path",
        isDefault: true,
        parseArgument: result => ValidateDirectory(result, "Extract", true)
    );

    public static readonly Option<ArchiveVersion> Version = new(
        name: "--version",
        description: "Archive version to produce",
        getDefaultValue: () => ArchiveVersion.V3
    );

    private static List<Tuple<Command, Action<InvocationContext>?>> _commandDefinitions = new()
    {
        new(new RootCommand("RGSSCli: decrypts, encrypts, extracts various RPG Maker archives"),
            null),
        new(
            new(
                name: "extract",
                description: "Extracts files from archive"
            )
            {
                InFile,
                OutputPath
            },
            context =>
            {
                var result = context.ParseResult;

                var file = result.GetValueForArgument(InFile);
                if (!File.Exists(file))
                {
                    context.Console.WriteLine($"Input file '{file}' does not exist");
                    return;
                }

                var outDir = result.GetValueForOption(OutputPath);
                if (!ParentDirectoryExists(outDir))
                {
                    context.Console.WriteLine($"Directory '{Path.GetDirectoryName(outDir)} does not exist");
                    return;
                }

                try
                {
                    var archive = ArchiveReader.Open(file);

                    if (archive.Table.Size > 0)
                    {
                        Directory.CreateDirectory(outDir);
                        archive.ExtractAll(outDir);

                        context.Console.WriteLine($"Extracted {archive.Table.Size} files to '{outDir}'");
                    }
                }
                catch (Exception e)
                {
                    context.Console.WriteLine($"Failed to extract: {e.Message}");
                }
            }
        ),
        new(
            new(
                name: "list",
                description: "Lists files in archive"
            )
            {
                InFile,
            },
            context =>
            {
                var result = context.ParseResult;

                var file = result.GetValueForArgument(InFile);
                if (!File.Exists(file))
                {
                    context.Console.WriteLine($"Input file '{file}' does not exist");
                    return;
                }

                try
                {
                    var archive = ArchiveReader.Open(file);

                    context.Console.WriteLine($"Entries count: {archive.Table.Size}, Version: {archive.Version}");
                    foreach (var entry in archive.Table)
                        context.Console.WriteLine($"{entry.Size} {entry.Path}");
                }
                catch (Exception e)
                {
                    context.Console.WriteLine($"Failed to list entries: {e.Message}");
                }
            }
        ),
        new(
            new(
                name: "encrypt",
                description: "Create archive from directory"
            )
            {
                InDirectory,
                OutFile,
                Version,
            },
            context =>
            {
                var result = context.ParseResult;

                var inDirectory = result.GetValueForArgument(InDirectory);
                if (!Directory.Exists(inDirectory))
                {
                    context.Console.WriteLine($"Directory '{inDirectory}' does not exist");
                    return;
                }

                var outFile = result.GetValueForArgument(OutFile);
                if (!ParentDirectoryExists(outFile))
                {
                    context.Console.WriteLine($"Directory '{Path.GetDirectoryName(outFile)}' does not exist");
                    return;
                }

                try
                {
                    var count = ArchiveWriter.Encrypt(inDirectory, outFile, result.GetValueForOption(Version));
                    Console.WriteLine($"Total files encrypted: {count}");
                }
                catch (Exception e)
                {
                    context.Console.WriteLine($"Encryption failed: {e.Message}");
                }
            }
        )
    };

    private static string ValidateDirectory(ArgumentResult result, string defaultDir = "", bool validateParent = false)
    {
        string path;
        if (result.Tokens.Count == 0)
        {
            if (!string.IsNullOrEmpty(defaultDir))
                path = defaultDir;
            else
            {
                result.ErrorMessage = FormatError("value must be specified", result.Argument);
                return "";
            }
        }
        else
            path = result.Tokens.Single().ToString();

        var checkDir = path;
        if (validateParent)
            checkDir = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(checkDir) && !Directory.Exists(checkDir))
        {
            result.ErrorMessage = FormatError($"directory '{checkDir}' does not exist", result.Argument);
            return "";
        }

        return path;
    }

    private static string FormatError(string message, Argument arg)
    {
        return $"Error in '{arg.Name}': {message}";
    }

    private static bool ParentDirectoryExists(string path)
    {
        var dir = Path.GetDirectoryName(path);
        return string.IsNullOrEmpty(dir) || Directory.Exists(dir);
    }

    public static int Main(string[] args)
    {
        Command commandTree = null;
        foreach (var command in _commandDefinitions)
        {
            if (command.Item2 != null)
                command.Item1.SetHandler(command.Item2);
            if (commandTree == null)
            {
                commandTree = command.Item1;
                continue;
            }

            commandTree.AddCommand(command.Item1);
        }

        var parser = new CommandLineBuilder(commandTree).UseDefaults().Build();

        try
        {
            return parser.Invoke(args);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
        }

        return 1;
    }
}
