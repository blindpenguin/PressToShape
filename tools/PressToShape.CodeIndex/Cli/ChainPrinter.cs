using System.IO;
using System.Text.Json;
using PressToShape.CodeIndex.Chain;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Cli;

/// <summary>Prints a chain as an indented tree (text) or as the JSON of the same tree. Links show [verified] or [inferred], and inferred links show their reason.</summary>
internal static class ChainPrinter
{
    /// <returns>True when the chain was found. Otherwise the candidates or the message were printed.</returns>
    public static bool Print(ChainResult result, TextWriter output, bool json)
    {
        if (result.Status == ChainStatus.Found)
        {
            if (json)
            {
                output.Write(JsonSerializer.Serialize(result.Root, JsonIndexWriter.Options) + "\n");
            }
            else
            {
                PrintNode(result.Root!, 0, output, isRoot: true);
            }

            return true;
        }

        if (json)
        {
            output.Write(JsonSerializer.Serialize(new { Status = result.Status.ToString(), result.Message, result.Candidates }, JsonIndexWriter.Options) + "\n");
            return false;
        }

        output.Write(result.Message + "\n");
        foreach (var candidate in result.Candidates)
        {
            output.Write($"{candidate.Id} [{candidate.SymbolType}] {candidate.Source}\n");
        }

        return false;
    }

    private static void PrintNode(ChainNode node, int level, TextWriter output, bool isRoot)
    {
        var line = new System.Text.StringBuilder(new string(' ', level * 2)).Append(node.Label);
        if (node.Confidence is { } confidence)
        {
            line.Append(" [").Append(confidence == Confidence.Verified ? "verified" : "inferred").Append(']');
        }

        if (node.Location is not null)
        {
            line.Append("  ").Append(node.Location);
        }

        if (isRoot && node.Id is not null)
        {
            line.Append("  ").Append(node.Id);
        }

        if (node.Reason is not null)
        {
            line.Append(" - ").Append(node.Reason);
        }

        output.Write(line.Append('\n').ToString());
        foreach (var child in node.Children)
        {
            PrintNode(child, level + 1, output, isRoot: false);
        }
    }
}
