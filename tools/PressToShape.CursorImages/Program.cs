using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace PressToShape.CursorImages;

/// <summary>
/// Developer tool that renders the canvas cursor images from the bundled Lucide icons. <c>generate</c> writes the PNGs,
/// <c>check</c> compares them pixel for pixel with the committed ones and exits with 1 if one is missing or different.
/// </summary>
internal static class Program
{
    private const string Usage =
        "Usage: PressToShape.CursorImages (generate | check) [--source <icon folder>] [--output <image folder>]";

    private static int Main(string[] args) => Run(args, Console.Out, Console.Error);

    /// <summary>Runs a command and returns the exit code: 0 on success, 1 on any failure.</summary>
    internal static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (!TryParse(args, out var command, out var source, out var target, out var problem))
        {
            error.WriteLine(problem);
            error.WriteLine(Usage);
            return 1;
        }

        try
        {
            source ??= Path.Combine(FindRoot(), "assets-src", "cursors", "lucide");
            target ??= Path.Combine(FindRoot(), "src", "PressToShape.App", "Assets", "Cursors");
            return command == "generate" ? Generate(source, target, output) : Check(source, target, output, error);
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            error.WriteLine(ex.Message);
            return 1;
        }
    }

    private static int Generate(string source, string target, TextWriter output)
    {
        Directory.CreateDirectory(target);
        var count = 0;
        foreach (var entry in CursorSet.Entries)
        {
            using var icon = SvgIcon.Load(Path.Combine(source, entry.Icon + ".svg"));
            foreach (var size in CursorSet.Sizes)
            {
                using var bitmap = CursorRenderer.Render(icon, size);
                ImageFiles.WriteAtomic(Path.Combine(target, ImageFiles.FileName(entry.Name, size)), ImageFiles.Encode(bitmap));
                count++;
            }
        }

        output.WriteLine($"Wrote {count} images to {target}.");
        return 0;
    }

    private static int Check(string source, string target, TextWriter output, TextWriter error)
    {
        var problems = new List<string>();
        foreach (var entry in CursorSet.Entries)
        {
            using var icon = SvgIcon.Load(Path.Combine(source, entry.Icon + ".svg"));
            foreach (var size in CursorSet.Sizes)
            {
                var name = ImageFiles.FileName(entry.Name, size);
                var file = Path.Combine(target, name);
                if (!File.Exists(file))
                {
                    problems.Add($"missing: {name}");
                    continue;
                }

                using var rendered = CursorRenderer.Render(icon, size);
                using var expected = ImageFiles.AsStored(rendered);
                using var committed = ImageFiles.Read(file, size);
                if (committed is null)
                {
                    problems.Add($"not a {size} px PNG: {name}");
                }
                else if (!ImageFiles.SamePixels(expected, committed))
                {
                    problems.Add($"different: {name}");
                }
            }
        }

        if (problems.Count == 0)
        {
            output.WriteLine("The cursor images are up to date.");
            return 0;
        }

        foreach (var problem in problems)
        {
            error.WriteLine(problem);
        }

        error.WriteLine("The cursor images are out of date. Run the generate command.");
        return 1;
    }

    private static bool TryParse(
        string[] args, out string command, out string? source, out string? target, out string problem)
    {
        command = string.Empty;
        source = null;
        target = null;
        problem = string.Empty;
        if (args.Length == 0 || (args[0] != "generate" && args[0] != "check"))
        {
            problem = "Missing or unknown command.";
            return false;
        }

        command = args[0];
        for (var i = 1; i < args.Length; i += 2)
        {
            if (i + 1 >= args.Length)
            {
                problem = $"The option {args[i]} needs a value.";
                return false;
            }

            switch (args[i])
            {
                case "--source":
                    source = args[i + 1];
                    break;
                case "--output":
                    target = args[i + 1];
                    break;
                default:
                    problem = $"Unknown option {args[i]}.";
                    return false;
            }
        }

        return true;
    }

    // The repository root is found from the tool's own location, so no absolute path is stored anywhere.
    private static string FindRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PressToShape.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("PressToShape.sln was not found above the tool. Pass --source and --output.");
    }
}
