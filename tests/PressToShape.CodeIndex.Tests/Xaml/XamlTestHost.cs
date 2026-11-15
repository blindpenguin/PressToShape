using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

/// <summary>Builds a small solution in memory: stub Avalonia types, application C# source, XAML files, and the resolver over them.</summary>
internal sealed class XamlTestHost
{
    public const string Header = "xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\"";

    public const string Avalonia = """
        [assembly: Avalonia.Metadata.XmlnsDefinition("https://github.com/avaloniaui", "Avalonia.Controls")]
        [assembly: Avalonia.Metadata.XmlnsDefinition("https://github.com/avaloniaui", "Avalonia.Controls.Templates")]
        [assembly: Avalonia.Metadata.XmlnsDefinition("https://github.com/avaloniaui", "Avalonia.Controls.Presenters")]
        namespace Avalonia.Metadata
        {
            [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
            public sealed class XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace) : System.Attribute { }
        }
        namespace Avalonia.Controls.Templates
        {
            public interface IDataTemplate { object? Build(object? param); bool Match(object? data); }
            public class DataTemplate { }
        }
        namespace Avalonia.Data.Converters
        {
            public interface IValueConverter { object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture); }
            public interface IMultiValueConverter { object? Convert(System.Collections.Generic.IList<object?> values, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture); }
        }
        namespace Avalonia.Controls.ApplicationLifetimes
        {
            public interface IApplicationLifetime { }
            public interface IClassicDesktopStyleApplicationLifetime : IApplicationLifetime { }
            public interface ISingleViewApplicationLifetime : IApplicationLifetime { }
            public interface IActivityApplicationLifetime : IApplicationLifetime { }
        }
        namespace Avalonia
        {
            public class Application { public Avalonia.Controls.ApplicationLifetimes.IApplicationLifetime? ApplicationLifetime { get; } public virtual void Initialize() { } public virtual void OnFrameworkInitializationCompleted() { } }
            public abstract class AvaloniaProperty { public static StyledProperty<TValue> Register<TOwner, TValue>(string name, TValue defaultValue = default!) => new(); public static AttachedProperty<TValue> RegisterAttached<TOwner, THost, TValue>(string name, TValue defaultValue = default!) => new(); public static DirectProperty<TOwner, TValue> RegisterDirect<TOwner, TValue>(string name, System.Func<TOwner, TValue> getter) => new(); }
            public class StyledProperty<TValue> : AvaloniaProperty { }
            public class AttachedProperty<TValue> : AvaloniaProperty { }
            public class DirectProperty<TOwner, TValue> : AvaloniaProperty { }
        }
        namespace Avalonia.Interactivity
        {
            [System.Flags] public enum RoutingStrategies { Direct = 1, Tunnel = 2, Bubble = 4 }
            public class RoutedEventArgs : System.EventArgs { }
            public class RoutedEvent { public static RoutedEvent<TArgs> Register<TOwner, TArgs>(string name, RoutingStrategies routingStrategies) where TArgs : RoutedEventArgs => new(); }
            public class RoutedEvent<TArgs> : RoutedEvent where TArgs : RoutedEventArgs { }
        }
        namespace Avalonia.Controls.Presenters { public class ContentPresenter : Avalonia.Controls.Control { public object? Content { get; set; } } }
        namespace Avalonia.Controls
        {
            public class Control { public object? DataContext { get; set; } public event System.EventHandler? Loaded; }
            public class ContentControl : Control { public object? Content { get; set; } public object? ContentTemplate { get; set; } }
            public class ItemsControl : Control { public System.Collections.IEnumerable? ItemsSource { get; set; } public object? ItemTemplate { get; set; } }
            public class UserControl : ContentControl { }
            public class Window : ContentControl { }
            public class Button : ContentControl { public event System.EventHandler? Click; public System.Windows.Input.ICommand? Command { get; set; } }
            public class ToggleButton : Button { public bool IsChecked { get; set; } }
            public class TextBlock : Control { public string? Text { get; set; } }
            public class Grid : Control { }
            public class StackPanel : Control { }
            public class DockPanel : Control { }
            public class Application { }
            public class Style { }
            public class Setter { }
            public class ControlTemplate { }
            public class ResourceDictionary { }
            public class ResourceInclude { }
            public class StyleInclude { }
            public class Styles { }
            public class SolidColorBrush { }
            public class Thickness { }
        }
        """;

    private XamlTestHost(Dictionary<string, XamlDocument> documents, XamlWorld world, XamlResolver resolver, ProjectCompilation project, ViewLocatorConvention? convention)
    {
        Documents = documents;
        World = world;
        Resolver = resolver;
        Project = project;
        Convention = convention;
    }

    public Dictionary<string, XamlDocument> Documents { get; }

    public XamlWorld World { get; }

    public XamlResolver Resolver { get; }

    public ProjectCompilation Project { get; }

    public ViewLocatorConvention? Convention { get; }

    /// <summary>Creates the host. XAML files are given as (path, text). A file with a root <c>Application</c> is the application document.</summary>
    public static XamlTestHost Create(string appSource, params (string Path, string Xml)[] files)
    {
        var fake = TestCompilation.Project("Fake.Avalonia", Avalonia);
        var project = TestCompilation.Project("App", appSource);
        project = project with { Compilation = project.Compilation.AddReferences(fake.Compilation.ToMetadataReference()) };

        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        var documents = new Dictionary<string, XamlDocument>();
        foreach (var (path, xml) in files)
        {
            var read = XamlReader.Read(path, "App", xml);
            Assert.Null(read.Problem);
            documents[path] = read.Document!;
        }

        var world = new XamlWorld(documents.Values, new Dictionary<string, string> { ["App"] = "src/App" });
        var application = documents.Values.FirstOrDefault(d => XamlNames.IsType(d.Root, "Application"));
        ViewLocatorConvention? convention = null;
        if (application is not null)
        {
            convention = ViewLocatorConvention.Detect(application, new XamlTypeResolver(project.Compilation, known), project.Compilation, known).Convention;
        }

        return new XamlTestHost(documents, world, new XamlResolver(world, project.Compilation, known, convention), project, convention);
    }

    /// <summary>An application project that references the stub Avalonia types as metadata, so that they are not indexed as part of the solution.</summary>
    public static ProjectCompilation AppProject(string name, params string[] sources)
    {
        var fake = TestCompilation.Project("Fake.Avalonia", Avalonia);
        var project = TestCompilation.Project(name, sources);
        return project with { Compilation = project.Compilation.AddReferences(fake.Compilation.ToMetadataReference()) };
    }

    public XamlResolution Resolve(string path) => Resolver.Resolve(Documents[path]);

    public static string Edge(RelationshipRecord edge) => $"{edge.Type}:{edge.FromId}->{edge.ToId} [{edge.Confidence}]";
}
