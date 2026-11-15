using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Xaml;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ViewLocatorConventionTests
{
    private const string Header = "xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:local=\"using:App\"";

    private const string Infrastructure = """
        namespace Avalonia.Controls.Templates { public interface IDataTemplate { object? Build(object? param); bool Match(object? data); } }
        namespace App.ViewModels { public class ViewModelBase { } public class MainViewModel : ViewModelBase { } public class OrphanViewModel : ViewModelBase { public class NestedViewModel { } } }
        namespace App.Views { public class MainView { } }
        """;

    // The Build method is copied from src/PressToShape.App/ViewLocator.cs.
    private const string RealLocator = """
        namespace App
        {
            public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate
            {
                public object? Build(object? param)
                {
                    if (param is null)
                        return null;

                    var name = param.GetType().FullName!.Replace("ViewModel", "View", System.StringComparison.Ordinal);
                    var type = System.Type.GetType(name);

                    if (type != null)
                    {
                        return System.Activator.CreateInstance(type)!;
                    }

                    return "Not Found: " + name;
                }

                public bool Match(object? data) => data is App.ViewModels.ViewModelBase;
            }
        }
        """;

    private static string AppXaml(string dataTemplates = "<local:ViewLocator />") =>
        $"<Application {Header}>\n<Application.DataTemplates>\n{dataTemplates}\n</Application.DataTemplates>\n</Application>";

    private static (ConventionDetection Detection, Compilation Compilation, XamlDocument Application) Detect(string locatorSource, string? appXaml = null)
    {
        var project = TestCompilation.Project("App", Infrastructure, locatorSource);
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        var application = XamlReader.Read("src/App/App.axaml", "App", appXaml ?? AppXaml()).Document!;
        var resolver = new XamlTypeResolver(project.Compilation, known);
        return (ViewLocatorConvention.Detect(application, resolver, project.Compilation, known), project.Compilation, application);
    }

    [Fact]
    public void Detect_TheRealViewLocatorPattern_IsRecognized()
    {
        var (detection, _, _) = Detect(RealLocator);

        var convention = detection.Convention!;
        Assert.Null(detection.Reason);
        Assert.Equal("ViewModel", convention.Find);
        Assert.Equal("View", convention.Replace);
        Assert.Equal("src/App/App.axaml", convention.RegistrationFile);
        Assert.Equal(3, convention.RegistrationLine);
        Assert.Equal("T:App.ViewLocator", convention.LocatorId);
    }

    [Fact]
    public void Detect_AnotherReplacePair_IsReadFromTheCodeNotAssumed()
    {
        var locator = RealLocator.Replace("\"ViewModel\", \"View\"", "\"Model\", \"Page\"", System.StringComparison.Ordinal);

        var convention = Detect(locator).Detection.Convention!;

        Assert.Equal(("Model", "Page"), (convention.Find, convention.Replace));
    }

    [Fact]
    public void Detect_ReplaceResultPassedDirectlyToGetType_IsRecognized()
    {
        var locator = """
            namespace App { public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate {
                public object? Build(object? param) => System.Type.GetType(param!.GetType().FullName!.Replace("ViewModel", "View"));
                public bool Match(object? data) => true; } }
            """;

        Assert.NotNull(Detect(locator).Detection.Convention);
    }

    [Fact]
    public void Detect_LocatorWithoutReplace_GivesAReason()
    {
        var locator = """
            namespace App { public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate {
                public object? Build(object? param) => System.Type.GetType(param!.GetType().FullName!);
                public bool Match(object? data) => true; } }
            """;

        var detection = Detect(locator).Detection;

        Assert.Null(detection.Convention);
        Assert.Contains("FullName.Replace", detection.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_ReplaceWithoutGetType_GivesAReason()
    {
        var locator = """
            namespace App { public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate {
                public object? Build(object? param) => param!.GetType().FullName!.Replace("ViewModel", "View");
                public bool Match(object? data) => true; } }
            """;

        var detection = Detect(locator).Detection;

        Assert.Null(detection.Convention);
        Assert.Contains("Type.GetType", detection.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_ReplaceOnSomethingElse_IsNotRecognized()
    {
        var locator = """
            namespace App { public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate {
                public object? Build(object? param) { var name = param!.ToString()!.Replace("ViewModel", "View"); return System.Type.GetType(name); }
                public bool Match(object? data) => true; } }
            """;

        Assert.Null(Detect(locator).Detection.Convention);
    }

    [Fact]
    public void Detect_GetTypeOfAnotherString_IsNotRecognized()
    {
        var locator = """
            namespace App { public class ViewLocator : Avalonia.Controls.Templates.IDataTemplate {
                public object? Build(object? param) { var name = param!.GetType().FullName!.Replace("ViewModel", "View"); return System.Type.GetType("App.Views.MainView") ?? (object?)name; }
                public bool Match(object? data) => true; } }
            """;

        Assert.Null(Detect(locator).Detection.Convention);
    }

    [Fact]
    public void Detect_NoRegistration_GivesAReason()
    {
        var (detection, _, _) = Detect(RealLocator, $"<Application {Header} />");

        Assert.Null(detection.Convention);
        Assert.Contains("no data template is registered", detection.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_RegistrationOfATypeThatIsNotALocator_GivesAReason()
    {
        var (detection, _, _) = Detect(RealLocator, AppXaml("<local:Unknown />"));

        Assert.Null(detection.Convention);
        Assert.Contains("is not a class of this solution that implements IDataTemplate", detection.Reason, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_LocatorNotImplementingTheInterface_IsSkipped()
    {
        var locator = RealLocator.Replace(" : Avalonia.Controls.Templates.IDataTemplate", string.Empty, System.StringComparison.Ordinal);

        Assert.Null(Detect(locator).Detection.Convention);
    }

    [Fact]
    public void Detect_SecondRegistrationIsTheLocator_IsFound()
    {
        var (detection, _, _) = Detect(RealLocator, AppXaml("<local:Unknown />\n<local:ViewLocator />"));

        Assert.NotNull(detection.Convention);
        Assert.Equal(4, detection.Convention!.RegistrationLine);
    }

    [Fact]
    public void ViewFor_ExistingCandidate_IsReturned()
    {
        var (detection, compilation, _) = Detect(RealLocator);
        var viewModel = compilation.GetTypeByMetadataName("App.ViewModels.MainViewModel")!;

        var view = detection.Convention!.ViewFor(viewModel);

        Assert.Equal("App.Views.MainView", view!.ToDisplayString());
    }

    [Fact]
    public void ViewFor_NoSuchView_IsNull()
    {
        var (detection, compilation, _) = Detect(RealLocator);

        Assert.Null(detection.Convention!.ViewFor(compilation.GetTypeByMetadataName("App.ViewModels.OrphanViewModel")!));
    }

    [Fact]
    public void ViewFor_NestedViewModel_UsesThePlusSeparatedRuntimeName()
    {
        var (detection, compilation, _) = Detect(RealLocator);
        var nested = compilation.GetTypeByMetadataName("App.ViewModels.OrphanViewModel+NestedViewModel")!;

        Assert.Null(detection.Convention!.ViewFor(nested));
    }

    [Fact]
    public void ViewFor_NameWithoutTheFindText_IsNull()
    {
        var (detection, compilation, _) = Detect(RealLocator);
        var plain = compilation.GetTypeByMetadataName("App.ViewLocator")!;

        Assert.Null(detection.Convention!.ViewFor(plain));
    }

    [Fact]
    public void ViewFor_CandidateInAnotherAssembly_IsNullBecauseTypeGetTypeOnlySearchesTheCallingAssembly()
    {
        var library = TestCompilation.Project("Lib", "namespace App.Views { public class OtherView { } } namespace App.ViewModels { public class OtherViewModel { } }");
        var app = TestCompilation.Project("App", Infrastructure, RealLocator);
        app = app with { Compilation = app.Compilation.AddReferences(library.Compilation.ToMetadataReference()) };
        var extractor = new SymbolExtractor(TestCompilation.RepoRoot);
        var known = extractor.Extract(app).Concat(extractor.Extract(library)).Select(s => s.Id).ToHashSet();
        var application = XamlReader.Read("src/App/App.axaml", "App", AppXaml()).Document!;
        var detection = ViewLocatorConvention.Detect(application, new XamlTypeResolver(app.Compilation, known), app.Compilation, known);

        var viewModel = app.Compilation.GetTypeByMetadataName("App.ViewModels.OtherViewModel")!;

        Assert.Null(detection.Convention!.ViewFor(viewModel));
    }

    [Fact]
    public void Describe_NamesTheRegistrationTheConventionAndTheEvidence()
    {
        var (detection, compilation, _) = Detect(RealLocator);
        var viewModel = compilation.GetTypeByMetadataName("App.ViewModels.MainViewModel")!;
        var view = detection.Convention!.ViewFor(viewModel)!;

        var text = detection.Convention.Describe(viewModel, view, "ContentControl Content binding at src/App/V.axaml:12");

        Assert.Contains("Application.DataTemplates at src/App/App.axaml:3", text, System.StringComparison.Ordinal);
        Assert.Contains("'ViewModel' is replaced by 'View'", text, System.StringComparison.Ordinal);
        Assert.Contains("App.ViewModels.MainViewModel maps to App.Views.MainView", text, System.StringComparison.Ordinal);
        Assert.Contains("Evidence: ContentControl Content binding at src/App/V.axaml:12", text, System.StringComparison.Ordinal);
    }
}
