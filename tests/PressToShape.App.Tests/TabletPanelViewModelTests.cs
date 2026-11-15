using System;
using System.Collections.Generic;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class TabletPanelViewModelTests
{
    private static TabletPanelViewModel Make(out TestShell shell, Dictionary<string, string>? german = null)
    {
        shell = new TestShell(FakeLocalizer.Create(german: german));
        return new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
    }

    [Fact]
    public void New_IsClosedAndHasNoContent()
    {
        var panel = Make(out _);

        Assert.False(panel.IsOpen);
        Assert.Null(panel.OpenTab);
        Assert.Null(panel.Content);
    }

    [Fact]
    public void Toggle_WhenClosed_OpensTheTab()
    {
        var panel = Make(out var shell);

        panel.Toggle(InspectorTab.Layers);

        Assert.True(panel.IsOpen);
        Assert.Equal(InspectorTab.Layers, panel.OpenTab);
        Assert.Same(shell.Inspector.Layers, panel.Content);
        Assert.Equal(InspectorTab.Layers, shell.Inspector.SelectedTab);
    }

    [Fact]
    public void Toggle_TheOpenTab_ClosesThePanel()
    {
        var panel = Make(out _);
        panel.Toggle(InspectorTab.Properties);

        panel.Toggle(InspectorTab.Properties);

        Assert.False(panel.IsOpen);
        Assert.Null(panel.Content);
    }

    [Fact]
    public void Toggle_AnotherTab_SwitchesTheContentAndStaysOpen()
    {
        var panel = Make(out var shell);
        panel.Toggle(InspectorTab.Properties);

        panel.Toggle(InspectorTab.History);

        Assert.True(panel.IsOpen);
        Assert.Same(shell.Inspector.History, panel.Content);
    }

    [Fact]
    public void Toggle_Assets_IsIgnored()
    {
        var panel = Make(out _);

        panel.Toggle(InspectorTab.Assets);

        Assert.False(panel.IsOpen);
    }

    [Fact]
    public void Toggle_Assets_WhileOpen_KeepsTheOpenTab()
    {
        var panel = Make(out _);
        panel.Toggle(InspectorTab.Layers);

        panel.Toggle(InspectorTab.Assets);

        Assert.Equal(InspectorTab.Layers, panel.OpenTab);
    }

    [Fact]
    public void Open_WhenClosed_OpensTheTab()
    {
        var panel = Make(out var shell);

        panel.Open(InspectorTab.Properties);

        Assert.True(panel.IsOpen);
        Assert.Same(shell.Inspector.Properties, panel.Content);
        Assert.Equal(InspectorTab.Properties, shell.Inspector.SelectedTab);
    }

    [Fact]
    public void Open_AnotherTab_Switches()
    {
        var panel = Make(out var shell);
        panel.Toggle(InspectorTab.History);

        panel.Open(InspectorTab.Properties);

        Assert.Same(shell.Inspector.Properties, panel.Content);
    }

    [Fact]
    public void Open_TheOpenTab_KeepsItOpen()
    {
        var panel = Make(out _);
        panel.Open(InspectorTab.Properties);

        panel.Open(InspectorTab.Properties);

        Assert.True(panel.IsOpen);
        Assert.Equal(InspectorTab.Properties, panel.OpenTab);
    }

    [Fact]
    public void Open_Assets_IsIgnored()
    {
        var panel = Make(out _);

        panel.Open(InspectorTab.Assets);

        Assert.False(panel.IsOpen);
    }

    [Fact]
    public void Title_FollowsTheTabAndTheLanguage()
    {
        var panel = Make(out var shell, new Dictionary<string, string> { ["Layers"] = "Ebenen" });
        Assert.Equal(string.Empty, panel.Title);

        panel.Toggle(InspectorTab.Layers);
        Assert.Equal("Layers", panel.Title);

        shell.LocalizerForTests.SetLanguage("de");
        Assert.Equal("Ebenen", panel.Title);
    }

    [Fact]
    public void PropertyChanged_IsRaisedForOpenStateContentAndTitle()
    {
        var panel = Make(out _);
        var changed = new List<string?>();
        panel.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        panel.Toggle(InspectorTab.History);

        Assert.Contains(nameof(TabletPanelViewModel.IsOpen), changed);
        Assert.Contains(nameof(TabletPanelViewModel.Content), changed);
        Assert.Contains(nameof(TabletPanelViewModel.Title), changed);
    }

    [Fact]
    public void Close_AppliesAFieldThatIsStillBeingTypedIn()
    {
        var panel = Make(out var shell);
        var rectangle = shell.AddAndSelectRectangle(x: 100);
        panel.Toggle(InspectorTab.Properties);
        var x = shell.Inspector.Properties.X;
        x.IsEditing = true;
        x.Text = "150";

        panel.Close();

        Assert.False(panel.IsOpen);
        Assert.False(x.IsEditing);
        Assert.Equal(150, ((Core.Shapes.RectangleShape)shell.State.History.Current.FindShape(rectangle.Id)!.Shape).X);
    }

    [Fact]
    public void Close_WhenNothingIsBeingTypedIn_ChangesNothing()
    {
        var panel = Make(out var shell);
        shell.AddAndSelectRectangle(x: 100);
        panel.Toggle(InspectorTab.Properties);
        var steps = shell.State.History.CurrentIndex;

        panel.Close();

        Assert.Equal(steps, shell.State.History.CurrentIndex);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var shell = new TestShell();

        Assert.Throws<ArgumentNullException>(() => new TabletPanelViewModel(null!, shell.LocalizerForTests));
        Assert.Throws<ArgumentNullException>(() => new TabletPanelViewModel(shell.Inspector, null!));
    }
}
