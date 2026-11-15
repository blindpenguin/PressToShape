using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PressToShape.App.ViewModels;
using Xunit;

namespace PressToShape.App.Tests;

public class AppArchitectureTests
{
    private static readonly Assembly AppAssembly = typeof(PressToShapeApp).Assembly;

    [Theory]
    [InlineData("PressToShape.Desktop")]
    [InlineData("PressToShape.Android")]
    public void App_DoesNotReference_PlatformHeads(string head)
    {
        var references = AppAssembly.GetReferencedAssemblies().Select(a => a.Name);

        Assert.DoesNotContain(head, references);
    }

    // Every change of the document must go through EditorState (Edit, Preview and Commit, ReplaceDocument), or it would
    // bypass the undo history. An assignment like "state.Document = ..." is only allowed inside EditorState itself.
    private static readonly Regex DocumentAssignment = new(@"\.Document\s*=(?![=>])", RegexOptions.Compiled);

    private static IEnumerable<string> ProductionFiles() =>
        Directory.EnumerateFiles(RepoPaths.Combine("src/PressToShape.App"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

    [Fact]
    public void OnlyEditorState_AssignsTheDocument()
    {
        var offenders = ProductionFiles()
            .Where(f => Path.GetFileName(f) != "EditorState.cs")
            .Where(f => DocumentAssignment.IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(RepoPaths.Root, f))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "These files assign a Document directly and would bypass the undo history. Use EditorState.Edit, Preview and Commit, or ReplaceDocument: "
            + string.Join(", ", offenders));
    }

    [Theory]
    [InlineData("state.Document = state.Document.AddShape(id, shape);", true)]
    [InlineData("_state.Document = document;", true)]
    [InlineData("shell.State.Document=x;", true)]
    [InlineData("var same = state.Document == other;", false)]
    [InlineData("Assert.Equal(a, canvas.Document);", false)]
    [InlineData("public Document Document => _state.Document;", false)]
    [InlineData("Func<Document, Document> f = d => d;", false)]
    public void DocumentAssignmentPattern_FlagsAssignmentsOnly(string line, bool expected)
    {
        Assert.Equal(expected, DocumentAssignment.IsMatch(line));
    }
}
