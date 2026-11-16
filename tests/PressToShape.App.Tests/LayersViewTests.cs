using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// What bindings and view models cannot show about the Objects tab: that the view builds, shows the rows and reacts to the document.
[Collection(HeadlessUi.CollectionName)]
public class LayersViewTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static RectangleShape Rect(string name, double x = 0) => new() { Name = name, X = x, Width = 10, Height = 10 };

    private static (LayersView View, Window Window, EditorState State, LayersViewModel List, ObjectEditor Editor) Show(bool touch, params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        var editor = new ObjectEditor(state);
        var list = new LayersViewModel(state, editor, FakeLocalizer.Create());
        var view = new LayersView { DataContext = list };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 400, Height = 500, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window, state, list, editor);
    }

    private static List<string?> Texts(LayersView view) =>
        view.GetVisualDescendants().OfType<TextBlock>().Where(t => t.IsEffectivelyVisible).Select(t => t.Text).ToList();

    private static List<Button> Buttons(LayersView view, string automationName) =>
        view.GetVisualDescendants().OfType<Button>().Where(b => AutomationProperties.GetName(b) == automationName && b.IsEffectivelyVisible).ToList();

    [Fact]
    public void View_ShowsTheLayerAndTheObjectsTopmostFirst() => HeadlessUi.Run(() =>
    {
        var (view, window, _, _, _) = Show(touch: false, Rect("first"), Rect("second", 20));

        var texts = Texts(view);

        Assert.Contains("Layers", texts);
        Assert.True(texts.IndexOf("Layer") < texts.IndexOf("second"));
        Assert.True(texts.IndexOf("second") < texts.IndexOf("first"));

        window.Close();
    });

    [Fact]
    public void View_HideToggle_HidesTheObjectAndTheRowFollows() => HeadlessUi.Run(() =>
    {
        var a = Rect("a");
        var (view, window, state, list, _) = Show(touch: false, a);

        var toggle = Buttons(view, "Toggle visibility")[1]; // the first one is the layer's
        toggle.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.False(state.Document.FindShape(a.Id)!.Shape.IsVisible);
        Assert.False(list.Rows.Single(r => r.Id == a.Id).IsVisible);
        Assert.Equal(HistoryStepType.HideShapes, state.History.Steps[^1]);

        window.Close();
    });

    [Fact]
    public void View_RowsFollowTheDocument_AGroupShowsAfterGroupingAndOpens() => HeadlessUi.Run(() =>
    {
        var a = Rect("a");
        var b = Rect("b", 20);
        var (view, window, state, list, editor) = Show(touch: false, a, b);
        state.SetSelection([a.Id, b.Id]);

        editor.Group();
        Dispatcher.UIThread.RunJobs();

        var group = list.Rows.Single(r => r.IsGroup);
        Assert.DoesNotContain("a", Texts(view));
        Assert.Contains("Group", Texts(view));

        list.SetExpanded(group, true);
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("a", Texts(view));
        Assert.Contains("b", Texts(view));

        window.Close();
    });

    [Fact]
    public void View_SelectingOnTheCanvas_MarksTheRowSelected() => HeadlessUi.Run(() =>
    {
        var a = Rect("a");
        var (view, window, state, list, _) = Show(touch: false, a, Rect("b", 20));

        state.SelectOnly(a.Id);
        Dispatcher.UIThread.RunJobs();

        var selected = view.GetVisualDescendants().OfType<Border>().Where(b => b.Classes.Contains("selected")).ToList();
        Assert.Single(selected);
        Assert.True(list.Rows.Single(r => r.Id == a.Id).IsSelected);

        window.Close();
    });

    [Fact]
    public void View_WithTheTouchSizes_EveryButtonIsAtLeast48() => HeadlessUi.Run(() =>
    {
        var (view, window, _, _, _) = Show(touch: true, Rect("a"), Rect("b", 20));

        var buttons = view.GetVisualDescendants().OfType<Button>().Where(b => b.IsEffectivelyVisible && b.TemplatedParent is null).ToList();

        Assert.Equal(8, buttons.Count); // add layer, the layer's expander and two toggles, and two toggles for each of the two objects
        Assert.All(buttons, b => Assert.True(b.Bounds.Width >= 48 && b.Bounds.Height >= 48, $"{AutomationProperties.GetName(b)} {b.Bounds.Width:0}x{b.Bounds.Height:0}"));

        window.Close();
    });

    [Fact]
    public void Inspector_ShowsTheLayersTabThroughTheViewLocator() => HeadlessUi.Run(() =>
    {
        var shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        shell.Inspector.ShowTab(InspectorTab.Layers);

        var view = new PressToShape.App.Views.Editor.InspectorView { DataContext = shell.Inspector };
        var window = new Window { Width = 400, Height = 600, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.Single(view.GetVisualDescendants().OfType<LayersView>());
        Assert.Equal(4, view.GetVisualDescendants().OfType<TabItem>().Count());

        window.Close();
    });

    [Fact]
    public void TabletPanel_ShowsTheLayersListForTheLayersTab()
    {
        var shell = new TabletShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create(), new MessageDialogHost());
        shell.Panel.Toggle(InspectorTab.Layers);

        Assert.Same(shell.Inspector.Layers, shell.Panel.Content);
        Assert.Equal("Layers", shell.Panel.Title);
    }
}
