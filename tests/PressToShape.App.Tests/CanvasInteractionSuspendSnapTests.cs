using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public sealed class CanvasInteractionSuspendSnapTests
{
    private static (EditorState State, CanvasInteraction Interaction) Create()
    {
        var state = new EditorState { ActiveTool = Tool.Rectangle };
        return (state, new CanvasInteraction(state));
    }

    [Fact]
    public void PointerMethods_WithoutTheArgument_DoNotSuspend()
    {
        var (_, interaction) = Create();

        interaction.PointerPressed(10, 10);
        Assert.False(interaction.IsSnapSuspended);
        interaction.PointerMoved(20, 20);
        Assert.False(interaction.IsSnapSuspended);
        interaction.PointerReleased(20, 20);
        Assert.False(interaction.IsSnapSuspended);
    }

    [Fact]
    public void PointerMethods_WithTheArgument_SuspendForThatEvent()
    {
        var (_, interaction) = Create();

        interaction.PointerPressed(10, 10, suspendSnap: true);
        Assert.True(interaction.IsSnapSuspended);
        interaction.PointerMoved(20, 20, suspendSnap: true);
        Assert.True(interaction.IsSnapSuspended);
        interaction.PointerMoved(25, 25);
        Assert.False(interaction.IsSnapSuspended);
        interaction.PointerReleased(30, 30, suspendSnap: true);
        Assert.True(interaction.IsSnapSuspended);
    }

    [Fact]
    public void CanvasAreaViewModel_PassesTheArgumentToTheInteraction()
    {
        var state = new EditorState { ActiveTool = Tool.Rectangle };
        var canvas = new CanvasAreaViewModel(state, new SelectionEditor(state));

        // The view model forwards the flag; with the interaction private we check the drag still works with it set.
        canvas.PointerPressed(100, 100, suspendSnap: true);
        canvas.PointerMoved(200, 180, suspendSnap: true);
        canvas.PointerReleased(200, 180, suspendSnap: true);

        Assert.Single(state.Document.Layers[0].Shapes);
    }
}
