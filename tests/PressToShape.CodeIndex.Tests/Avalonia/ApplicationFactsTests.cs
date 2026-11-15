using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Avalonia;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Extraction;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ApplicationFactsTests
{
    private const string Source = """
        namespace App
        {
            public class Settings { }
            public class Services { }
            public class MainWindow { }
            public class MainView { }
            public class Shell { }

            /// <summary>Formats a flag.</summary>
            public class FlagConverter : Avalonia.Data.Converters.IValueConverter
            {
                public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => value;
            }

            public class PairConverter : Avalonia.Data.Converters.IValueConverter, Avalonia.Data.Converters.IMultiValueConverter
            {
                public object? Convert(object? value, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => value;
                public object? Convert(System.Collections.Generic.IList<object?> values, System.Type targetType, object? parameter, System.Globalization.CultureInfo culture) => null;
            }

            public class LooksLikeAConverter { public object? Convert(object? value) => value; }

            public class MyApp : Avalonia.Application
            {
                public override void Initialize()
                {
                    var settings = new Settings();
                    var text = new System.Text.StringBuilder();
                }

                public override void OnFrameworkInitializationCompleted()
                {
                    var services = new Services();
                    if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var window = new MainWindow();
                    }
                    else if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.ISingleViewApplicationLifetime single)
                    {
                        var view = new MainView();
                        var shell = new Shell();
                    }
                    else if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IActivityApplicationLifetime)
                    {
                        var other = new MainView();
                    }

                    base.OnFrameworkInitializationCompleted();
                }

                public void Helper() { var again = new Settings(); }
            }

            public class NotAnApp { public virtual void Initialize() { var s = new Settings(); } }
        }
        """;

    private static AvaloniaFacts Run(string source = Source)
    {
        var project = XamlTestHost.AppProject("App", source);
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        return new AvaloniaFactsExtractor(TestCompilation.RepoRoot).Extract(project, known, new HashSet<string>());
    }

    [Fact]
    public void Extract_Converters_ListBothInterfacesAndIgnoreLookAlikes()
    {
        var converters = Run().Converters;

        Assert.Equal(["FlagConverter", "PairConverter"], converters.Select(c => c.Name));
        var flag = converters[0];
        Assert.Equal(("T:App.FlagConverter", "Formats a flag.", "src/App/File0.cs"), (flag.Id, flag.Summary, flag.File));
        Assert.Equal(["IValueConverter"], flag.Interfaces);
        Assert.Equal(["IMultiValueConverter", "IValueConverter"], converters[1].Interfaces);
        Assert.Null(converters[1].Summary);
    }

    [Fact]
    public void Extract_Application_HasTheClassAndItsLifecycleOverridesOnly()
    {
        var application = Assert.Single(Run().Applications);

        Assert.Equal(("T:App.MyApp", "MyApp"), (application.Id, application.Name));
        Assert.Equal(["Initialize", "OnFrameworkInitializationCompleted"], application.Overrides.Select(o => o.Name));
        Assert.Equal(["M:App.MyApp.Initialize", "M:App.MyApp.OnFrameworkInitializationCompleted"], application.Overrides.Select(o => o.MethodId));
    }

    [Fact]
    public void Extract_Initialize_ListsCreatedSolutionTypesAndIgnoresFrameworkTypes()
    {
        var initialize = Run().Applications[0].Overrides[0];

        var created = Assert.Single(initialize.Created);

        Assert.Equal(("T:App.Settings", "Settings"), (created.TypeId, created.TypeName));
        Assert.Empty(initialize.Branches);
    }

    [Fact]
    public void Extract_LifetimeBranches_HaveTheirOwnCreations()
    {
        var method = Run().Applications[0].Overrides[1];

        Assert.Equal(["IClassicDesktopStyleApplicationLifetime", "ISingleViewApplicationLifetime", "IActivityApplicationLifetime"], method.Branches.Select(b => b.LifetimeType));
        Assert.Equal(["MainWindow"], method.Branches[0].Created.Select(c => c.TypeName));
        Assert.Equal(["MainView", "Shell"], method.Branches[1].Created.Select(c => c.TypeName));
        Assert.Equal(["MainView"], method.Branches[2].Created.Select(c => c.TypeName));
    }

    [Fact]
    public void Extract_CreationOutsideAnyBranch_IsInTheMethodListAndNotInTheBranches()
    {
        var method = Run().Applications[0].Overrides[1];

        Assert.Equal(["Services"], method.Created.Select(c => c.TypeName));
    }

    [Fact]
    public void Extract_BranchAndMethodLines_AreTheLinesOfTheCode()
    {
        var method = Run().Applications[0].Overrides[1];

        Assert.Equal(31, method.Line);
        Assert.Equal(34, method.Branches[0].Line);
        Assert.Equal(36, method.Branches[0].Created[0].Line);
    }

    [Fact]
    public void Extract_NonOverrideMethodsAndOtherClasses_AreIgnored()
    {
        var application = Run().Applications.Single();

        Assert.DoesNotContain(application.Overrides, o => o.Name == "Helper");
        Assert.DoesNotContain(Run().Applications, a => a.Name == "NotAnApp");
    }

    [Fact]
    public void Extract_NoApplicationOrConverters_GivesEmptyLists()
    {
        var facts = Run("namespace App { public class Plain { } }");

        Assert.Empty(facts.Applications);
        Assert.Empty(facts.Converters);
        Assert.Empty(facts.Controls);
    }

    [Fact]
    public void Extract_OverrideWithoutCreations_IsStillListed()
    {
        var facts = Run("namespace App { public class MyApp : Avalonia.Application { public override void Initialize() { } } }");

        var initialize = Assert.Single(facts.Applications.Single().Overrides);

        Assert.Empty(initialize.Created);
        Assert.Empty(initialize.Branches);
    }

    [Fact]
    public void Extract_TwoRuns_GiveTheSameFacts()
    {
        Assert.Equal(
            Run().Applications.Select(a => (a.Id, a.Overrides.Select(o => (o.MethodId, o.Created.Count, o.Branches.Count)).ToList())),
            Run().Applications.Select(a => (a.Id, a.Overrides.Select(o => (o.MethodId, o.Created.Count, o.Branches.Count)).ToList())));
    }
}
