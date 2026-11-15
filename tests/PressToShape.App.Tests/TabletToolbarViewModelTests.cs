using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class TabletToolbarViewModelTests
{
    private static (TestShell Shell, TabletToolbarViewModel Toolbar) Create(TestShell? shell = null)
    {
        shell ??= new TestShell();
        var menu = new OverflowMenuViewModel(shell.LocalizerForTests, shell.FileCommandsForTests, shell.EditCommandsForTests);
        var zoom = new PressToShape.App.ViewModels.Editor.ZoomViewModel(shell.State, shell.LocalizerForTests, shell.FitToWindow);
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
        return (shell, new TabletToolbarViewModel(shell.LocalizerForTests, shell, shell.UndoCommand, shell.RedoCommand, zoom, menu, panel));
    }

    [Fact]
    public void NewDocument_IsShownAsUntitled_WithoutAStar()
    {
        var (_, toolbar) = Create();

        Assert.Equal("Untitled", toolbar.DocumentTitle);
    }

    [Fact]
    public void AnEdit_PutsAStarInFront_AndNotifies()
    {
        var (shell, toolbar) = Create();
        var raised = new List<string?>();
        toolbar.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.AddRectangle();

        Assert.Equal("* Untitled", toolbar.DocumentTitle);
        Assert.Contains(nameof(TabletToolbarViewModel.DocumentTitle), raised);
    }

    [Fact]
    public async Task AfterSaveAs_TheTitleIsTheFileName_WithoutAStar()
    {
        var (shell, toolbar) = Create();
        shell.AddRectangle();
        shell.Dialogs.SaveAnswers.Enqueue("/work/plan.p2s");

        await shell.SaveAsCommand.ExecuteAsync(null);

        Assert.Equal("plan.p2s", toolbar.DocumentTitle);
    }

    [Fact]
    public void TheTitle_FollowsTheLanguage()
    {
        var localizer = PressToShape.App.Tests.FakeLocalizer.Create("en", new Dictionary<string, string> { ["Untitled"] = "Unbenannt" });
        var (_, toolbar) = Create(new TestShell(localizer));

        localizer.SetLanguage("de");

        Assert.Equal("Unbenannt", toolbar.DocumentTitle);
    }

    [Fact]
    public void UndoAndRedoButtons_AreNamed_AndDisabledWithoutHistory()
    {
        var (_, toolbar) = Create();

        Assert.Equal("Undo", toolbar.UndoButton.Name);
        Assert.Equal("Redo", toolbar.RedoButton.Name);
        Assert.False(toolbar.UndoButton.IsEnabled);
        Assert.False(toolbar.RedoButton.IsEnabled);
    }

    [Fact]
    public void UndoAndRedoButtons_FollowTheHistory_AndChangeTheDocument()
    {
        var (shell, toolbar) = Create();
        shell.AddRectangle();
        Assert.True(toolbar.UndoButton.IsEnabled);
        Assert.False(toolbar.RedoButton.IsEnabled);

        toolbar.UndoButton.Command!.Execute(null);

        Assert.Empty(shell.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.False(toolbar.UndoButton.IsEnabled);
        Assert.True(toolbar.RedoButton.IsEnabled);

        toolbar.RedoButton.Command!.Execute(null);

        Assert.Single(shell.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.True(toolbar.UndoButton.IsEnabled);
    }

    [Fact]
    public void UndoButtonChangesNotify_WhenTheHistoryChanges()
    {
        var (shell, toolbar) = Create();
        var raised = new List<string?>();
        toolbar.UndoButton.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.AddRectangle();

        Assert.Contains(nameof(PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel.IsEnabled), raised);
    }

    [Fact]
    public void Zoom_ShowsTheZoomOfTheEditor_AndItsCommandsWork()
    {
        var (shell, toolbar) = Create();
        Assert.Equal("100%", toolbar.Zoom.ZoomText);

        toolbar.Zoom.ZoomInCommand.Execute(null);

        Assert.Equal("150%", toolbar.Zoom.ZoomText);
        Assert.Equal(150, shell.State.ZoomPercent);
        Assert.Equal(PressToShape.App.Workspace.ZoomLevels.Presets, toolbar.Zoom.ZoomPresets.Select(p => p.Percent));
    }

    [Fact]
    public void TheOverflowMenu_IsTheOneThatWasGiven()
    {
        var (_, toolbar) = Create();

        Assert.NotEmpty(toolbar.OverflowMenu.Items);
    }

    [Fact]
    public void DuplicateAndDelete_AreDisabledWithoutSelection_AndNamed()
    {
        var (_, toolbar) = Create();

        Assert.Equal("Duplicate", toolbar.DuplicateButton.Name);
        Assert.Equal("Delete", toolbar.DeleteButton.Name);
        Assert.False(toolbar.DuplicateButton.IsEnabled);
        Assert.False(toolbar.DeleteButton.IsEnabled);
    }

    [Fact]
    public void Duplicate_WithASelection_AddsACopyAndSelectsIt()
    {
        var (shell, toolbar) = Create();
        shell.AddAndSelectRectangle();
        Assert.True(toolbar.DuplicateButton.IsEnabled);

        toolbar.DuplicateButton.Command!.Execute(null);

        Assert.Equal(2, shell.State.Document.Layers.SelectMany(l => l.Shapes).Count());
    }

    [Fact]
    public void Delete_WithASelection_RemovesItAsOneUndoStep()
    {
        var (shell, toolbar) = Create();
        shell.AddAndSelectRectangle();
        var steps = shell.State.History.CurrentIndex;

        toolbar.DeleteButton.Command!.Execute(null);

        Assert.Empty(shell.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.Equal(steps + 1, shell.State.History.CurrentIndex);
        Assert.False(toolbar.DeleteButton.IsEnabled);
    }

    [Fact]
    public void PanelButtons_AreProperties_Layers_History_InThatOrder()
    {
        var (_, toolbar) = Create();

        Assert.Equal([InspectorTab.Properties, InspectorTab.Layers, InspectorTab.History], toolbar.PanelButtons.Select(b => b.Tab));
        Assert.Equal(["Properties", "Layers", "History"], toolbar.PanelButtons.Select(b => b.Name));
    }

    [Fact]
    public void PanelButton_Toggle_OpensThePanel_AndMarksOnlyItActive()
    {
        var shell = new TestShell();
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
        var toolbar = CreateWith(shell, panel);

        toolbar.PanelButtons[1].ToggleCommand.Execute(null);

        Assert.True(panel.IsOpen);
        Assert.Equal([false, true, false], toolbar.PanelButtons.Select(b => b.IsActive));

        toolbar.PanelButtons[2].ToggleCommand.Execute(null);
        Assert.Equal([false, false, true], toolbar.PanelButtons.Select(b => b.IsActive));

        toolbar.PanelButtons[2].ToggleCommand.Execute(null);
        Assert.False(panel.IsOpen);
        Assert.All(toolbar.PanelButtons, b => Assert.False(b.IsActive));
    }

    [Fact]
    public void PanelButton_IsActive_NotifiesWhenThePanelChanges()
    {
        var shell = new TestShell();
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
        var toolbar = CreateWith(shell, panel);
        var raised = new List<string?>();
        toolbar.PanelButtons[0].PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        panel.Toggle(InspectorTab.Properties);

        Assert.Contains(nameof(PanelButtonViewModel.IsActive), raised);
    }

    [Fact]
    public void ButtonNames_FollowTheLanguage()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Layers"] = "Ebenen", ["Delete"] = "Löschen" });
        var (_, toolbar) = Create(new TestShell(localizer));

        localizer.SetLanguage("de");

        Assert.Equal("Ebenen", toolbar.PanelButtons[1].Name);
        Assert.Equal("Löschen", toolbar.DeleteButton.Name);
    }

    private static TabletToolbarViewModel CreateWith(TestShell shell, TabletPanelViewModel panel)
    {
        var menu = new OverflowMenuViewModel(shell.LocalizerForTests, shell.FileCommandsForTests, shell.EditCommandsForTests);
        var zoom = new PressToShape.App.ViewModels.Editor.ZoomViewModel(shell.State, shell.LocalizerForTests, shell.FitToWindow);
        return new TabletToolbarViewModel(shell.LocalizerForTests, shell, shell.UndoCommand, shell.RedoCommand, zoom, menu, panel);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var shell = new TestShell();
        var menu = new OverflowMenuViewModel(shell.LocalizerForTests, shell.FileCommandsForTests, shell.EditCommandsForTests);
        var zoom = new PressToShape.App.ViewModels.Editor.ZoomViewModel(shell.State, shell.LocalizerForTests, shell.FitToWindow);
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
        var localizer = shell.LocalizerForTests;

        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(null!, shell, shell.UndoCommand, shell.RedoCommand, zoom, menu, panel));
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(localizer, null!, shell.UndoCommand, shell.RedoCommand, zoom, menu, panel));
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(localizer, shell, null!, shell.RedoCommand, zoom, menu, panel));
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(localizer, shell, shell.UndoCommand, null!, zoom, menu, panel));
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(localizer, shell, shell.UndoCommand, shell.RedoCommand, null!, menu, panel));
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(localizer, shell, shell.UndoCommand, shell.RedoCommand, zoom, null!, panel));
        Assert.Throws<System.ArgumentNullException>(() => new TabletToolbarViewModel(localizer, shell, shell.UndoCommand, shell.RedoCommand, zoom, menu, null!));
    }
}
