using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Cli;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class CliXamlTests
{
    private sealed class MemoryFolder : IIndexFolder
    {
        private readonly SortedDictionary<string, byte[]> _files = new(StringComparer.Ordinal);
        private readonly HashSet<string> _written = [];

        public void Write(string relativePath, byte[] content)
        {
            _files[relativePath] = content;
            _written.Add(relativePath);
        }

        public byte[]? Read(string relativePath) => _files.GetValueOrDefault(relativePath);

        public IReadOnlyList<string> ListFiles() => _files.Keys.ToList();

        public IReadOnlyList<string> RemoveStale()
        {
            var stale = _files.Keys.Where(k => !_written.Contains(k)).ToList();
            stale.ForEach(k => _files.Remove(k));
            return stale;
        }
    }

    private sealed class FakeSource(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private static async Task<(StringWriter Out, StringWriter Err, MemoryFolder Folder, Func<string[], Task<int>> Run)> Start(InMemoryXamlFileSource? xaml = null)
    {
        var folder = new MemoryFolder();
        var output = new StringWriter();
        var error = new StringWriter();
        var services = new CliServices(
            _ => TestCompilation.RepoRoot,
            (_, _) => new FakeSource(TestCompilation.Project("App", XamlTestHost.Avalonia, XamlSampleData.AppSource)),
            _ => folder,
            _ => xaml ?? XamlSampleData.Files());
        Task<int> Run(string[] args) => new CliApp(output, error, TestCompilation.RepoRoot, services).RunAsync(args, CancellationToken.None);

        Assert.Equal(CliApp.Success, await Run(["generate"]));
        output.GetStringBuilder().Clear();
        error.GetStringBuilder().Clear();
        return (output, error, folder, Run);
    }

    [Fact]
    public async Task Show_ViewClass_PrintsItsXamlFileBindingsAndHandlers()
    {
        var (output, _, _, run) = await Start();

        var code = await run(["show", "ShellView"]);

        var text = output.ToString();
        Assert.Equal(CliApp.Success, code);
        Assert.Contains("T:App.Views.ShellView [Class]", text, StringComparison.Ordinal);
        Assert.Contains("XAML: src/App/Views/ShellView.axaml [View] X:src/App/Views/ShellView.axaml", text, StringComparison.Ordinal);
        Assert.Contains("root UserControl; class App.Views.ShellView [T:App.Views.ShellView]; data type vm:ShellViewModel [T:App.ViewModels.ShellViewModel]", text, StringComparison.Ordinal);
        Assert.Contains("Bindings (3):", text, StringComparison.Ordinal);
        Assert.Contains("Text = Binding 'Title' -> P:App.ViewModels.ShellViewModel.Title (verified), converter StaticResource Gesture", text, StringComparison.Ordinal);
        Assert.Contains("Command = Binding 'SaveCommand' -> P:App.ViewModels.ShellViewModel.SaveCommand (verified), command", text, StringComparison.Ordinal);
        Assert.Contains("Event handlers (1):", text, StringComparison.Ordinal);
        Assert.Contains("Click = OnSaveClick -> M:App.Views.ShellView.OnSaveClick(System.Object,System.EventArgs) (verified)", text, StringComparison.Ordinal);
        Assert.Contains("Resource references (", text, StringComparison.Ordinal);
        Assert.Contains("Resources defined (", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Show_ViewClass_PrintsTheEvidenceOfItsViewModelLinks()
    {
        var (output, _, _, run) = await Start();

        await run(["show", "ShellView"]);

        var text = output.ToString();
        Assert.Contains("UsesViewModel [verified] ShellViewModel src/App/Views/ShellView.axaml:1 T:App.ViewModels.ShellViewModel - x:DataType", text, StringComparison.Ordinal);
        Assert.Contains("CodeBehind [verified]", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Show_Element_PrintsItsOwnRecordsOnly()
    {
        var (output, _, _, run) = await Start();

        await run(["show", "TitleText"]);

        var text = output.ToString();
        Assert.Contains("[Element]", text, StringComparison.Ordinal);
        Assert.Contains("element TextBlock 'TitleText' line 5", text, StringComparison.Ordinal);
        Assert.Contains("Binding 'Title'", text, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveCommand", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Show_ResourceAndStyleAndTemplate_PrintTheirRecords()
    {
        var (output, _, _, run) = await Start();

        await run(["show", "Gesture"]);
        await run(["show", "Button.icon"]);
        await run(["show", "X:src/App/Views/ShellView.axaml#/UserControl/UserControl.Resources[1]/DataTemplate[1]"]);

        var text = output.ToString();
        Assert.Contains("resource 'Gesture' (local:GestureConverter) line 2 [T:App.GestureConverter]", text, StringComparison.Ordinal);
        Assert.Contains("style 'Button.icon' line 3; sets Padding; uses PanelPadding", text, StringComparison.Ordinal);
        Assert.Contains("DataTemplate RowTemplate line 2; target vm:PanelViewModel [T:App.ViewModels.PanelViewModel]", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Show_Json_ContainsTheXamlFileAndIsValid()
    {
        var (output, _, _, run) = await Start();

        await run(["show", "ShellView", "--json"]);

        using var document = JsonDocument.Parse(output.ToString());
        var xaml = document.RootElement.GetProperty("Xaml");
        Assert.Equal("View", xaml.GetProperty("EntityType").GetString());
        Assert.Equal(3, xaml.GetProperty("File").GetProperty("Bindings").GetArrayLength());
        Assert.Equal("T:App.Views.ShellView", document.RootElement.GetProperty("Symbol").GetProperty("Id").GetString());
    }

    [Fact]
    public async Task Find_PrintsXamlEntriesWithTheirTypes()
    {
        var (output, _, _, run) = await Start();

        await run(["find", "ShellView"]);

        var lines = output.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Contains(lines, l => l.StartsWith("ShellView [View] src/App/Views/ShellView.axaml:1 X:src/App/Views/ShellView.axaml", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.StartsWith("ShellView [Class]", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Refs_ViewModelProperty_PrintsTheBindingWithFileLineAndConfidence()
    {
        var (output, _, _, run) = await Start();

        await run(["refs", "P:App.ViewModels.ShellViewModel.Title"]);

        Assert.Contains("ShellView.axaml TextBlock[1] @Text [BindsTo, verified] src/App/Views/ShellView.axaml:5", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Refs_ViewModelOfAnInferredLink_PrintsTheReasonAndTheInferredMark()
    {
        var (output, _, _, run) = await Start();

        await run(["refs", "PanelViewModel"]);

        var line = output.ToString().Split('\n').Single(l => l.Contains("[UsesViewModel, inferred]", StringComparison.Ordinal));
        Assert.Contains("- ViewLocator (registered in Application.DataTemplates at src/App/App.axaml:", line, StringComparison.Ordinal);
        Assert.Contains("'ViewModel' is replaced by 'View'", line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Refs_Json_IsAValidArrayWithConfidence()
    {
        var (output, _, _, run) = await Start();

        await run(["refs", "ShellViewModel", "--json"]);

        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Contains(document.RootElement.EnumerateArray(), h => h.GetProperty("RelationType").GetString() == "UsesViewModel" && h.GetProperty("Confidence").GetString() == "Verified");
    }

    [Fact]
    public async Task Generate_PrintsXamlFileCountAndFindingsAsWarnings()
    {
        var folder = new MemoryFolder();
        var output = new StringWriter();
        var error = new StringWriter();
        var xaml = XamlSampleData.Files().Add("src/App/Views/ShellView.axaml", XamlSampleData.Shell.Replace("{Binding Title,", "{Binding Titel,", StringComparison.Ordinal));
        var services = new CliServices(
            _ => TestCompilation.RepoRoot,
            (_, _) => new FakeSource(TestCompilation.Project("App", XamlTestHost.Avalonia, XamlSampleData.AppSource)),
            _ => folder,
            _ => xaml);

        var code = await new CliApp(output, error, TestCompilation.RepoRoot, services).RunAsync(["generate"], CancellationToken.None);

        Assert.Equal(CliApp.Success, code);
        Assert.Contains("and 2 XAML files into docs/index.", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("Finding: src/App/Views/ShellView.axaml: Binding 'Titel' at line 5 does not resolve", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Generate_MalformedXaml_FailsAndWritesNothing()
    {
        var folder = new MemoryFolder();
        var error = new StringWriter();
        var xaml = XamlSampleData.Files().Add("src/App/Views/Broken.axaml", "<UserControl>\n<Grid>\n</UserControl>");
        var services = new CliServices(
            _ => TestCompilation.RepoRoot,
            (_, _) => new FakeSource(TestCompilation.Project("App", XamlTestHost.Avalonia, XamlSampleData.AppSource)),
            _ => folder,
            _ => xaml);

        var code = await new CliApp(new StringWriter(), error, TestCompilation.RepoRoot, services).RunAsync(["generate"], CancellationToken.None);

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("Error: App: src/App/Views/Broken.axaml:", error.ToString(), StringComparison.Ordinal);
        Assert.Empty(folder.ListFiles());
    }

    [Fact]
    public async Task Check_AfterGenerate_IncludesXamlFilesAndReportsUpToDate()
    {
        var (output, _, folder, run) = await Start();

        var code = await run(["check"]);

        Assert.Equal(CliApp.Success, code);
        Assert.Contains("up to date", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(folder.ListFiles(), f => f.StartsWith("xaml/App/", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Check_ChangedXamlSource_IsReportedAsChanged()
    {
        var (output, _, folder, _) = await Start();
        var changed = XamlSampleData.Files().Add("src/App/Views/ShellView.axaml", XamlSampleData.Shell.Replace("{Binding Panel}", "{Binding Title}", StringComparison.Ordinal));
        var services = new CliServices(
            _ => TestCompilation.RepoRoot,
            (_, _) => new FakeSource(TestCompilation.Project("App", XamlTestHost.Avalonia, XamlSampleData.AppSource)),
            _ => folder,
            _ => changed);

        var code = await new CliApp(output, new StringWriter(), TestCompilation.RepoRoot, services).RunAsync(["check"], CancellationToken.None);

        Assert.Equal(CliApp.Failure, code);
        Assert.Contains("changed: xaml/App/Views.ShellView.axaml.json", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Help_ListsTheXamlSymbolTypes()
    {
        var (output, _, _, run) = await Start();

        await run(["--help"]);

        foreach (var type in new[] { "View", "Element", "Resource", "Style", "Template" })
        {
            Assert.Contains(type, output.ToString(), StringComparison.Ordinal);
        }
    }
}
