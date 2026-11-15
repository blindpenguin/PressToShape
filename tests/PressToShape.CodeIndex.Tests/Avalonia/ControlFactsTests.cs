using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Avalonia;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Extraction;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ControlFactsTests
{
    private const string Source = """
        namespace App.Controls
        {
            /// <summary>A ruler along the canvas.</summary>
            public class Ruler : Avalonia.Controls.Control
            {
                public static readonly Avalonia.StyledProperty<double> ZoomProperty = Avalonia.AvaloniaProperty.Register<Ruler, double>(nameof(Zoom), 100);
                public double Zoom { get; set; }

                public static readonly Avalonia.StyledProperty<bool> HiddenProperty = Avalonia.AvaloniaProperty.Register<Ruler, bool>("Hidden");

                public static readonly Avalonia.AttachedProperty<int> LevelProperty = Avalonia.AvaloniaProperty.RegisterAttached<Ruler, Avalonia.Controls.Control, int>("Level", 1);
                public static int GetLevel(Avalonia.Controls.Control control) => 0;
                public static void SetLevel(Avalonia.Controls.Control control, int value) { }

                public static readonly Avalonia.AttachedProperty<string> NoGetterProperty = Avalonia.AvaloniaProperty.RegisterAttached<Ruler, Avalonia.Controls.Control, string>("NoGetter");

                public static readonly Avalonia.DirectProperty<Ruler, string> TextProperty = Avalonia.AvaloniaProperty.RegisterDirect<Ruler, string>(nameof(Text), o => o.Text);
                public string Text => "";

                public static readonly Avalonia.StyledProperty<int> ComputedProperty = Avalonia.AvaloniaProperty.Register<Ruler, int>(Name());
                public int Computed { get; set; }
                private static string Name() => "dynamic";

                public static readonly Avalonia.Interactivity.RoutedEvent<Avalonia.Interactivity.RoutedEventArgs> ChangedEvent =
                    Avalonia.Interactivity.RoutedEvent.Register<Ruler, Avalonia.Interactivity.RoutedEventArgs>(nameof(Changed), Avalonia.Interactivity.RoutingStrategies.Bubble);

                public static readonly Avalonia.Interactivity.RoutedEvent<Avalonia.Interactivity.RoutedEventArgs> OtherEvent =
                    Avalonia.Interactivity.RoutedEvent.Register<Ruler, Avalonia.Interactivity.RoutedEventArgs>("Other", Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble);

                public event System.EventHandler? Changed;
            }

            public class Plain : Avalonia.Controls.Control { }
            public partial class ShellView : Avalonia.Controls.UserControl { }
            public class NotAControl { public static readonly Avalonia.StyledProperty<int> XProperty = Avalonia.AvaloniaProperty.Register<NotAControl, int>("X"); }
        }
        """;

    private static (AvaloniaFacts Facts, ControlFact Ruler) Run(IReadOnlySet<string>? xamlClasses = null)
    {
        var project = XamlTestHost.AppProject("App", Source);
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        var facts = new AvaloniaFactsExtractor(TestCompilation.RepoRoot).Extract(project, known, xamlClasses ?? new HashSet<string>());
        return (facts, facts.Controls.Single(c => c.Name == "Ruler"));
    }

    [Fact]
    public void Extract_CustomControl_HasIdBaseTypeSummaryFileAndLine()
    {
        var (_, ruler) = Run();

        Assert.Equal("T:App.Controls.Ruler", ruler.Id);
        Assert.Equal("Control", ruler.BaseType);
        Assert.Equal("A ruler along the canvas.", ruler.Summary);
        Assert.Equal("src/App/File0.cs", ruler.File);
        Assert.Equal(4, ruler.Line);
    }

    [Fact]
    public void Extract_ClassesThatAreNotControlsOrAreViews_AreSkipped()
    {
        var (facts, _) = Run(new HashSet<string> { "T:App.Controls.ShellView" });

        Assert.Equal(["Plain", "Ruler"], facts.Controls.Select(c => c.Name));
    }

    [Fact]
    public void Extract_StyledProperty_HasNameTypeDefaultAndWrapper()
    {
        var zoom = Run().Ruler.Properties.Single(p => p.Name == "Zoom");

        Assert.Equal(("F:App.Controls.Ruler.ZoomProperty", "Styled", "double", "100", true), (zoom.FieldId, zoom.PropertyType, zoom.ValueType, zoom.Default, zoom.HasClrWrapper));
        Assert.Equal(6, zoom.Line);
    }

    [Fact]
    public void Extract_StyledPropertyWithoutDefaultAndWrapper_SaysSo()
    {
        var hidden = Run().Ruler.Properties.Single(p => p.Name == "Hidden");

        Assert.Null(hidden.Default);
        Assert.False(hidden.HasClrWrapper);
        Assert.Equal("bool", hidden.ValueType);
    }

    [Fact]
    public void Extract_AttachedProperty_UsesTheStaticGetterAsWrapper()
    {
        var properties = Run().Ruler.Properties;

        var level = properties.Single(p => p.Name == "Level");
        var noGetter = properties.Single(p => p.Name == "NoGetter");

        Assert.Equal(("Attached", "int", "1", true), (level.PropertyType, level.ValueType, level.Default, level.HasClrWrapper));
        Assert.Equal(("Attached", "string", false), (noGetter.PropertyType, noGetter.ValueType, noGetter.HasClrWrapper));
    }

    [Fact]
    public void Extract_DirectProperty_UsesTheValueTypeArgument()
    {
        var text = Run().Ruler.Properties.Single(p => p.Name == "Text");

        Assert.Equal(("Direct", "string", true), (text.PropertyType, text.ValueType, text.HasClrWrapper));
    }

    [Fact]
    public void Extract_NonConstantRegistrationName_FallsBackToTheFieldNameWithoutProperty()
    {
        var computed = Run().Ruler.Properties.Single(p => p.FieldId.EndsWith("ComputedProperty", System.StringComparison.Ordinal));

        Assert.Equal("Computed", computed.Name);
        Assert.True(computed.HasClrWrapper);
    }

    [Fact]
    public void Extract_Properties_AreSortedByName()
    {
        var names = Run().Ruler.Properties.Select(p => p.Name).ToList();

        Assert.Equal(names.OrderBy(n => n, System.StringComparer.Ordinal), names);
        Assert.Equal(6, names.Count);
    }

    [Fact]
    public void Extract_RoutedEvents_HaveNameArgumentTypeAndRoutingAsWritten()
    {
        var events = Run().Ruler.Events;

        var changed = events.Single(e => e.Name == "Changed");
        var other = events.Single(e => e.Name == "Other");

        Assert.Equal(("F:App.Controls.Ruler.ChangedEvent", "RoutedEventArgs", "Avalonia.Interactivity.RoutingStrategies.Bubble"), (changed.FieldId, changed.ArgumentType, changed.Routing));
        Assert.Equal("Avalonia.Interactivity.RoutingStrategies.Tunnel | Avalonia.Interactivity.RoutingStrategies.Bubble", other.Routing);
    }

    [Fact]
    public void Extract_ControlWithoutPropertiesOrEvents_HasEmptyLists()
    {
        var plain = Run().Facts.Controls.Single(c => c.Name == "Plain");

        Assert.Empty(plain.Properties);
        Assert.Empty(plain.Events);
        Assert.Null(plain.Summary);
    }

    [Fact]
    public void Extract_ProjectWithoutControls_IsEmptyAndTheOutputIsDeterministic()
    {
        var project = XamlTestHost.AppProject("App", "namespace App { public class Plain { } }");
        var known = new SymbolExtractor(TestCompilation.RepoRoot).Extract(project).Select(s => s.Id).ToHashSet();
        var extractor = new AvaloniaFactsExtractor(TestCompilation.RepoRoot);

        var first = extractor.Extract(project, known, new HashSet<string>());
        var second = Run().Facts;

        Assert.Empty(first.Controls);
        Assert.Equal(second.Controls.Select(c => c.Id), Run().Facts.Controls.Select(c => c.Id));
    }
}
