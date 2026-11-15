using System.Linq;
using System.Text.Json;
using PressToShape.CodeIndex.Avalonia.Model;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Output;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class AvaloniaModelTests
{
    private static AvaloniaFacts Sample() => new(
        [
            new ControlFact("T:App.Ruler", "Ruler", "Control", "src/App/Ruler.cs", 10, "A ruler.",
                [new AvaloniaPropertyFact("F:App.Ruler.ZoomProperty", "Zoom", "Styled", "double", "100", true, 12)],
                [new RoutedEventFact("F:App.Ruler.ChangedEvent", "Changed", "RoutedEventArgs", "RoutingStrategies.Bubble", 14)]),
        ],
        [new ConverterFact("T:App.FlagConverter", "FlagConverter", "src/App/FlagConverter.cs", 5, null, ["IValueConverter"])],
        [
            new ApplicationFact("T:App.MyApp", "MyApp", "src/App/MyApp.cs", [
                new LifecycleFact("M:App.MyApp.OnStart", "OnStart", 20, [new CreatedFact("T:App.Settings", "Settings", 21)],
                    [new LifetimeBranchFact("IClassicDesktopStyleApplicationLifetime", 23, [new CreatedFact("T:App.MainWindow", "MainWindow", 24)])]),
            ]),
        ]);

    [Fact]
    public void RelationshipType_ExecutesMethod_IsAppendedLast()
    {
        var names = System.Enum.GetNames<RelationshipType>();

        Assert.Equal("ExecutesMethod", names[^1]);
        Assert.Equal(["Inherits", "Implements", "InjectedDependency", "References", "Calls", "Registers"], names.Take(6));
        Assert.Contains("\"Type\":\"ExecutesMethod\"", JsonSerializer.Serialize(new RelationshipRecord(RelationshipType.ExecutesMethod, "P:A.B", "M:A.C", null, null, Confidence.Verified, "[RelayCommand]"), JsonIndexWriter.Options).Replace(" ", string.Empty), System.StringComparison.Ordinal);
    }

    [Fact]
    public void IndexSchema_Version_IsThree() => Assert.Equal(3, IndexSchema.Version);

    [Fact]
    public void IndexData_WithoutFacts_HasNullAvalonia()
    {
        var data = new IndexData([], []);

        Assert.Null(data.Avalonia);
        Assert.Equal(data, new IndexData([], []) with { Avalonia = null });
    }

    [Fact]
    public void IndexData_WithFacts_KeepsThem()
    {
        var data = new IndexData([], [], Sample());

        Assert.Equal("Ruler", data.Avalonia!.Controls.Single().Name);
        Assert.Equal("Zoom", data.Avalonia.Controls[0].Properties.Single().Name);
    }

    [Fact]
    public void AvaloniaFacts_Empty_HasNoItems()
    {
        Assert.Empty(AvaloniaFacts.Empty.Controls);
        Assert.Empty(AvaloniaFacts.Empty.Converters);
        Assert.Empty(AvaloniaFacts.Empty.Applications);
    }

    [Fact]
    public void Facts_AreWritableAsPlainDataAndReadBack()
    {
        var facts = Sample();

        var json = JsonSerializer.Serialize(facts, JsonIndexWriter.Options);
        var back = JsonSerializer.Deserialize<AvaloniaFacts>(json, JsonIndexWriter.Options)!;

        Assert.Equal(json, JsonSerializer.Serialize(back, JsonIndexWriter.Options));
        Assert.Equal("IClassicDesktopStyleApplicationLifetime", back.Applications[0].Overrides[0].Branches[0].LifetimeType);
        Assert.Equal("RoutingStrategies.Bubble", back.Controls[0].Events[0].Routing);
    }

    [Fact]
    public void Writer_DoesNotWriteFactsAsFiles()
    {
        var sink = new InMemoryFileSink();

        JsonIndexWriter.Write(new IndexData([new ProjectIndex("P", "src/P/P.csproj", [], [])], [], Sample()), sink);

        Assert.DoesNotContain(sink.Files.Keys, k => k.Contains("avalonia", System.StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(sink.Files.Values, bytes => System.Text.Encoding.UTF8.GetString(bytes).Contains("FlagConverter", System.StringComparison.Ordinal));
    }
}
