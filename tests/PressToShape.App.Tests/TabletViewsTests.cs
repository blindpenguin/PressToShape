using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Views.Tablet;
using Xunit;

namespace PressToShape.App.Tests;

// What bindings and view models cannot show about the tablet layout: that its views build with their view models and that their controls have touch sizes.
[Collection(HeadlessUi.CollectionName)]
public class TabletViewsTests
{
    private const double MinimumTouchTarget = 48;

    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static TabletShellViewModel CreateShell(PressToShape.App.Localization.ILocalizer? localizer = null)
    {
        localizer ??= FakeLocalizer.Create();
        var host = new MessageDialogHost();
        var services = new FileServices(new MemorySettingsStore(), new FakeFileStore(), new FakeFileDialogs(), new OverlayMessageDialogs(host, localizer));
        return new TabletShellViewModel(new NoShutdown(), localizer, services, host);
    }

    private static (T View, Window Window) Show<T>(T view, bool touch, double width = 1280, double height = 400) where T : Control
    {
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = width, Height = height, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    private static T Named<T>(Visual root, string automationName) where T : Control =>
        root.GetVisualDescendants().OfType<T>().First(c => AutomationProperties.GetName(c) == automationName);

    // The controls of the view itself. Parts inside the template of another control are not counted.
    private static List<Control> InteractiveControls(Visual view) => view.GetVisualDescendants()
        .OfType<Control>()
        .Where(c => c is Button or TextBox or CheckBox or Slider && c.TemplatedParent is null && c.IsEffectivelyVisible)
        .ToList();

    private static string Describe(Control c) => $"{c.GetType().Name} '{AutomationProperties.GetName(c)}' {c.Bounds.Width:0}x{c.Bounds.Height:0}";

    // ---- toolbar ----

    [Fact]
    public void Toolbar_BuildsAndShowsTheTitleAndTheZoom() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: false);

        var texts = view.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("Untitled", texts);
        Assert.Equal("100%", Assert.IsType<string>(view.GetVisualDescendants().OfType<DropDownButton>().Single().Content));

