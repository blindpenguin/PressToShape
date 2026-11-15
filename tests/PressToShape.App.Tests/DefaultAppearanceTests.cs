using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class DefaultAppearanceTests
{
    private static readonly Appearance Custom = new() { Fill = Color.FromRgb(1, 2, 3), Stroke = null, StrokeWidth = 4, Opacity = 0.5 };

    private static Shape DrawWith(Tool tool, Appearance? appearance)
    {
        var state = new EditorState(Document.Create("L")) { ActiveTool = tool };
        if (appearance is not null)
        {
            state.DefaultAppearance = appearance;
        }

        var interaction = new CanvasInteraction(state);
        interaction.PointerPressed(10, 10);
        interaction.PointerMoved(60, 60);
        interaction.PointerReleased(110, 110);
        return Assert.Single(state.Document.Layers.SelectMany(layer => layer.Shapes));
    }

    [Fact]
    public void DefaultAppearance_NewState_IsTheModelDefault() =>
        Assert.Same(Appearance.Default, new EditorState().DefaultAppearance);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Draw_ChangedDefault_ShapeUsesIt(bool ellipse) =>
        Assert.Equal(Custom, DrawWith(ellipse ? Tool.Ellipse : Tool.Rectangle, Custom).Appearance);

    [Fact]
    public void Draw_UnchangedDefault_ShapeUsesModelDefault() =>
        Assert.Equal(Appearance.Default, DrawWith(Tool.Rectangle, null).Appearance);

    [Fact]
    public void UndoRedoAndReplaceDocument_DoNotChangeTheDefault()
    {
        var state = new EditorState(Document.Create("L")) { DefaultAppearance = Custom };
        state.Edit(PressToShape.Core.History.HistoryStepType.AddSwatch, document => document.AddSwatch(Color.FromRgb(9, 9, 9)));

        state.Undo();
        state.Redo();
        state.ReplaceDocument(Document.Create("M"));

        Assert.Same(Custom, state.DefaultAppearance);
    }
}
