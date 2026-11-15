using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Loading;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class AvaloniaPagesPartTwoTests
{
    private sealed class FakeProjects(params ProjectCompilation[] projects) : ICompilationSource
    {
        public Task<LoadResult> LoadAsync(CancellationToken cancellationToken) => Task.FromResult(new LoadResult(projects, []));
    }

    private static string Page(string name, bool reverse = false) =>
        Encoding.UTF8.GetString(MvvmSampleData.Index(reverse).Files["markdown/avalonia/" + name]);

    [Fact]
    public void Styles_ListFilesWithCountsAndVariants()
    {
        var page = Page("styles-and-resources.md");

        Assert.Contains("### src/App/App.axaml", page, StringComparison.Ordinal);
        Assert.Contains("variants Dark, Light", page, StringComparison.Ordinal);
        Assert.Contains("5 resources, 0 styles", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Styles_ListKeysThatAreMissingOrUnused()
    {
        var page = Page("styles-and-resources.md");

        var missing = page[page.IndexOf("## Keys referenced but not defined", StringComparison.Ordinal)..page.IndexOf("## Keys defined but never referenced", StringComparison.Ordinal)];
        Assert.Contains("- `SystemAccentColor`, 1 uses", missing, StringComparison.Ordinal);
        Assert.DoesNotContain("WindowBrush", missing, StringComparison.Ordinal);
        var unused = page[page.IndexOf("## Keys defined but never referenced", StringComparison.Ordinal)..];
        Assert.Contains("- `UnusedBrush` (SolidColorBrush, Dark) at src/App/App.axaml:", unused, StringComparison.Ordinal);
        Assert.DoesNotContain("`PanelPadding`", unused, StringComparison.Ordinal);
    }

    [Fact]
    public void Styles_GraphStartsAtTheApplicationFile()
    {
        var page = Page("styles-and-resources.md");

        Assert.Contains("## Include graph from the application file\n\n- src/App/App.axaml\n", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Services_ListInterfacesWithImplementationsUsersAndCreationSites()
    {
        var page = Page("services.md");

        Assert.Contains("### IFileStore (interface)", page, StringComparison.Ordinal);
        Assert.Contains("- used by: `ShellViewModel`\n", page, StringComparison.Ordinal);
        Assert.DoesNotContain("FakeClockUser", page, StringComparison.Ordinal);
        Assert.DoesNotContain("FakeStore", page, StringComparison.Ordinal);
        Assert.Contains("- implemented by `LocalFileStore`, created at `MyApp.OnFrameworkInitializationCompleted` (src/App/File1.cs:", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Services_WithoutRegistrations_NameTheMembersThatCreateServices()
    {
        var page = Page("services.md");

        Assert.Contains("no container registrations found; services are created by hand in `MyApp.Initialize`, `MyApp.OnFrameworkInitializationCompleted`.", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Services_WithARegistrationAndAnInterfaceWithoutImplementation_ListThem()
    {
        const string Source = """
            namespace Microsoft.Extensions.DependencyInjection
            {
                public interface IServiceCollection { }
                public static class Extensions { public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection services) => services; }
            }
            namespace App
            {
                public interface IA { }
                public class A : IA { }
                public interface IB { }
                public class User { public User(IA a, IB b) { } }
                public static class Setup { public static void Run(Microsoft.Extensions.DependencyInjection.IServiceCollection s) => Microsoft.Extensions.DependencyInjection.Extensions.AddSingleton<IA, A>(s); }
            }
            """;
        var data = (await new IndexGenerator(new FakeProjects(TestCompilation.Project("App", Source)), TestCompilation.RepoRoot, null).GenerateAsync(CancellationToken.None)).Data;
        var sink = new InMemoryFileSink();
        IndexGenerator.Write(data, sink);

        var page = Encoding.UTF8.GetString(sink.Files["markdown/avalonia/services.md"]);

        Assert.Contains("- implementations: none found", page, StringComparison.Ordinal);
        Assert.Contains("- `IA` -> `A` (verified):", page, StringComparison.Ordinal);
        Assert.DoesNotContain("no container registrations found", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Application_ListsTheClassItsLifetimeBranchesAndTheConvention()
    {
        var page = Page("application.md");

        Assert.Contains("### MyApp", page, StringComparison.Ordinal);
        Assert.Contains("- `OnFrameworkInitializationCompleted` at line", page, StringComparison.Ordinal);
        Assert.Contains("- when the lifetime is `", page, StringComparison.Ordinal);
        Assert.Contains("- ViewLocator (registered in Application.DataTemplates at src/App/App.axaml:", page, StringComparison.Ordinal);
        Assert.Contains("'ViewModel' is replaced by 'View' in the full type name", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Application_ConventionListsOnlyViewLocatorReasons()
    {
        var page = Page("application.md");

        var section = page[page.IndexOf("## View locator convention\n\n", StringComparison.Ordinal)..page.IndexOf("## Explicit data templates", StringComparison.Ordinal)];
        var lines = section.Split('\n').Where(l => l.StartsWith("- ", StringComparison.Ordinal)).ToList();
        Assert.Single(lines);
        Assert.StartsWith("- ViewLocator (", lines[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Application_ListsContentHostsAsInferredAndClaimsNoNavigation()
    {
        var page = Page("application.md");

        Assert.Contains("shows `DetailViewModel` as `DetailView` (inferred)", page, StringComparison.Ordinal);
        Assert.Contains("no navigation service found", page, StringComparison.Ordinal);
        Assert.Contains("## Explicit data templates\n\nnone found\n", page, StringComparison.Ordinal);
    }

    [Fact]
    public void Application_WithoutAnApplicationClass_SaysNoneFound()
    {
        var sink = new InMemoryFileSink();
        PressToShape.CodeIndex.Output.Avalonia.AvaloniaPagesWriter.Write(new PressToShape.CodeIndex.Model.IndexData([], []), sink);

        var page = Encoding.UTF8.GetString(sink.Files["markdown/avalonia/application.md"]);

        Assert.Contains("## Application class\n\nnone found", page, StringComparison.Ordinal);
        Assert.Contains("none recognized", page, StringComparison.Ordinal);
        Assert.Contains("none found", Encoding.UTF8.GetString(sink.Files["markdown/avalonia/services.md"]), StringComparison.Ordinal);
        Assert.Contains("none found", Encoding.UTF8.GetString(sink.Files["markdown/avalonia/styles-and-resources.md"]), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("styles-and-resources.md")]
    [InlineData("services.md")]
    [InlineData("application.md")]
    public void Pages_AreDeterministicAndFreeOfAbsolutePathsAndTheWordKind(string name)
    {
        var page = Page(name);

        Assert.Equal(page, Page(name, reverse: true));
        Assert.Equal(page, Page(name));
        Assert.DoesNotContain("/home/", page, StringComparison.Ordinal);
        Assert.DoesNotContain("kind", page, StringComparison.OrdinalIgnoreCase);
        Assert.True(page.Split('\n').Length < 400);
    }

    [Fact]
    public void Readme_LinksAllSixPages()
    {
        var readme = Encoding.UTF8.GetString(MvvmSampleData.Index().Files[IndexLayout.MarkdownReadmePath]);

        Assert.Equal(6, readme.Split('\n').Count(l => l.Contains("](avalonia/", StringComparison.Ordinal)));
    }
}
