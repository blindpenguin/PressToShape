using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class PropertiesViewPathTests
{
    private static (EditorState State, PropertiesViewModel Properties, PropertiesView View, Window Window) Show(Tool tool, bool touch = false, Localizer? localizer = null)
    {
        var state = new EditorState { ActiveTool = tool };
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), localizer ?? FakeLocalizer.Create());
        var view = new PropertiesView { DataContext = properties };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 420, Height = 1200, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (state, properties, view, window);
    }

    private static void SelectPath(EditorState state, bool closed = false)
    {
        var path = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 0), new DocumentNode(50, 100)], closed);
        state.Edit(HistoryStepType.DrawPath, d => d.AddShape(state.ActiveLayerId, path));
        state.Select(path.Id);
        Dispatcher.UIThread.RunJobs();
    }

    private static Button? Choice(PropertiesView view, string name) =>
        view.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Classes.Contains("choice") && Avalonia.Automation.AutomationProperties.GetName(b) == name);

    [Fact]
    public void ThePathSection_WithoutAPath_ShowsTheHintAndNoChoices() => HeadlessUi.Run(() =>
    {
        var (_, properties, view, window) = Show(Tool.Pen);
        properties.IsPathExpanded = true;
        Dispatcher.UIThread.RunJobs();

        Assert.True(properties.ShowPath);
        Assert.False(Choice(view, "Closed path")?.IsEffectivelyVisible ?? false);

        window.Close();
    });

    [Fact]
    public void ThePathSection_WithASelectedPath_ShowsTheChoicesAndMarksTheOnesThatAreSet() => HeadlessUi.Run(() =>
    {
        var (state, properties, view, window) = Show(Tool.Select);
        properties.IsPathExpanded = true;
        SelectPath(state);

        var open = Choice(view, "Open path")!;
        var closed = Choice(view, "Closed path")!;
        var nonZero = Choice(view, "Non-zero")!;
        var evenOdd = Choice(view, "Even-odd")!;

        Assert.True(open.IsEffectivelyVisible);
        Assert.Contains("selected", open.Classes);
        Assert.DoesNotContain("selected", closed.Classes);
        Assert.Contains("selected", nonZero.Classes);
        Assert.DoesNotContain("selected", evenOdd.Classes);

        window.Close();
    });

    [Fact]
    public void ThePathSection_ClickingAChoice_ChangesThePathAndMovesTheMark() => HeadlessUi.Run(() =>
    {
        var (state, properties, view, window) = Show(Tool.Select);
        properties.IsPathExpanded = true;
        SelectPath(state);

        Choice(view, "Closed path")!.Command!.Execute(null);
        Choice(view, "Even-odd")!.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var path = Assert.IsType<PathShape>(state.Document.FindShape(state.SelectedShapeId!.Value)!.Shape);
        Assert.True(path.SubPaths[0].IsClosed);
        Assert.Equal(FillRule.EvenOdd, path.FillRule);
        Assert.Contains("selected", Choice(view, "Closed path")!.Classes);
        Assert.DoesNotContain("selected", Choice(view, "Open path")!.Classes);
        Assert.Contains("selected", Choice(view, "Even-odd")!.Classes);

        window.Close();
    });

    [Fact]
    public void ThePathSection_InTheTouchLayout_HasButtonsOfAtLeast48() => HeadlessUi.Run(() =>
    {
        var (state, properties, view, window) = Show(Tool.Select, touch: true);
        properties.IsPathExpanded = true;
        SelectPath(state);

        foreach (var name in new[] { "Open path", "Closed path", "Non-zero", "Even-odd" })
        {
            var button = Choice(view, name)!;
            Assert.True(button.Bounds.Height >= 48, $"{name} is {button.Bounds.Height} high");
            Assert.True(button.Bounds.Width >= 48, $"{name} is {button.Bounds.Width} wide");
        }

        window.Close();
    });
}
