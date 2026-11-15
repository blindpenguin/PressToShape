using System.Collections.Generic;

namespace PressToShape.CodeIndex.Cli;

/// <summary>The parsed command line. When <see cref="Error"/> is set, the other values must not be used.</summary>
internal sealed record CliOptions(string? Command, string? Argument, bool Json, string? Solution, bool Help, string? Error)
{
    public static CliOptions Parse(IReadOnlyList<string> args)
    {
        string? command = null;
        string? argument = null;
        string? solution = null;
        var json = false;
        var help = false;

        for (var i = 0; i < args.Count; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--help" or "-h":
                    help = true;
                    break;
                case "--json":
                    json = true;
                    break;
                case "--solution":
                    if (i + 1 >= args.Count)
                    {
                        return Failed("The option --solution needs a path.");
                    }

                    solution = args[++i];
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        return Failed($"Unknown option: {arg}");
                    }

                    if (command is null)
                    {
                        command = arg;
                    }
                    else if (argument is null)
                    {
                        argument = arg;
                    }
                    else
                    {
                        return Failed($"Unexpected argument: {arg}");
                    }

                    break;
            }
        }

        return new CliOptions(command, argument, json, solution, help, null);
    }

    private static CliOptions Failed(string error) => new(null, null, false, null, false, error);
}
