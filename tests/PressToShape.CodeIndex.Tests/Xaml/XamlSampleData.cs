using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Tests;

/// <summary>A small solution with XAML, generated through the real generator, for the writer, reader and query tests.</summary>
internal static class XamlSampleData
{
    public const string AppSource = """
        namespace App.ViewModels
        {
            public class ShellViewModel
            {
                public string Title { get; } = "";
                public System.Windows.Input.ICommand SaveCommand { get; } = null!;
                public PanelViewModel Panel { get; } = new();
            }
            public class PanelViewModel { }
        }
        namespace App.Views
        {
            public partial class ShellView : Avalonia.Controls.UserControl
            {
                public void OnSaveClick(object? sender, System.EventArgs e) { }
            }
            public partial class PanelView : Avalonia.Controls.UserControl { }
            public class Startup { public Avalonia.Controls.Window Make() => new Avalonia.Controls.Window { DataContext = new App.ViewModels.ShellViewModel() }; }
        }
        namespace App
        {
            public class GestureConverter { }
            public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate
            {
                public object? Build(object? param)
                {
                    var name = param!.GetType().FullName!.Replace("ViewModel", "View", System.StringComparison.Ordinal);
                    var type = System.Type.GetType(name);
                    return type is null ? null : System.Activator.CreateInstance(type);
                }

                public bool Match(object? data) => true;
            }
        }
        """;

    public const string Application =
        "<Application xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:App\">\n"
        + "<Application.DataTemplates><local:ViewLocator /></Application.DataTemplates>\n"
        + "<Application.Resources><ResourceDictionary><ResourceDictionary.ThemeDictionaries>"
        + "<ResourceDictionary x:Key=\"Dark\"><SolidColorBrush x:Key=\"WindowBrush\" /></ResourceDictionary>"
        + "<ResourceDictionary x:Key=\"Light\"><SolidColorBrush x:Key=\"WindowBrush\" /></ResourceDictionary>"
        + "</ResourceDictionary.ThemeDictionaries><Thickness x:Key=\"PanelPadding\" /></ResourceDictionary></Application.Resources>\n"
        + "</Application>";

    public const string Shell =
        "<UserControl xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vm=\"using:App.ViewModels\" xmlns:local=\"using:App\" x:Class=\"App.Views.ShellView\" x:DataType=\"vm:ShellViewModel\">\n"
        + "<UserControl.Resources><local:GestureConverter x:Key=\"Gesture\" /><DataTemplate x:Key=\"RowTemplate\" DataType=\"vm:PanelViewModel\"><TextBlock /></DataTemplate></UserControl.Resources>\n"
        + "<UserControl.Styles><Style Selector=\"Button.icon\"><Setter Property=\"Padding\" Value=\"{StaticResource PanelPadding}\" /></Style></UserControl.Styles>\n"
        + "<StackPanel Background=\"{DynamicResource WindowBrush}\">\n"
        + "<TextBlock x:Name=\"TitleText\" Text=\"{Binding Title, Converter={StaticResource Gesture}}\" />\n"
        + "<Button Command=\"{Binding SaveCommand}\" Click=\"OnSaveClick\" />\n"
        + "<ContentControl Content=\"{Binding Panel}\" />\n"
        + "</StackPanel>\n"
        + "</UserControl>";

    public static InMemoryXamlFileSource Files() => new InMemoryXamlFileSource()
        .Add("src/App/App.axaml", Application)
        .Add("src/App/Views/ShellView.axaml", Shell);

    public static IndexData Data(bool reverse = false)
    {
        var app = TestCompilation.Project("App", XamlTestHost.Avalonia, AppSource);
        var lib = TestCompilation.Project("Lib", "namespace Lib { public class Marker { } }");
        var projects = reverse ? new[] { lib, app } : [app, lib];
        var source = new FakeProjects(projects);
        var result = new IndexGenerator(source, TestCompilation.RepoRoot, Files()).GenerateAsync(CancellationToken.None).GetAwaiter().GetResult();
        return result.Data;
    }

    public static InMemoryFileSink Index(bool reverse = false)
    {
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(Data(reverse), sink);
        return sink;
    }

    private sealed class FakeProjects(ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }
}
