using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Desktop;
using PressToShape.App.Views.Editor;
using PressToShape.App.Views.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

// What bindings and view models cannot show about the settings: that the views build, that the page follows the category, that the error shows.
[Collection(HeadlessUi.CollectionName)]
public sealed class SettingsViewHeadlessTests
{
    private static SettingsViewModel NewSettings() =>
        new(SnapPreferences.InMemory(), FakeLocalizer.Create(), CultureInfo.GetCultureInfo("en"), defaultShowRulers: false);

    private static (T View, Window Window) Show<T>(T view, bool touch = false) where T : Control
    {
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 800, Height = 480, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (view, window);
    }

    private static List<T> Visible<T>(Visual root) where T : Control =>
        root.GetVisualDescendants().OfType<T>().Where(c => c.IsEffectivelyVisible).ToList();

    private static TextBlock? ErrorText(Visual root) =>
        root.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Classes.Contains("error"));

    private static Button ButtonWithText(Visual root, string text) =>
        root.GetVisualDescendants().OfType<Button>().First(b => b.Content as string == text);

    // ---- desktop ----

    [Fact]
    public void SettingsView_ShowsTheThreeCategoriesAndTheGeneralPage() => HeadlessUi.Run(() =>
    {
        var (view, window) = Show(new SettingsView { DataContext = NewSettings() });

        var list = view.GetVisualDescendants().OfType<ListBox>().First(l => AutomationProperties.GetName(l) == "Categories");
        Assert.Equal(3, list.ItemCount);
        Assert.NotEmpty(Visible<GeneralSettingsPageView>(view));
        Assert.Empty(Visible<CanvasSettingsPageView>(view));

        window.Close();
    });

    [Fact]
    public void SettingsView_SelectingACategory_ShowsItsPageAndKeepsTheEnteredValues() => HeadlessUi.Run(() =>
    {
        var settings = NewSettings();
        var (view, window) = Show(new SettingsView { DataContext = settings });

        settings.SelectedCategory = settings.Categories[1];
        Dispatcher.UIThread.RunJobs();
        Assert.NotEmpty(Visible<CanvasSettingsPageView>(view));
        Assert.Empty(Visible<GeneralSettingsPageView>(view));

        settings.ShowGrid = true;
        settings.SelectedCategory = settings.Categories[2];
        Dispatcher.UIThread.RunJobs();
        Assert.NotEmpty(Visible<SnappingSettingsPageView>(view));

        settings.SelectedCategory = settings.Categories[1];
        Dispatcher.UIThread.RunJobs();
        var showGrid = Visible<CheckBox>(view).First(c => c.Content as string == "Show grid");
        Assert.True(showGrid.IsChecked);

        window.Close();
    });

    [Fact]
    public void SettingsView_InvalidSpacing_ShowsTheErrorAndDisablesOk() => HeadlessUi.Run(() =>
    {
        var settings = NewSettings();
        settings.SelectedCategory = settings.Categories[1];
        var (view, window) = Show(new SettingsView { DataContext = settings });
        Assert.False(ErrorText(view)!.IsEffectivelyVisible);
        Assert.True(ButtonWithText(view, "OK").IsEffectivelyEnabled);

        settings.GridSpacingText = "abc";
        Dispatcher.UIThread.RunJobs();

        Assert.True(ErrorText(view)!.IsEffectivelyVisible);
        Assert.Equal("Enter a number from 1 to 1000.", ErrorText(view)!.Text);
        Assert.False(ButtonWithText(view, "OK").IsEffectivelyEnabled);

        settings.GridSpacingText = "20";
        Dispatcher.UIThread.RunJobs();

        Assert.False(ErrorText(view)!.IsEffectivelyVisible);
        Assert.True(ButtonWithText(view, "OK").IsEffectivelyEnabled);

        window.Close();
    });

    [Fact]
    public void SettingsWindow_Cancel_ClosesTheWindow() => HeadlessUi.Run(() =>
    {
        var settings = NewSettings();
        var window = new SettingsWindow { DataContext = settings };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Assert.True(window.IsVisible);

        settings.CancelCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.False(window.IsVisible);
    });

    // ---- tablet ----

    [Fact]
    public void TabletSettingsView_ShowsTheListFirstThenThePageThenTheList() => HeadlessUi.Run(() =>
    {
        var settings = NewSettings();
        var (view, window) = Show(new TabletSettingsView { DataContext = settings }, touch: true);

        Assert.Empty(Visible<CanvasSettingsPageView>(view));
        Assert.Equal(["General", "Canvas", "Snapping"], Visible<TextBlock>(view).Select(t => t.Text).Where(t => t is "General" or "Canvas" or "Snapping"));
        Assert.DoesNotContain(Visible<Button>(view), b => AutomationProperties.GetName(b) == "Back");

        settings.OpenCategoryCommand.Execute(settings.Categories[1]);
        Dispatcher.UIThread.RunJobs();
        Assert.NotEmpty(Visible<CanvasSettingsPageView>(view));
        Assert.Contains(Visible<Button>(view), b => AutomationProperties.GetName(b) == "Back");

        settings.BackCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();
        Assert.Empty(Visible<CanvasSettingsPageView>(view));

        window.Close();
    });

    [Fact]
    public void TabletSettingsView_TappingACategory_OpensItsPage() => HeadlessUi.Run(() =>
    {
        var settings = NewSettings();
        var (view, window) = Show(new TabletSettingsView { DataContext = settings }, touch: true);

        var canvasRow = Visible<Button>(view).First(b => b.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "Canvas"));
        canvasRow.Command!.Execute(canvasRow.CommandParameter);
        Dispatcher.UIThread.RunJobs();

        Assert.True(settings.IsPageOpen);
        Assert.Same(settings.Categories[1], settings.SelectedCategory);
        Assert.NotEmpty(Visible<CanvasSettingsPageView>(view));

        window.Close();
    });

    [Fact]
    public void TabletSettingsView_Buttons_HaveTouchSize() => HeadlessUi.Run(() =>
    {
        var settings = NewSettings();
        var (view, window) = Show(new TabletSettingsView { DataContext = settings }, touch: true);

        var small = Visible<Button>(view).Where(b => b.TemplatedParent is null && b.Bounds.Height < 48).Select(b => $"{b.Content} {b.Bounds.Height:0}").ToList();

        Assert.Empty(small);

        window.Close();
    });
}
