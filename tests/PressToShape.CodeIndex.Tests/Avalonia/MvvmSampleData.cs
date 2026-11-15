using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Tests;

/// <summary>
/// A small MVVM solution for the chain and page tests: a model project, an app project (views, view models, services, a control, converters, the application class)
/// and a test project. It is built through the real generator, with in-memory XAML.
/// </summary>
internal static class MvvmSampleData
{
    public const string ToolkitStubs = """
        namespace CommunityToolkit.Mvvm.Input
        {
            [System.AttributeUsage(System.AttributeTargets.Method)] public sealed class RelayCommandAttribute : System.Attribute { }
            public abstract class CommandBase : System.Windows.Input.ICommand
            {
                public event System.EventHandler? CanExecuteChanged;
                public bool CanExecute(object? parameter) => true;
                public void Execute(object? parameter) { }
            }
            public class RelayCommand(System.Action execute) : CommandBase { }
        }
        """;

    public const string CoreSource = """
        namespace Core
        {
            /// <summary>A drawing document.</summary>
            public class Document { public string Name { get; } = ""; }
        }
        """;

    public const string AppSource = """
        namespace App.Services
        {
            /// <summary>Stores files.</summary>
            public interface IFileStore { void Save(string path); }
            public class LocalFileStore : IFileStore { public void Save(string path) { } }
            public interface IClock { }
            public class SystemClock : IClock { public SystemClock(int offset) { } }
        }
        namespace App.ViewModels
        {
            /// <summary>The window's view model.</summary>
            public class ViewModelBase { }

            public partial class ShellViewModel : ViewModelBase
            {
                private readonly App.Services.IFileStore _store;

                public ShellViewModel(App.Services.IFileStore store, App.Services.IClock clock, System.Action apply)
                {
                    _store = store;
                    Document = new Core.Document();
                    Panel = new PanelViewModel();
                    Detail = new DetailViewModel();
                    ZoomCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ZoomIn);
                    ApplyCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(apply);
                }

                public string Title { get; } = "";

                public Core.Document Document { get; }

                public PanelViewModel Panel { get; }

                public DetailViewModel Detail { get; }

                public System.Windows.Input.ICommand ZoomCommand { get; }

                public System.Windows.Input.ICommand ApplyCommand { get; }

                [CommunityToolkit.Mvvm.Input.RelayCommand]
                private void Save() { _store.Save(Document.Name); }

                private void ZoomIn() { }
            }

            public class PanelViewModel : ViewModelBase { }

            public class DetailViewModel : ViewModelBase { }

            public class OrphanViewModel : ViewModelBase { }
        }
        namespace App.Views
        {
            public partial class ShellView : Avalonia.Controls.UserControl { }
            public partial class PanelView : Avalonia.Controls.UserControl { }
            public partial class DetailView : Avalonia.Controls.UserControl { }
        }
        namespace App.Controls
        {
            /// <summary>A ruler along the canvas.</summary>
            public class Ruler : Avalonia.Controls.Control
            {
                public static readonly Avalonia.StyledProperty<double> ZoomProperty = Avalonia.AvaloniaProperty.Register<Ruler, double>(nameof(Zoom), 100);

                public double Zoom { get; set; }
            }
        }
        namespace App.Converters
        {
            /// <summary>Formats flags.</summary>
            public class FlagConverter : Avalonia.Data.Converters.IValueConverter
            {
                public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => value;
            }

            public class UnusedConverter : Avalonia.Data.Converters.IValueConverter
            {
                public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => value;
            }
        }
        namespace App
        {
            public class MainWindow { }

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

            public class MyApp : Avalonia.Application
            {
                public override void Initialize()
                {
                    var clock = new App.Services.SystemClock(0);
                }

                public override void OnFrameworkInitializationCompleted()
                {
                    var store = new App.Services.LocalFileStore();
                    if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var window = new MainWindow();
                        var shell = new App.ViewModels.ShellViewModel(store, new App.Services.SystemClock(1), () => { });
                    }
                    else if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ISingleViewApplicationLifetime single)
                    {
                        var other = new App.ViewModels.PanelViewModel();
                    }
                }
            }
        }
        """;

