using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class PropertiesViewNodeTests
{
    private static (EditorState State, PropertiesViewModel Properties, PropertiesView View, Window Window, PathShape Path) Show(string tool, bool touch = false)
    {
        var path = PathShape.Create([new DocumentNode(0, 100), new DocumentNode(100, 100), new DocumentNode(200, 100), new DocumentNode(300, 150)]);
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [path])) { ActiveTool = System.Enum.Parse<Tool>(tool) };
        state.Select(path.Id);
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());
        var view = new PropertiesView { DataContext = properties };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 420, Height = 1400, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (state, properties, view, window, path);
    }

    private static Button? Choice(PropertiesView view, string name) =>
        view.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Classes.Contains("choice") && Avalonia.Automation.AutomationProperties.GetName(b) == name);

    private static TextBox? Field(PropertiesView view, string name) =>
        view.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(t => Avalonia.Automation.AutomationProperties.GetName(t) == name);

    [Fact]
    public void NodeSection_WithTheNodeTool_ShowsPositionTypeAndSegmentControls() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, path) = Show("Node");
        state.SetNodeSelection([new NodeRef(path.Id, 2)]);
        Dispatcher.UIThread.RunJobs();

        Assert.True(Field(view, "Node X position")!.IsEffectivelyVisible);
        Assert.True(Field(view, "Node Y position")!.IsEffectivelyVisible);
        foreach (var name in new[] { "Corner", "Smooth", "Symmetric", "Straight segment", "Curved segment" })
        {
            Assert.True(Choice(view, name)!.IsEffectivelyVisible, name);
        }

        window.Close();
    });

    [Fact]
    public void NodeSection_WithDirectSelect_ShowsThePositionButNotTheTypeAndSegmentButtons() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, path) = Show("DirectSelect");
        state.SetNodeSelection([new NodeRef(path.Id, 2)]);
        Dispatcher.UIThread.RunJobs();

        Assert.True(Field(view, "Node X position")!.IsEffectivelyVisible);
        Assert.False(Choice(view, "Smooth")!.IsEffectivelyVisible);

        window.Close();
    });

    [Theory]
    [InlineData("Select")]
    [InlineData("Pen")]
    public void NodeSection_WithOtherTools_IsNotShown(string tool) => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show(tool);

        Assert.False(Field(view, "Node X position")?.IsEffectivelyVisible ?? false);

        window.Close();
    });

    [Fact]
    public void NodeSection_TheTypeButtonMarksTheTypeAndClickingSetsIt() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, path) = Show("Node");
        state.SetNodeSelection([new NodeRef(path.Id, 2)]);
        Dispatcher.UIThread.RunJobs();
        Assert.Contains("selected", Choice(view, "Corner")!.Classes);
        Assert.DoesNotContain("selected", Choice(view, "Smooth")!.Classes);

        Choice(view, "Smooth")!.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(NodeType.Smooth, ((PathShape)state.Document.FindShape(path.Id)!.Shape).SubPaths[0].Nodes[2].Type);
        Assert.Contains("selected", Choice(view, "Smooth")!.Classes);
        Assert.DoesNotContain("selected", Choice(view, "Corner")!.Classes);

        window.Close();
    });

    [Fact]
    public void NodeSection_TheFieldsShowThePositionOfTheNode() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, path) = Show("Node");

        state.SetNodeSelection([new NodeRef(path.Id, 2)]);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("200", Field(view, "Node X position")!.Text);
        Assert.Equal("100", Field(view, "Node Y position")!.Text);
        Assert.True(Field(view, "Node X position")!.IsEnabled);

        state.SetNodeSelection([]);
        Dispatcher.UIThread.RunJobs();

        Assert.False(Field(view, "Node X position")!.IsEnabled);

        window.Close();
    });

    [Fact]
    public void NodeSection_TheSegmentButtonsFollowTheSelectedNeighbors() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, path) = Show("Node");
        state.SetNodeSelection([new NodeRef(path.Id, 2), new NodeRef(path.Id, 3)]);
        Dispatcher.UIThread.RunJobs();
        // A button whose command cannot run is effectively disabled, while its own IsEnabled stays true.
        Assert.True(Choice(view, "Curved segment")!.IsEffectivelyEnabled);

        state.SetNodeSelection([new NodeRef(path.Id, 0), new NodeRef(path.Id, 3)]);
        Dispatcher.UIThread.RunJobs();

        Assert.False(Choice(view, "Curved segment")!.IsEffectivelyEnabled);
        Assert.False(Choice(view, "Straight segment")!.IsEffectivelyEnabled);

        window.Close();
    });

    [Fact]
    public void NodeSection_InTheTouchLayout_HasButtonsOfAtLeast48() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, path) = Show("Node", touch: true);
        state.SetNodeSelection([new NodeRef(path.Id, 2)]);
        Dispatcher.UIThread.RunJobs();

        foreach (var name in new[] { "Corner", "Smooth", "Symmetric", "Straight segment", "Curved segment" })
        {
            var button = Choice(view, name)!;
            Assert.True(button.Bounds.Height >= 48, $"{name} is {button.Bounds.Height} high");
            Assert.True(button.Bounds.Width >= 48, $"{name} is {button.Bounds.Width} wide");
        }

        foreach (var name in new[] { "Node X position", "Node Y position" })
        {
            Assert.True(Field(view, name)!.Bounds.Height >= 48, name);
        }

        window.Close();
    });

    [Fact]
    public void NodeSection_CanBeCollapsed() => HeadlessUi.Run(() =>
    {
        var (_, properties, view, window, _) = Show("Node");
        Assert.True(Field(view, "Node X position")!.IsEffectivelyVisible);

        properties.IsNodeExpanded = false;
        Dispatcher.UIThread.RunJobs();

        Assert.False(Field(view, "Node X position")!.IsEffectivelyVisible);

        window.Close();
    });

    [Fact]
    public void NodeSection_InANarrowPanel_WrapsTheButtonsInsteadOfOverflowing() => HeadlessUi.Run(() =>
    {
        var path = PathShape.Create([new DocumentNode(0, 100), new DocumentNode(100, 100), new DocumentNode(200, 100)]);
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [path])) { ActiveTool = Tool.Node };
        state.Select(path.Id);
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());
        var view = new PropertiesView { DataContext = properties };
        var window = new Window { Width = 250, Height = 1400, Content = view };
        window.Show();
        state.SetNodeSelection([new NodeRef(path.Id, 1)]);
        Dispatcher.UIThread.RunJobs();

        foreach (var name in new[] { "Corner", "Smooth", "Symmetric", "Straight segment", "Curved segment" })
        {
            var button = Choice(view, name)!;
            var right = button.TranslatePoint(new Point(button.Bounds.Width, 0), view)!.Value.X;
            Assert.True(right <= view.Bounds.Width + 0.5, $"{name} ends at {right} in a panel {view.Bounds.Width} wide");
        }

        window.Close();
    });
}
