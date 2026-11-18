using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class EditorStateSnapTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    [Fact]
    public void Snapping_Default_IsInMemoryWithTheDefaults()
    {
        var state = new EditorState();

        Assert.False(state.Snapping.ShowGrid);
        Assert.False(state.Snapping.SnapToGrid);
        Assert.False(state.Snapping.SnapToObjects);
        Assert.Equal(10, state.Snapping.GridSpacing);
        Assert.True(state.Snapping.ShowRulers);
    }

    [Fact]
    public void Snapping_Given_IsUsed()
    {
        var preferences = new SnapPreferences(new MemorySettingsStore(), defaultShowRulers: false);

        var state = new EditorState(snapping: preferences);

        Assert.Same(preferences, state.Snapping);
        Assert.False(state.Snapping.ShowRulers);
    }

    [Fact]
    public void SnapGuides_Default_IsEmpty()
    {
        Assert.Empty(new EditorState().SnapGuides);
    }

    [Fact]
    public void SnapGuides_Set_RaisesPropertyChanged()
    {
        var state = new EditorState();
        var raised = new System.Collections.Generic.List<string?>();
        state.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.SnapGuides = [new SnapGuide(SnapAxis.X, 10, 0, 5)];

        Assert.Contains(nameof(EditorState.SnapGuides), raised);
    }

    [Fact]
    public void ChangingASetting_IsNoEditAndNoStepAndLeavesTheDocumentAlone()
    {
        var state = new EditorState();
        var document = state.Document;
        var canUndo = state.CanUndo;
        var changed = new System.Collections.Generic.List<string?>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        state.Snapping.ShowGrid = true;
        state.Snapping.SnapToGrid = true;
        state.Snapping.SnapToObjects = true;
        state.Snapping.GridSpacing = 25;
        state.Snapping.ShowRulers = false;

        Assert.Same(document, state.Document);
        Assert.Equal(canUndo, state.CanUndo);
        Assert.False(state.CanRedo);
        Assert.DoesNotContain(nameof(EditorState.Document), changed);
    }

    [Fact]
    public void ChangingASetting_DoesNotRecordAHistoryStep()
    {
        var state = new EditorState();
        var steps = state.History.Steps.Count;

        state.Snapping.SnapToGrid = true;

        Assert.Equal(steps, state.History.Steps.Count);
    }

    [Fact]
    public void ChangingASetting_DoesNotMarkTheDocumentAsChanged()
    {
        var state = new EditorState();
        var session = new DocumentSession(state);

        state.Snapping.ShowGrid = true;
        state.Snapping.GridSpacing = 40;

        Assert.False(session.IsDirty);
    }

    [Fact]
    public void DesktopShell_UsesTheSettingsFileAndShowsRulersByDefault()
    {
        var store = new MemorySettingsStore();
        var shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create(settings: store));

        shell.State.Snapping.SnapToGrid = true;

        Assert.True(shell.State.Snapping.ShowRulers);
        Assert.True(new SnapPreferences(store).SnapToGrid);
    }

    [Fact]
    public void TabletShell_HidesRulersByDefault()
    {
        var shell = new TabletShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create(), new MessageDialogHost());

        Assert.False(shell.State.Snapping.ShowRulers);
    }
}
