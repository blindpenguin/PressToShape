using System.Linq;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Desktop;
using PressToShape.App.Views.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public sealed class StatusBarSnapViewTests
{
    private static (EditorState State, Window Window, Control View) Show(bool tablet)
    {
        var state = new EditorState();
        var bar = new StatusBarViewModel(state, FakeLocalizer.Create(), () => { });
        Control view = tablet ? new TabletStatusBarView { DataContext = bar } : new StatusBarView { DataContext = bar };
        var window = new Window { Width = 1280, Height = 120, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (state, window, view);
    }

    private static ToggleButton Toggle(Control view, string name) =>
        view.GetVisualDescendants().OfType<ToggleButton>().Single(t => AutomationProperties.GetName(t) == name);

    private static TextBox SpacingBox(Control view) => view.GetVisualDescendants().OfType<TextBox>().Single(t => t.Name == "GridSpacingBox");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Toggles_ReflectTheStateAndChangeIt(bool tablet) => HeadlessUi.Run(() =>
    {
        var (state, window, view) = Show(tablet);
        var grid = Toggle(view, "Show Grid");
        var snapGrid = Toggle(view, "Snap to Grid");
        var snapObjects = Toggle(view, "Snap to Objects");
        Assert.All([grid, snapGrid, snapObjects], t => Assert.False(t.IsChecked));

        state.Snapping.ShowGrid = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(grid.IsChecked);

        snapGrid.IsChecked = true;
        snapObjects.IsChecked = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(state.Snapping.SnapToGrid);
        Assert.True(state.Snapping.SnapToObjects);

        grid.IsChecked = false;
        Dispatcher.UIThread.RunJobs();
        Assert.False(state.Snapping.ShowGrid);
        window.Close();
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpacingBox_ShowsTheSpacing_AndAppliesAValueOnEnter(bool tablet) => HeadlessUi.Run(() =>
    {
        var (state, window, view) = Show(tablet);
        var box = SpacingBox(view);
        Assert.Equal("10", box.Text);

        box.Focus();
        box.Text = "25";
        box.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter, Source = box });
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(25, state.Snapping.GridSpacing);
        window.Close();
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpacingBox_InvalidValue_ComesBackToTheOldOne(bool tablet) => HeadlessUi.Run(() =>
    {
        var (state, window, view) = Show(tablet);
        var box = SpacingBox(view);

        box.Focus();
        box.Text = "5000";
        box.RaiseEvent(new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = Key.Enter, Source = box });
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(10, state.Snapping.GridSpacing);
        Assert.Equal("10", box.Text);
        window.Close();
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GuideToggles_ReflectTheStateAndChangeIt(bool tablet) => HeadlessUi.Run(() =>
    {
        var (state, window, view) = Show(tablet);
        var smart = Toggle(view, "Smart Guides");
        var guides = Toggle(view, "Show Guides");
        Assert.False(smart.IsChecked);
        Assert.True(guides.IsChecked);

        state.Snapping.SmartGuides = true;
        Dispatcher.UIThread.RunJobs();
        Assert.True(smart.IsChecked);

        guides.IsChecked = false;
        smart.IsChecked = false;
        Dispatcher.UIThread.RunJobs();

        Assert.False(state.Snapping.ShowGuides);
        Assert.False(state.Snapping.SmartGuides);
        window.Close();
    });
}
