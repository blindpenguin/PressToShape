using System.Text;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Output.Avalonia;

/// <summary>Small text helpers shared by the Avalonia pages.</summary>
internal static class PageText
{
    public const string NoDocumentation = "_(no documentation)_";

    public static string Mark(Confidence confidence) => confidence == Confidence.Verified ? "verified" : "inferred";

    public static string Summary(DocRecord doc) => doc.HasDocumentation && !string.IsNullOrWhiteSpace(doc.Summary) ? doc.Summary!.Trim() : NoDocumentation;

    public static string Summary(string? summary) => string.IsNullOrWhiteSpace(summary) ? NoDocumentation : summary.Trim();

    public static byte[] Encode(string text) => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text);
}