        window.Close();
    });

    [Fact]
    public void Toolbar_FollowsTheDocument_StarUndoAndZoom() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: false);
        var undo = Named<Button>(view, "Undo");
        Assert.False(undo.IsEnabled);

        shell.State.Edit(
            PressToShape.Core.History.HistoryStepType.DrawRectangle,
            d => d.AddShape(shell.State.ActiveLayerId, new PressToShape.Core.Shapes.RectangleShape { Width = 5, Height = 5 }));
        Dispatcher.UIThread.RunJobs();

        Assert.True(undo.IsEnabled);
        Assert.Contains("* Untitled", view.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text));

        Named<Button>(view, "Zoom in").Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal("150%", view.GetVisualDescendants().OfType<DropDownButton>().Single().Content);

        window.Close();
    });

    [Fact]
    public void Toolbar_TheButtonsHaveAccessibleNames() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: false);

        foreach (var name in new[] { "Undo", "Redo", "Duplicate", "Delete", "Zoom out", "Zoom in", "Fit artboard in window", "More options" })
        {
            Assert.NotNull(Named<Button>(view, name));
        }

        Assert.NotNull(Named<DropDownButton>(view, "Zoom level"));

        window.Close();
    });

    [Fact]
    public void Toolbar_WithTheTouchSizes_EveryControlIsAtLeast48() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);

        var controls = InteractiveControls(view);
        Assert.Equal(12, controls.Count); // undo, redo, duplicate, delete, three panel icons, zoom out, zoom drop-down, zoom in, fit, overflow
        var tooSmall = controls.Where(c => c.Bounds.Width < MinimumTouchTarget || c.Bounds.Height < MinimumTouchTarget).Select(Describe).ToList();
        Assert.True(tooSmall.Count == 0, "Controls under 48 dp: " + string.Join(", ", tooSmall));

        window.Close();
    });

    [Fact]
    public void Toolbar_TheOverflowButton_HasTheMenuOfTheViewModel() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);

        var button = Named<Button>(view, "More options");
        var flyout = Assert.IsType<MenuFlyout>(button.Flyout);
        Assert.Same(shell.OverflowMenu.Items, flyout.ItemsSource);

        flyout.ShowAt(button);
        Dispatcher.UIThread.RunJobs();
        Assert.True(flyout.IsOpen);
        flyout.Hide();

        window.Close();
    });

    [Fact]
    public void Toolbar_ShowsTheGermanTitleAfterALanguageChange() => HeadlessUi.Run(() =>
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Untitled"] = "Unbenannt" });
        var shell = CreateShell(localizer);
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: false);

        localizer.SetLanguage("de");
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("Unbenannt", view.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text));

        window.Close();
    });

    [Fact]
    public void Toolbar_HasThePanelIconsInOrder_AndTheOpenOneIsChecked() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);
        var icons = new[] { "Properties", "Layers", "History" }.Select(n => Named<ToggleButton>(view, n)).ToList();
        var lefts = icons.Select(i => i.TranslatePoint(new Point(0, 0), window)!.Value.X).ToList();
        Assert.Equal(lefts.OrderBy(x => x), lefts);
        Assert.All(icons, i => Assert.NotEqual(true, i.IsChecked));

        icons[1].Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal([false, true, false], icons.Select(i => i.IsChecked == true));

        icons[2].Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Equal([false, false, true], icons.Select(i => i.IsChecked == true));

        window.Close();
    });

    [Fact]
    public void Toolbar_DuplicateAndDelete_AreDisabledWithoutASelection_AndActOnIt() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);
        Assert.False(Named<Button>(view, "Duplicate").IsEffectivelyEnabled);
        Assert.False(Named<Button>(view, "Delete").IsEffectivelyEnabled);

        var shape = new PressToShape.Core.Shapes.RectangleShape { Width = 20, Height = 20 };
        shell.State.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(shell.State.ActiveLayerId, shape));
        shell.State.SelectOnly(shape.Id);
        Dispatcher.UIThread.RunJobs();
        Assert.True(Named<Button>(view, "Duplicate").IsEffectivelyEnabled);

        Named<Button>(view, "Duplicate").Command!.Execute(null);
        Assert.Equal(2, shell.State.Document.Layers.SelectMany(l => l.Shapes).Count());
        Named<Button>(view, "Delete").Command!.Execute(null);
        Assert.Single(shell.State.Document.Layers.SelectMany(l => l.Shapes));

        window.Close();
    });

    [Theory]
    [InlineData(1280)]
    [InlineData(900)]
    public void Toolbar_FitsInOneRow_AtTheTabletWidths(int width) => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true, width: width, height: 200);

        var controls = InteractiveControls(view);
        var rights = controls.Select(c => c.TranslatePoint(new Point(c.Bounds.Width, 0), window)!.Value.X).ToList();
        var tops = controls.Select(c => Math.Round(c.TranslatePoint(new Point(0, 0), window)!.Value.Y)).Distinct().ToList();
        Assert.True(rights.Max() <= width, $"A control ends at {rights.Max()}, past {width}.");
        Assert.Single(tops);
        var name = view.GetVisualDescendants().OfType<TextBlock>().First(t => t.Text == "Untitled");
        Assert.Equal(Avalonia.Media.TextTrimming.CharacterEllipsis, name.TextTrimming);

        window.Close();
    });

    [Fact]
    public void Toolbar_MenuClosed_IsRaisedWhenAMenuCloses() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);
        var closed = 0;
        view.MenuClosed += (_, _) => closed++;
        var button = Named<Button>(view, "More options");

        button.Flyout!.ShowAt(button);
        Dispatcher.UIThread.RunJobs();
        button.Flyout.Hide();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, closed);

        window.Close();
    });

    [Fact]
    public void Toolbar_TryCloseMenu_ClosesTheOpenMenu_AndSaysSo() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);
        Assert.False(view.TryCloseMenu());

        var button = Named<Button>(view, "More options");
        button.Flyout!.ShowAt(button);
        Dispatcher.UIThread.RunJobs();
        Assert.True(button.Flyout.IsOpen);

        Assert.True(view.TryCloseMenu());
        Dispatcher.UIThread.RunJobs();
        Assert.False(button.Flyout.IsOpen);
        Assert.False(view.TryCloseMenu());

        window.Close();
    });

    // ---- floating panel ----

    private static (TabletPanelView View, Window Window) ShowPanel(TabletShellViewModel shell, bool touch) =>
        Show(new TabletPanelView { DataContext = shell.Panel }, touch, width: 360, height: 900);

    [Fact]
    public void Panel_WhileClosed_IsHiddenAndShowsNoContent() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowPanel(shell, touch: false);

        Assert.False(view.IsVisible);
        Assert.Empty(view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.PropertiesView>());

        window.Close();
    });

    [Fact]
    public void Panel_Open_ShowsTheTitleAndTheContentOfTheTab() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowPanel(shell, touch: false);

        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Layers);
        Dispatcher.UIThread.RunJobs();

        Assert.True(view.IsVisible);
        Assert.Contains("Layers", view.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text));
        Assert.Single(view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.LayersView>());

        window.Close();
    });

    [Fact]
    public void Panel_SwitchingTheTab_ShowsTheOtherContent() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowPanel(shell, touch: false);

        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Properties);
        Dispatcher.UIThread.RunJobs();
        Assert.Single(view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.PropertiesView>());

        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.History);
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.PropertiesView>());
        Assert.Single(view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.HistoryView>());

        shell.Panel.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.False(view.IsVisible);

        window.Close();
    });

    [Fact]
    public void Panel_ShowsTheSameInspectorAsTheShell() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowPanel(shell, touch: false);
        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Properties);
        Dispatcher.UIThread.RunJobs();

        var properties = view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.PropertiesView>().Single();

        Assert.Same(shell.Inspector.Properties, properties.DataContext);

        window.Close();
    });

    [Fact]
    public void Panel_HasTheWidthOfTheTheme() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowPanel(shell, touch: true);
        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Properties);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(340, view.Bounds.Width);

        window.Close();
    });

    [Fact]
    public void Panel_WithTheTouchSizes_TheContentIsTouchSized() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowPanel(shell, touch: true);
        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Properties);
        Dispatcher.UIThread.RunJobs();

        var tooSmall = InteractiveControls(view).Where(c => c.Bounds.Height < MinimumTouchTarget).Select(Describe).ToList();

        Assert.True(tooSmall.Count == 0, "Controls under 48 dp high: " + string.Join(", ", tooSmall));

        window.Close();
    });

    // ---- the whole layout ----

    private static (TabletShellView View, Window Window) ShowShell(TabletShellViewModel shell, double width = 1280, double height = 720)
    {
        var view = new TabletShellView { DataContext = shell };
        var window = new Window { Width = width, Height = height, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    private static T Only<T>(Visual root) where T : Control => root.GetVisualDescendants().OfType<T>().Single();

    [Fact]
    public void Shell_ShowsEveryRegion_WithItsViewModel() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell);

        Assert.Same(shell.Toolbar, Only<TabletToolbarView>(view).DataContext);
        Assert.Same(shell.Column, Only<TabletToolColumnView>(view).DataContext);
        Assert.Same(shell.Canvas, Only<PressToShape.App.Views.Editor.CanvasAreaView>(view).DataContext);
        Assert.Same(shell.Panel, Only<TabletPanelView>(view).DataContext);

        window.Close();
    });

    [Fact]
    public void Shell_HasNoRulers_NoMenuBar_NoStatusBar_AndNoBarUnderTheCanvas() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell);

        Assert.All(view.GetVisualDescendants().OfType<RulerControl>(), r => Assert.False(r.IsEffectivelyVisible));
        Assert.Empty(view.GetVisualDescendants().OfType<Menu>());
        Assert.Empty(view.GetVisualDescendants().OfType<PressToShape.App.Views.Desktop.StatusBarView>());
        Assert.DoesNotContain(view.GetVisualDescendants().OfType<TextBox>(), t => t.IsEffectivelyVisible && t.Tag is not null);

        window.Close();
    });

    [Fact]
    public void Shell_IsTouchSized_WithoutAnyHelp() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell);
        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Properties);
        Dispatcher.UIThread.RunJobs();

        Assert.Contains("touch", view.Classes);
        Assert.All(GroupButtons(view), b => Assert.Equal(48, b.Bounds.Width));
        var toolbarControls = InteractiveControls(Only<TabletToolbarView>(view));
        Assert.All(toolbarControls, c => Assert.True(c.Bounds.Height >= MinimumTouchTarget, Describe(c)));
        var panelControls = InteractiveControls(Only<TabletPanelView>(view));
        Assert.All(panelControls, c => Assert.True(c.Bounds.Height >= MinimumTouchTarget, Describe(c)));

        window.Close();
    });

    private static Point Origin(Visual c, Visual window) => c.TranslatePoint(new Point(0, 0), window)!.Value;

    [Fact]
    public void Shell_Layout_ToolbarOnTop_ToolColumnLeft_CanvasFillsTheRest() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell, 1280, 720);

        var toolbar = Only<TabletToolbarView>(view);
        var column = Only<TabletToolColumnView>(view);
        var canvas = Only<PressToShape.App.Views.Editor.CanvasAreaView>(view);

        Assert.Equal(0, Origin(toolbar, window).Y);
        Assert.Equal(toolbar.Bounds.Height, Origin(column, window).Y, 0.5);
        Assert.Equal(toolbar.Bounds.Height, Origin(canvas, window).Y, 0.5);
        Assert.Equal(column.Bounds.Width, Origin(canvas, window).X, 0.5);
        Assert.Equal(1280, column.Bounds.Width + canvas.Bounds.Width, 0.5);
        Assert.Equal(720, Origin(canvas, window).Y + canvas.Bounds.Height, 0.5);

        window.Close();
    });

    [Fact]
    public void Shell_TheCanvas_IsAsWideAsTheLayoutMinusTheNarrowColumn() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell, 1280, 720);

        var column = Only<TabletToolColumnView>(view);
        var canvas = Only<PressToShape.App.Views.Editor.CanvasAreaView>(view);

        Assert.InRange(column.Bounds.Width, 56, 70);
        Assert.Equal(1280 - column.Bounds.Width, canvas.Bounds.Width, 0.5);

        window.Close();
    });

    [Fact]
    public void Shell_OpeningAndClosingThePanel_DoesNotChangeTheCanvas() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell, 1280, 720);
        var canvas = Only<PressToShape.App.Views.Editor.CanvasAreaView>(view);
        var panel = Only<TabletPanelView>(view);
        var before = (canvas.Bounds, Origin(canvas, window));
        Assert.False(panel.IsVisible);

        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Layers);
        Dispatcher.UIThread.RunJobs();

        Assert.True(panel.IsEffectivelyVisible);
        Assert.Equal(before, (canvas.Bounds, Origin(canvas, window)));
        Assert.InRange(panel.Bounds.Width, 260, 360);
        Assert.Equal(1280, Origin(panel, window).X + panel.Bounds.Width, 0.5);
        Assert.Equal(720, Origin(panel, window).Y + panel.Bounds.Height, 0.5);
        Assert.Equal(Origin(canvas, window).Y, Origin(panel, window).Y, 0.5);

        shell.Panel.Close();
        Dispatcher.UIThread.RunJobs();
        Assert.Equal(before, (canvas.Bounds, Origin(canvas, window)));

        window.Close();
    });

    private static Point CanvasPoint(Visual canvas, Visual window, double x, double y) => canvas.TranslatePoint(new Point(x, y), window)!.Value;

    [Fact]
    public void Shell_APressOnTheCanvas_ClosesThePanel_AndStillDrawsWithTheTool() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell, 1280, 720);
        var canvas = Only<PressToShape.App.Views.Editor.CanvasAreaView>(view);
        shell.State.ActiveTool = PressToShape.App.Workspace.Tool.Rectangle;
        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Layers);
        Dispatcher.UIThread.RunJobs();

        // Inside the artboard, which is centered in the viewport.
        var cx = canvas.Bounds.Width / 2;
        var cy = canvas.Bounds.Height / 2;
        window.MouseDown(CanvasPoint(canvas, window, cx - 60, cy - 40), Avalonia.Input.MouseButton.Left);
        Assert.False(shell.Panel.IsOpen);
        window.MouseMove(CanvasPoint(canvas, window, cx, cy));
        window.MouseMove(CanvasPoint(canvas, window, cx + 60, cy + 40));
        window.MouseUp(CanvasPoint(canvas, window, cx + 60, cy + 40), Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Single(shell.State.Document.Layers.SelectMany(l => l.Shapes));

        window.Close();
    });

    [Fact]
    public void Shell_APressOnAToolOrTheToolbar_KeepsThePanelOpen() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell, 1280, 720);
        shell.Panel.Toggle(PressToShape.App.Workspace.InspectorTab.Properties);
        Dispatcher.UIThread.RunJobs();

        var tool = GroupButtons(view)[3];
        window.MouseDown(tool.TranslatePoint(new Point(24, 24), window)!.Value, Avalonia.Input.MouseButton.Left);
        window.MouseUp(tool.TranslatePoint(new Point(24, 24), window)!.Value, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(shell.Panel.IsOpen);
        Assert.Equal(PressToShape.App.Workspace.Tool.Rectangle, shell.State.ActiveTool);

        window.Close();
    });

    [Fact]
    public void Shell_TheDialogOverlay_IsHidden_UntilADialogIsOpen_AndShowsItsTitleAndMessage() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell);
        var scrim = view.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "DialogScrim");
        Assert.False(scrim.IsVisible);

        var dialog = PressToShape.App.ViewModels.MessageDialogViewModel.ForError(FakeLocalizer.Create(), "Could not save “a”. Reason");
        var shown = shell.Dialogs.ShowAsync(dialog);
        Dispatcher.UIThread.RunJobs();

        Assert.True(scrim.IsVisible);
        var texts = scrim.GetVisualDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("Error", texts);
        Assert.Contains("Could not save “a”. Reason", texts);
        Assert.Single(scrim.GetVisualDescendants().OfType<PressToShape.App.Views.MessageDialogView>());

        dialog.Buttons.Single().Command.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.True(shown.IsCompleted);
        Assert.False(scrim.IsVisible);

        window.Close();
    });

    [Fact]
    public void Shell_TheDialogButtons_AreTouchSized() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = ShowShell(shell);
        var dialog = PressToShape.App.ViewModels.MessageDialogViewModel.ForUnsavedChanges(FakeLocalizer.Create(), "plan.p2s");
        _ = shell.Dialogs.ShowAsync(dialog);
        Dispatcher.UIThread.RunJobs();

        var buttons = view.GetVisualDescendants().OfType<PressToShape.App.Views.MessageDialogView>().Single().GetVisualDescendants().OfType<Button>()
            .Where(b => b.TemplatedParent is null).ToList();

        Assert.Equal(3, buttons.Count);
        Assert.All(buttons, b => Assert.True(b.Bounds.Height >= MinimumTouchTarget, Describe(b)));

        shell.Dialogs.Dismiss();
        window.Close();
    });

    // ---- tool column ----

    private static List<ToolGroupButton> GroupButtons(Visual view) => view.GetVisualDescendants().OfType<ToolGroupButton>().ToList();

    [Fact]
    public void ToolColumn_ShowsSixGroupButtonsInOrder_OnlyTheActiveOneChecked() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);

        var buttons = GroupButtons(view);

        Assert.Equal(["Select", "Pen", "Text", "Rectangle", "Eyedropper", "Hand"], buttons.Select(AutomationProperties.GetName));
        Assert.Equal([true, false, false, false, false, false], buttons.Select(b => b.IsChecked == true));
        var x = buttons.Select(b => Math.Round(b.TranslatePoint(new Point(0, 0), window)!.Value.X)).Distinct();
        Assert.Single(x);

        window.Close();
    });

    [Fact]
    public void ToolColumn_MarksOnlyTheGroups() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);

        // The icons are drawn with paths too: only the paths outside an icon are marks.
        var marks = view.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>()
            .Where(p => p.FindAncestorOfType<Material.Icons.Avalonia.MaterialIcon>() is null).ToList();

        Assert.Equal(6, marks.Count);
        Assert.Equal([true, true, false, true, false, true], marks.Select(m => m.IsVisible));

        window.Close();
    });

    [Fact]
    public void ToolColumn_ChangingTheToolFromOutside_MovesTheHighlightAndTheIcon() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);

        shell.State.ActiveTool = PressToShape.App.Workspace.Tool.Spiral;
        Dispatcher.UIThread.RunJobs();

        var buttons = GroupButtons(view);
        Assert.Equal([false, false, false, true, false, false], buttons.Select(b => b.IsChecked == true));
        Assert.Equal("Spiral", AutomationProperties.GetName(buttons[3]));

        window.Close();
    });

    [Fact]
    public void ToolColumn_ATapOnAGroupSelectsItsCurrentTool() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);

        GroupButtons(view)[3].Command!.Execute(null);

        Assert.Equal(PressToShape.App.Workspace.Tool.Rectangle, shell.State.ActiveTool);

        window.Close();
    });

    [Fact]
    public void ToolColumn_TheLongPressCommand_OpensTheFlyoutWithAllToolsOfTheGroup() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        var shapes = GroupButtons(view)[3];

        shapes.LongPressCommand!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        var popup = view.GetVisualDescendants().OfType<Popup>().Single(p => p.IsOpen);
        var entries = popup.Child!.GetVisualDescendants().OfType<ToggleButton>().ToList();
        Assert.Equal(["Rectangle", "Ellipse", "Star", "Spiral"], entries.Select(AutomationProperties.GetName));
        Assert.True(shell.Column.Groups[3].IsFlyoutOpen);

        window.Close();
    });

    [Fact]
    public void ToolColumn_ChoosingAnEntry_SelectsTheToolAndClosesTheFlyout() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        GroupButtons(view)[3].LongPressCommand!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        var popup = view.GetVisualDescendants().OfType<Popup>().Single(p => p.IsOpen);

        var star = popup.Child!.GetVisualDescendants().OfType<ToggleButton>().Single(b => AutomationProperties.GetName(b) == "Star");
        star.Command!.Execute(star.CommandParameter);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(PressToShape.App.Workspace.Tool.Star, shell.State.ActiveTool);
        Assert.False(shell.Column.Groups[3].IsFlyoutOpen);
        Assert.All(view.GetVisualDescendants().OfType<Popup>(), p => Assert.False(p.IsOpen));
        Assert.Equal("Star", AutomationProperties.GetName(GroupButtons(view)[3]));

        window.Close();
    });

    [Fact]
    public void ToolColumn_TappingTheActiveToolAgain_KeepsItHighlighted() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        var select = GroupButtons(view)[0];
        var center = select.TranslatePoint(new Point(24, 24), window)!.Value;

        window.MouseDown(center, Avalonia.Input.MouseButton.Left);
        window.MouseUp(center, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(PressToShape.App.Workspace.Tool.Select, shell.State.ActiveTool);
        Assert.True(select.IsChecked);

        window.Close();
    });

    [Fact]
    public void ToolColumn_ATapOnAnotherTool_StillMovesTheHighlightBothWays() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        var buttons = GroupButtons(view);
        Point Center(Control c) => c.TranslatePoint(new Point(24, 24), window)!.Value;

        window.MouseDown(Center(buttons[3]), Avalonia.Input.MouseButton.Left);
        window.MouseUp(Center(buttons[3]), Avalonia.Input.MouseButton.Left);
        window.MouseDown(Center(buttons[0]), Avalonia.Input.MouseButton.Left);
        window.MouseUp(Center(buttons[0]), Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal([true, false, false, false, false, false], buttons.Select(b => b.IsChecked == true));

        window.Close();
    });

    [Fact]
    public void ToolColumn_TappingTheActiveEntryOfAFlyoutAgain_KeepsItChecked() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        shell.State.ActiveTool = PressToShape.App.Workspace.Tool.Rectangle;
        GroupButtons(view)[3].LongPressCommand!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        var popup = view.GetVisualDescendants().OfType<Popup>().Single(p => p.IsOpen);
        var rectangle = popup.Child!.GetVisualDescendants().OfType<ToggleButton>().Single(b => AutomationProperties.GetName(b) == "Rectangle");
        Assert.True(rectangle.IsChecked);

        rectangle.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(PressToShape.App.Workspace.Tool.Rectangle, shell.State.ActiveTool);
        Assert.True(rectangle.IsChecked);
        window.Close();
    });

    [Fact]
    public void Toolbar_ThePanelIcon_FollowsThePanelAlsoWhenThePanelClosesByItself() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolbarView { DataContext = shell.Toolbar }, touch: true);
        var icon = Named<ToggleButton>(view, "Layers");
        var center = icon.TranslatePoint(new Point(24, 24), window)!.Value;

        window.MouseDown(center, Avalonia.Input.MouseButton.Left);
        window.MouseUp(center, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        Assert.True(icon.IsChecked);

        shell.Panel.Close();
        Dispatcher.UIThread.RunJobs();

        Assert.False(icon.IsChecked);
        window.Close();
    });

    [Fact]
    public void ToolColumn_ASingleToolGroup_OpensNoFlyout() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);

        GroupButtons(view)[2].LongPressCommand!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.All(view.GetVisualDescendants().OfType<Popup>(), p => Assert.False(p.IsOpen));

        window.Close();
    });

    [Fact]
    public void ToolColumn_WithTheTouchSizes_EveryButtonIsAtLeast48_AlsoInTheFlyout() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        GroupButtons(view)[3].LongPressCommand!.Execute(null);
        Dispatcher.UIThread.RunJobs();
        var popup = view.GetVisualDescendants().OfType<Popup>().Single(p => p.IsOpen);

        var controls = InteractiveControls(view).Concat(InteractiveControls(popup.Child!)).ToList();

        Assert.NotEmpty(controls);
        var small = controls.Where(c => c.Bounds.Width < MinimumTouchTarget || c.Bounds.Height < MinimumTouchTarget).Select(Describe).ToList();
        Assert.True(small.Count == 0, "Too small: " + string.Join("; ", small));

        window.Close();
    });

    [Fact]
    public void ToolColumn_FitsTheHeightOfATablet() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 700);

        var lastButton = GroupButtons(view).Last();
        var bottom = lastButton.TranslatePoint(new Point(0, lastButton.Bounds.Height), window)!.Value.Y;
        var swatch = view.GetVisualDescendants().OfType<TabletColorSwatchView>().Single();
        var swatchTop = swatch.TranslatePoint(new Point(0, 0), window)!.Value.Y;

        Assert.True(bottom <= swatchTop, $"The tools end at {bottom} but the swatch starts at {swatchTop}.");

        window.Close();
    });

    [Fact]
    public void ToolColumn_HasNoSwapOrResetButton_AndOneSwatchOfAtLeast48() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);

        Assert.Empty(view.GetVisualDescendants().OfType<PressToShape.App.Views.Editor.SwatchesView>());
        var swatch = view.GetVisualDescendants().OfType<TabletColorSwatchView>().Single();
        var button = swatch.GetVisualDescendants().OfType<Button>().Single();
        Assert.True(button.Bounds.Width >= MinimumTouchTarget && button.Bounds.Height >= MinimumTouchTarget, Describe(button));
        Assert.Equal("Fill and stroke color", AutomationProperties.GetName(button));

        window.Close();
    });

    [Fact]
    public void ToolColumn_IsNarrow_AndEverythingIsCentered() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 200, 800);

        Assert.InRange(view.DesiredSize.Width, 56, 70);
        var centers = GroupButtons(view).Cast<Control>()
            .Concat(view.GetVisualDescendants().OfType<TabletColorSwatchView>().Single().GetVisualDescendants().OfType<Button>())
            .Select(c => Math.Round(c.TranslatePoint(new Point(c.Bounds.Width / 2, 0), window)!.Value.X)).Distinct().ToList();
        Assert.Single(centers);

        // The mark of a group is in the corner of its own button.
        var button = GroupButtons(view)[0];
        var mark = view.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>().First(p => p.FindAncestorOfType<Material.Icons.Avalonia.MaterialIcon>() is null);
        var markRight = mark.TranslatePoint(new Point(mark.Bounds.Width, 0), window)!.Value.X;
        var buttonRight = button.TranslatePoint(new Point(button.Bounds.Width, 0), window)!.Value.X;
        Assert.InRange(buttonRight - markRight, 0, 16);

        window.Close();
    });

    private static (Avalonia.Controls.Border Ring, Avalonia.Controls.Border Fill) VisibleParts(TabletColorSwatchView swatch)
    {
        var borders = swatch.GetVisualDescendants().OfType<Avalonia.Controls.Border>().Where(b => b.IsVisible && b.CornerRadius.TopLeft > 0).ToList();
        return (borders.First(b => b.CornerRadius.TopLeft == 6), borders.First(b => b.CornerRadius.TopLeft == 3));
    }

    private static Avalonia.Media.Color? BackgroundColor(Avalonia.Controls.Border b) => (b.Background as Avalonia.Media.ISolidColorBrush)?.Color;

    [Fact]
    public void ColorSwatch_ShowsTheFillInTheMiddleAndTheStrokeAsRing_AndFollowsChanges() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        var swatch = view.GetVisualDescendants().OfType<TabletColorSwatchView>().Single();
        shell.State.DefaultAppearance = PressToShape.Core.Shapes.Appearance.Default with
        {
            Fill = PressToShape.Core.Shapes.Color.FromRgb(255, 0, 0),
            Stroke = PressToShape.Core.Shapes.Color.FromRgb(0, 0, 255),
        };
        Dispatcher.UIThread.RunJobs();

        var (ring, fill) = VisibleParts(swatch);
        Assert.Equal(Avalonia.Media.Color.FromRgb(255, 0, 0), BackgroundColor(fill));
        Assert.Equal(Avalonia.Media.Color.FromRgb(0, 0, 255), BackgroundColor(ring));

        shell.State.DefaultAppearance = shell.State.DefaultAppearance with { Fill = PressToShape.Core.Shapes.Color.FromRgb(0, 255, 0) };
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(Avalonia.Media.Color.FromRgb(0, 255, 0), BackgroundColor(VisibleParts(swatch).Fill));

        window.Close();
    });

    [Fact]
    public void ColorSwatch_WithoutFillOrStroke_ShowsEmptyOutlinedParts() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        var swatch = view.GetVisualDescendants().OfType<TabletColorSwatchView>().Single();

        shell.State.DefaultAppearance = PressToShape.Core.Shapes.Appearance.Default with { Fill = null, Stroke = null };
        Dispatcher.UIThread.RunJobs();

        var (ring, fill) = VisibleParts(swatch);
        Assert.Null(BackgroundColor(ring));
        Assert.NotNull(ring.BorderBrush);
        Assert.NotNull(fill.BorderBrush);
        Assert.True(fill.Bounds.Width > 0 && ring.Bounds.Width > 0);

        window.Close();
    });

    [Fact]
    public void ColorSwatch_ATap_OpensThePropertiesPanel() => HeadlessUi.Run(() =>
    {
        var shell = CreateShell();
        var (view, window) = Show(new TabletToolColumnView { DataContext = shell.Column }, touch: true, 120, 800);
        var button = view.GetVisualDescendants().OfType<TabletColorSwatchView>().Single().GetVisualDescendants().OfType<Button>().Single();
        var center = button.TranslatePoint(new Point(24, 24), window)!.Value;

        window.MouseDown(center, Avalonia.Input.MouseButton.Left);
        window.MouseUp(center, Avalonia.Input.MouseButton.Left);
        Dispatcher.UIThread.RunJobs();

        Assert.True(shell.Panel.IsOpen);
        Assert.Equal(PressToShape.App.Workspace.InspectorTab.Properties, shell.Panel.OpenTab);

        window.Close();
    });
}
