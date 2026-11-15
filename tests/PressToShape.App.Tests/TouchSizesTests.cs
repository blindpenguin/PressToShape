using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// The touch sizes are a dictionary in Theme.axaml that the tablet layout merges into its resources. These tests check the file and the mechanism.
[Collection(HeadlessUi.CollectionName)]
public class TouchSizesTests
{
    private const string ThemeFile = "src/PressToShape.App/Styles/Theme.axaml";
    private const double MinimumTouchTarget = 48;

    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static (Dictionary<string, string> Desktop, Dictionary<string, string> Touch) ReadSizeKeys()
    {
        var root = XDocument.Load(RepoPaths.Combine(ThemeFile)).Root!;
        var touchElement = root.Elements().Single(e => (string?)e.Attribute(Xaml + "Key") == "TouchSizes");
        var desktop = root.Elements().Where(e => e != touchElement && e.Attribute(Xaml + "Key") is not null && e.Name.LocalName is "Double" or "Int32" or "Thickness")
            .ToDictionary(e => (string)e.Attribute(Xaml + "Key")!, e => e.Name.LocalName + ":" + e.Value);
        var touch = touchElement.Elements().ToDictionary(e => (string)e.Attribute(Xaml + "Key")!, e => e.Name.LocalName + ":" + e.Value);
        return (desktop, touch);
    }

    [Fact]
    public void EveryTouchKey_HasADesktopValueOfTheSameType()
    {
        var (desktop, touch) = ReadSizeKeys();

        Assert.NotEmpty(touch);
        foreach (var (key, value) in touch)
        {
            Assert.True(desktop.ContainsKey(key), $"The touch size {key} has no desktop value.");
            Assert.Equal(desktop[key].Split(':')[0], value.Split(':')[0]);
        }
    }

    [Theory]
    [InlineData("ToolButtonSize")]
    [InlineData("ToolbarButtonSize")]
    public void TouchTargets_AreAtLeast48(string key)
    {
        var (_, touch) = ReadSizeKeys();

        Assert.True(double.Parse(touch[key].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture) >= MinimumTouchTarget);
    }

    private static (ToolboxView View, Window Window) ShowToolbox(bool touch)
    {
        var state = new EditorState();
        var view = new ToolboxView { DataContext = new ToolboxViewModel(state, FakeLocalizer.Create()) };
        var host = new Border { Child = view };
        if (touch)
        {
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 400, Height = 900, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    // The distance between the tops of neighbouring rows. The buttons must sit one below the other (36 or 48 plus their margins), not be spread over the height.
    private static double[] RowDistances(List<ToggleButton> buttons, Visual window)
    {
        var tops = buttons.Select(b => Math.Round(b.TranslatePoint(new Point(0, 0), window)!.Value.Y)).Distinct().OrderBy(y => y).ToList();
        return tops.Zip(tops.Skip(1), (a, b) => b - a).Distinct().ToArray();
    }

    private static List<ToggleButton> ToolButtons(ToolboxView view) => view.GetVisualDescendants().OfType<ToggleButton>().ToList();

    [Fact]
    public void ApplyTo_CanBeUsedForSeveralRoots() => HeadlessUi.Run(() =>
    {
        var first = new Border();
        var second = new Border();

        TouchSizes.ApplyTo(first);
        TouchSizes.ApplyTo(second);

        Assert.True(first.Resources.ContainsKey("ToolButtonSize"));
        Assert.True(second.Resources.ContainsKey("ToolButtonSize"));
        Assert.Equal(48.0, first.Resources["ToolButtonSize"]);
    });

    [Fact]
    public void Desktop_ToolColumn_KeepsOneColumnOf36() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowToolbox(touch: false);

        var buttons = ToolButtons(view);
        Assert.Equal(ToolDefinitions.All.Count, buttons.Count);
        Assert.All(buttons, b => Assert.Equal(36, b.Bounds.Width));
        Assert.All(buttons, b => Assert.Equal(36, b.Bounds.Height));
        Assert.Equal(new[] { 40.0 }, RowDistances(buttons, window));
        Assert.Single(buttons.Select(b => Math.Round(b.TranslatePoint(new Point(0, 0), window)!.Value.X)).Distinct());
        // The column is as wide as it was: the swatch area (44), the padding (4 + 4) and the border (1).
        Assert.Equal(53, view.DesiredSize.Width);
        // The swap and reset buttons keep their 20 dp: a size on the button itself would lose against the Button.icon style.
        var swatchButtons = view.GetVisualDescendants().OfType<Button>().Where(b => b is not ToggleButton).ToList();
        Assert.Equal(2, swatchButtons.Count);
        Assert.All(swatchButtons, b => Assert.Equal((20, 20), (b.Bounds.Width, b.Bounds.Height)));

        window.Close();
    });

    [Fact]
    public void Desktop_TappingTheActiveToolAgain_KeepsItHighlighted() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowToolbox(touch: false);
        var select = ToolButtons(view)[0];
        var center = select.TranslatePoint(new Point(18, 18), window)!.Value;

