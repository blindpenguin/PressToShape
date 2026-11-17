using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PathEditorBackgroundTests
{
    // Runs the work when the test lets it, on the calling thread, so the test decides when the background work ends.
    private sealed class GatedRunner
    {
        private readonly TaskCompletionSource _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Calls { get; private set; }

        public Task<PathOperationResult> Run(Func<PathOperationResult> work)
        {
            Calls++;
            return RunAfterGate(work);
        }

        public void Release() => _gate.SetResult();

        private async Task<PathOperationResult> RunAfterGate(Func<PathOperationResult> work)
        {
            await _gate.Task.ConfigureAwait(false);
            return work();
        }
    }

    private sealed class Fixture
    {
        public Fixture(int limit, Func<Func<PathOperationResult>, Task<PathOperationResult>>? runner, params Shape[] shapes)
        {
            var document = Document.Create("x");
            State = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
            Messages = new FakeMessageDialogs();
            Editor = new PathEditor(State, Messages, FakeLocalizer.Create(), backgroundNodeLimit: limit, background: runner);
            State.SetSelection(shapes.Select(shape => shape.Id));
        }

        public EditorState State { get; }

        public FakeMessageDialogs Messages { get; }

        public PathEditor Editor { get; }

        public IReadOnlyList<Shape> Shapes => State.Document.Layers[0].Shapes;
    }

    private static RectangleShape Rect(double x, double y, double size = 100) => new() { X = x, Y = y, Width = size, Height = size };

    [Fact]
    public async Task BooleanAsync_AboveTheLimit_IsBusyUntilTheWorkEndsThenAppliesOnce()
    {
        var runner = new GatedRunner();
        var f = new Fixture(0, runner.Run, Rect(0, 0), Rect(50, 50));
        var busyChanges = 0;
        f.Editor.BusyChanged += (_, _) => busyChanges++;
        var steps = f.State.History.Steps.Count;

        var running = f.Editor.BooleanAsync(PathOperationType.Union);

        Assert.True(f.Editor.IsBusy);
        Assert.True(f.State.IsDragInProgress);
        Assert.False(f.Editor.CanBoolean);
        Assert.False(f.Editor.CanCombine);
        Assert.False(f.State.CanUndo);
        Assert.Equal(1, busyChanges);

        runner.Release();
        await running;

        Assert.False(f.Editor.IsBusy);
        Assert.False(f.State.IsDragInProgress);
        Assert.Equal(2, busyChanges);
        Assert.Equal(steps + 1, f.State.History.Steps.Count);
        Assert.IsType<PathShape>(Assert.Single(f.Shapes));
        Assert.Equal(1, runner.Calls);
    }

    [Fact]
    public async Task BooleanAsync_BelowTheLimit_RunsInlineWithoutBusyState()
    {
        var runner = new GatedRunner();
        var f = new Fixture(1_000_000, runner.Run, Rect(0, 0), Rect(50, 50));
        var busyChanges = 0;
        f.Editor.BusyChanged += (_, _) => busyChanges++;

        await f.Editor.BooleanAsync(PathOperationType.Union);

        Assert.Equal(0, runner.Calls);
        Assert.Equal(0, busyChanges);
        Assert.IsType<PathShape>(Assert.Single(f.Shapes));
    }

    [Fact]
    public async Task BooleanAsync_DocumentChangedWhileRunning_DropsTheResult()
    {
        var a = Rect(0, 0);
        var b = Rect(50, 50);
        var runner = new GatedRunner();
        var f = new Fixture(0, runner.Run, a, b);
        var running = f.Editor.BooleanAsync(PathOperationType.Union);

        f.State.Edit(HistoryStepType.RenameShape, document => document.Rename(a.Id, "changed"));
        var steps = f.State.History.Steps.Count;
        runner.Release();
        await running;

        Assert.False(f.Editor.IsBusy);
        Assert.Equal(steps, f.State.History.Steps.Count);
        Assert.Equal([a.Id, b.Id], f.Shapes.Select(s => s.Id));
        Assert.Empty(f.Messages.Errors);
    }

    [Fact]
    public async Task BooleanAsync_EmptyResultInTheBackground_ShowsTheMessageAndIsNoLongerBusy()
    {
        var runner = new GatedRunner();
        var f = new Fixture(0, runner.Run, Rect(0, 0, 10), Rect(500, 500, 10));
        var running = f.Editor.BooleanAsync(PathOperationType.Intersection);

        runner.Release();
        await running;

        Assert.False(f.Editor.IsBusy);
        Assert.False(f.State.IsDragInProgress);
        Assert.Equal(["The operation left nothing, so nothing was changed."], f.Messages.Errors);
        Assert.Equal(2, f.Shapes.Count);
    }

    [Fact]
    public async Task BooleanAsync_RunnerThrows_IsNoLongerBusyAndTheExceptionReachesTheCaller()
    {
        var f = new Fixture(0, _ => throw new InvalidOperationException("boom"), Rect(0, 0), Rect(50, 50));

        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Editor.BooleanAsync(PathOperationType.Union));

        Assert.False(f.Editor.IsBusy);
        Assert.False(f.State.IsDragInProgress);
    }

    [Fact]
    public async Task BooleanAsync_WithTheDefaultBackgroundRunner_ProducesTheSameResult()
    {
        var f = new Fixture(0, null, Rect(0, 0), Rect(50, 50));

        await f.Editor.BooleanAsync(PathOperationType.Union);

        Assert.False(f.Editor.IsBusy);
        var result = Assert.IsType<PathShape>(Assert.Single(f.Shapes));
        Assert.True(result.Contains(20, 20));
        Assert.True(result.Contains(140, 140));
    }

    [Fact]
    public void CanBoolean_WhileADragIsInProgress_IsFalse()
    {
        var f = new Fixture(1_000_000, null, Rect(0, 0), Rect(50, 50));
        Assert.True(f.Editor.CanBoolean);

        f.State.IsDragInProgress = true;

        Assert.False(f.Editor.CanBoolean);
        Assert.False(f.Editor.CanCombine);
    }

    [Fact]
    public async Task StatusBar_WhileAnOperationRuns_ShowsTheBusyTextAndRaisesChanges()
    {
        var runner = new GatedRunner();
        var f = new Fixture(0, runner.Run, Rect(0, 0), Rect(50, 50));
        var bar = new StatusBarViewModel(f.State, FakeLocalizer.Create(), () => { }, f.Editor);
        var changed = new List<string?>();
        bar.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        Assert.False(bar.IsBusy);

        var running = f.Editor.BooleanAsync(PathOperationType.Union);

        Assert.True(bar.IsBusy);
        Assert.Equal("Working…", bar.BusyText);
        Assert.Contains(nameof(StatusBarViewModel.IsBusy), changed);

        runner.Release();
        await running;

        Assert.False(bar.IsBusy);
    }

    [Fact]
    public void StatusBar_WithoutPathEditing_IsNeverBusy()
    {
        var bar = new StatusBarViewModel(new EditorState(Document.Create("x")), FakeLocalizer.Create(), () => { });

        Assert.False(bar.IsBusy);
    }
}