    public const string TestSource = """
        namespace Xunit { [System.AttributeUsage(System.AttributeTargets.Method)] public sealed class FactAttribute : System.Attribute { } }
        namespace App.Tests
        {
            public class ShellViewModelTests
            {
                [Xunit.Fact]
                public void Title_IsEmpty()
                {
                    var vm = new App.ViewModels.ShellViewModel(new App.Services.LocalFileStore(), new App.Services.SystemClock(0), () => { });
                    _ = vm.Title;
                }
            }

            public class StoreTests
            {
                [Xunit.Fact]
                public void Save_Works()
                {
                    App.Services.IFileStore store = new App.Services.LocalFileStore();
                    store.Save("a");
                }
            }

            public class NotATestClass
            {
                public void Helper() { var vm = new App.ViewModels.PanelViewModel(); }
            }

            public class FakeClockUser
            {
                public FakeClockUser(App.Services.IClock clock) { }
            }

            public class FakeStore : App.Services.IFileStore
            {
                public void Save(string path) { }
            }

            public static class ClockFactory
            {
                public static App.Services.IClock Create() => new App.Services.SystemClock(1);
            }
        }
        """;

    private static readonly string Header = "xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

    public static InMemoryXamlFileSource Files() => new InMemoryXamlFileSource()
        .Add("src/App/App.axaml",
            $"<Application {Header} xmlns:local=\"using:App\">\n"
            + "<Application.DataTemplates><local:ViewLocator /></Application.DataTemplates>\n"
            + "<Application.Resources><ResourceDictionary><ResourceDictionary.ThemeDictionaries>"
            + "<ResourceDictionary x:Key=\"Dark\"><SolidColorBrush x:Key=\"WindowBrush\" /><SolidColorBrush x:Key=\"UnusedBrush\" /></ResourceDictionary>"
            + "<ResourceDictionary x:Key=\"Light\"><SolidColorBrush x:Key=\"WindowBrush\" /><SolidColorBrush x:Key=\"UnusedBrush\" /></ResourceDictionary>"
            + "</ResourceDictionary.ThemeDictionaries><Thickness x:Key=\"PanelPadding\" /></ResourceDictionary></Application.Resources>\n"
            + "</Application>")
        .Add("src/App/Views/ShellView.axaml",
            $"<UserControl {Header} xmlns:vm=\"using:App.ViewModels\" xmlns:ctl=\"using:App.Controls\" xmlns:conv=\"using:App.Converters\" xmlns:views=\"using:App.Views\" x:Class=\"App.Views.ShellView\" x:DataType=\"vm:ShellViewModel\">\n"
            + "<UserControl.Resources><conv:FlagConverter x:Key=\"Flag\" /></UserControl.Resources>\n"
            + "<StackPanel Background=\"{DynamicResource WindowBrush}\" Margin=\"{StaticResource PanelPadding}\">\n"
            + "<TextBlock Text=\"{Binding Title, Converter={StaticResource Flag}}\" Foreground=\"{DynamicResource SystemAccentColor}\" />\n"
            + "<Button Command=\"{Binding SaveCommand}\" />\n"
            + "<Button Command=\"{Binding ZoomCommand}\" />\n"
            + "<Button Command=\"{Binding ApplyCommand}\" />\n"
            + "<ctl:Ruler />\n"
            + "<views:PanelView DataContext=\"{Binding Panel}\" />\n"
            + "<ContentControl Content=\"{Binding Detail}\" />\n"
            + "</StackPanel>\n"
            + "</UserControl>")
        .Add("src/App/Views/PanelView.axaml",
            $"<UserControl {Header} xmlns:vm=\"using:App.ViewModels\" x:Class=\"App.Views.PanelView\" x:DataType=\"vm:PanelViewModel\">\n<TextBlock />\n</UserControl>")
        .Add("src/App/Views/DetailView.axaml",
            $"<UserControl {Header} xmlns:vm=\"using:App.ViewModels\" x:Class=\"App.Views.DetailView\" x:DataType=\"vm:DetailViewModel\">\n<TextBlock />\n</UserControl>");

    public static ProjectCompilation[] Projects()
    {
        var core = TestCompilation.Project("Core", CoreSource);

        var app = XamlTestHost.AppProject("App", ToolkitStubs, AppSource);
        app = app with { Compilation = app.Compilation.AddReferences(core.Compilation.ToMetadataReference()) };
        app = TestCompilation.WithGenerator(app, new FixedSourceGenerator("Gen.g.cs",
            "namespace App.ViewModels { partial class ShellViewModel { public System.Windows.Input.ICommand SaveCommand { get; } = null!; } }"));

        var tests = XamlTestHost.AppProject("App.Tests", TestSource);
        tests = tests with { Compilation = tests.Compilation.AddReferences(core.Compilation.ToMetadataReference(), app.Compilation.ToMetadataReference()) };

        return [core, app, tests];
    }

    public static GenerateResult Generate(bool reverse = false)
    {
        var projects = Projects();
        if (reverse)
        {
            projects = [.. projects.Reverse()];
        }

        return new IndexGenerator(new FakeProjects(projects), TestCompilation.RepoRoot, Files()).GenerateAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public static IndexData Data(bool reverse = false) => Generate(reverse).Data;

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