        window.MouseDown(center, Avalonia.Input.MouseButton.Left);
        window.MouseUp(center, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(select.IsChecked);
        window.Close();
    });

    [Fact]
    public void WithTheTouchSizes_ToolButtonsAre48_InOneColumn() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowToolbox(touch: true);

        var buttons = ToolButtons(view);
        Assert.Equal(ToolDefinitions.All.Count, buttons.Count);
        Assert.All(buttons, b => Assert.Equal(48, b.Bounds.Width));
        Assert.All(buttons, b => Assert.Equal(48, b.Bounds.Height));
        Assert.Single(buttons.Select(b => Math.Round(b.TranslatePoint(new Point(0, 0), window)!.Value.X)).Distinct());
        Assert.Equal(new[] { 56.0 }, RowDistances(buttons, window));

        window.Close();
    });

    // ---- Properties panel ----

    private static (PropertiesView View, Window Window) ShowProperties(bool touch)
    {
        var state = new EditorState();
        var view = new PropertiesView { DataContext = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create()) };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 420, Height = 1200, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    // The controls of the view itself. Parts inside the template of another control (a slider's buttons, the buttons of a text box) are not counted.
    private static List<Control> InteractiveControls(Visual view) => view.GetVisualDescendants()
        .OfType<Control>()
        .Where(c => c is Button or TextBox or CheckBox or Slider && c.TemplatedParent is null && c.IsEffectivelyVisible)
        .ToList();

    [Fact]
    public void Desktop_PropertiesPanel_KeepsItsSizes() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowProperties(touch: false);

        var fields = view.GetVisualDescendants().OfType<TextBox>().Where(t => t.Classes.Contains("field")).ToList();
        Assert.NotEmpty(fields);
        Assert.All(fields, f => Assert.Equal(28, f.Bounds.Height));
        var swatch = view.GetVisualDescendants().OfType<Border>().First(b => b.Classes.Contains("swatch"));
        Assert.Equal((52, 24), (swatch.Bounds.Width, swatch.Bounds.Height));
        var keepProportions = view.GetVisualDescendants().OfType<ToggleButton>().First(t => t.Classes.Contains("tool"));
        Assert.Equal(36, keepProportions.Bounds.Width);

        window.Close();
    });

    [Fact]
    public void WithTheTouchSizes_EveryControlOfThePropertiesPanel_IsAtLeast48High() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowProperties(touch: true);

        var controls = InteractiveControls(view);
        Assert.True(controls.Count > 15, "The panel should have many controls, found " + controls.Count);
        var tooSmall = controls.Where(c => c.Bounds.Height < MinimumTouchTarget).Select(Describe).ToList();
        Assert.True(tooSmall.Count == 0, "Controls under 48 dp high: " + string.Join(", ", tooSmall));

        window.Close();
    });

    [Fact]
    public void WithTheTouchSizes_ButtonsOfThePropertiesPanel_AreAtLeast48Wide() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowProperties(touch: true);

        var tooNarrow = InteractiveControls(view).Where(c => c is Button or CheckBox && c.Bounds.Width < MinimumTouchTarget).Select(Describe).ToList();
        Assert.True(tooNarrow.Count == 0, "Controls under 48 dp wide: " + string.Join(", ", tooNarrow));

        window.Close();
    });

    private static string Describe(Control c) =>
        $"{c.GetType().Name} '{Avalonia.Automation.AutomationProperties.GetName(c)}' {c.Bounds.Width:0}x{c.Bounds.Height:0}";

    // ---- Layers and History panels ----

    private static (Control View, Window Window) ShowPanel(Func<EditorState, Control> create, bool touch)
    {
        var document = Document.Create("Layer A") with { Layers = [new Layer { Name = "Layer A" }, new Layer { Name = "Layer B" }, new Layer { Name = "Layer C" }] };
        var state = new EditorState(document);
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 10, Height = 10 }));
        state.Edit(HistoryStepType.DrawEllipse, d => d.AddShape(state.ActiveLayerId, new EllipseShape { Width = 10, Height = 10 }));
        var view = create(state);
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 400, Height = 900, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    private static Control CreateLayers(EditorState state) => new LayersView { DataContext = new LayersViewModel(state) };

    private static Control CreateHistory(EditorState state) => new HistoryView { DataContext = new HistoryViewModel(state, FakeLocalizer.Create()) };

    [Fact]
    public void Desktop_LayerRows_KeepTheirSize() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowPanel(CreateLayers, touch: false);

        var rows = view.GetVisualDescendants().OfType<ListBoxItem>().ToList();
        Assert.Equal(3, rows.Count);
        Assert.All(rows, r => Assert.Equal(30, r.Bounds.Height));
        var rowButtons = view.GetVisualDescendants().OfType<Button>().Where(b => b.TemplatedParent is null && b.FindAncestorOfType<ListBoxItem>() is not null).ToList();
        Assert.Equal(6, rowButtons.Count);
        Assert.All(rowButtons, b => Assert.Equal(28, b.Bounds.Width));

        window.Close();
    });

    [Fact]
    public void Desktop_HistoryRows_KeepTheirSize() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowPanel(CreateHistory, touch: false);

        var rows = view.GetVisualDescendants().OfType<ListBoxItem>().ToList();
        Assert.Equal(3, rows.Count); // the start state and two steps
        Assert.All(rows, r => Assert.Equal(28, r.Bounds.Height));

        window.Close();
    });

    [Fact]
    public void WithTheTouchSizes_LayerRowsAndTheirButtons_AreAtLeast48() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowPanel(CreateLayers, touch: true);

        var rows = view.GetVisualDescendants().OfType<ListBoxItem>().ToList();
        Assert.Equal(3, rows.Count);
        Assert.All(rows, r => Assert.True(r.Bounds.Height >= MinimumTouchTarget, "Layer row " + r.Bounds.Height));
        var buttons = InteractiveControls(view);
        Assert.Equal(8, buttons.Count); // add, more, and a lock and a visibility button per layer
        var tooSmall = buttons.Where(c => c.Bounds.Width < MinimumTouchTarget || c.Bounds.Height < MinimumTouchTarget).Select(Describe).ToList();
        Assert.True(tooSmall.Count == 0, "Controls under 48 dp: " + string.Join(", ", tooSmall));

        window.Close();
    });

    [Fact]
    public void WithTheTouchSizes_HistoryRows_AreAtLeast48High() => HeadlessUi.Run(() =>
    {
        var (view, window) = ShowPanel(CreateHistory, touch: true);

        var rows = view.GetVisualDescendants().OfType<ListBoxItem>().ToList();
        Assert.Equal(3, rows.Count);
        Assert.All(rows, r => Assert.True(r.Bounds.Height >= MinimumTouchTarget, "History row " + r.Bounds.Height));

        window.Close();
    });

    // ---- selection handles on the canvas ----

    private static (CanvasAreaView View, CanvasAreaViewModel ViewModel, Window Window) ShowCanvas(bool touch, double? grabSizeOverride = null)
    {
        var state = new EditorState();
        var viewModel = new CanvasAreaViewModel(state);
        var view = new CanvasAreaView();
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        if (grabSizeOverride is { } grab)
        {
            host.Resources["TouchHandleGrabSize"] = grab;
        }

        var window = new Window { Width = 800, Height = 600, Content = host };
        window.Show();
        view.DataContext = viewModel;
        Dispatcher.UIThread.RunJobs();
        return (view, viewModel, window);
    }

    [Fact]
    public void Canvas_ShowsItsRulers_ByDefault_AndHidesThemWhenAsked() => HeadlessUi.Run(() =>
    {
        var (view, _, window) = ShowCanvas(touch: false);
        var rulers = view.GetVisualDescendants().OfType<RulerControl>().ToList();
        Assert.Equal(2, rulers.Count);
        Assert.All(rulers, r => Assert.True(r.IsEffectivelyVisible));

        view.ShowRulers = false;
        Dispatcher.UIThread.RunJobs();

        Assert.All(rulers, r => Assert.False(r.IsEffectivelyVisible));
        window.Close();
    });

    [Fact]
    public void Desktop_SelectionHandles_AreDrawnWith8() => HeadlessUi.Run(() =>
    {
        var (view, _, window) = ShowCanvas(touch: false);

        Assert.Equal(8, view.GetVisualDescendants().OfType<DocumentCanvasControl>().Single().HandleSize);

        window.Close();
    });

    [Fact]
    public void WithTheTouchSizes_SelectionHandles_AreDrawnLarger() => HeadlessUi.Run(() =>
    {
        var (view, _, window) = ShowCanvas(touch: true);

        Assert.Equal(18, view.GetVisualDescendants().OfType<DocumentCanvasControl>().Single().HandleSize);

        window.Close();
    });

    [Fact]
    public void TheTouchGrabSize_ComesFromTheTheme() => HeadlessUi.Run(() =>
    {
        var (_, viewModel, window) = ShowCanvas(touch: true);
        Assert.Equal(44, viewModel.TouchGrabSize);
        window.Close();

        var (_, overridden, otherWindow) = ShowCanvas(touch: true, grabSizeOverride: 60);
        Assert.Equal(60, overridden.TouchGrabSize);
        otherWindow.Close();
    });

    [Fact]
    public void TheGrabAreaForAFinger_IsLargerThanTheDrawnHandle()
    {
        var (_, touch) = ReadSizeKeys();
        var root = XDocument.Load(RepoPaths.Combine(ThemeFile)).Root!;
        var grab = double.Parse(root.Elements().Single(e => (string?)e.Attribute(Xaml + "Key") == "TouchHandleGrabSize").Value, System.Globalization.CultureInfo.InvariantCulture);
        var drawn = double.Parse(touch["SelectionHandleSize"].Split(':')[1], System.Globalization.CultureInfo.InvariantCulture);

        Assert.True(grab > drawn);
        Assert.True(grab >= 44, "A fingertip needs about 44 dp.");
    }
}
