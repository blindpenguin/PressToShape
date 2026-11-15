using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.History;

public class DocumentHistoryTests
{
    private static Document Start() => Document.Create("Layer 1");

    private static Document Next(Document from, double x = 0) =>
        from.AddShape(from.Layers[0].Id, new RectangleShape { X = x, Width = 1, Height = 1 });

    [Fact]
    public void New_StartsAtTheGivenDocumentWithoutSteps()
    {
        var start = Start();

        var history = new DocumentHistory(start);

        Assert.Same(start, history.Current);
        Assert.Equal(0, history.CurrentIndex);
        Assert.Empty(history.Steps);
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Equal(DocumentHistory.DefaultMaxSteps, history.MaxSteps);
    }

    [Fact]
    public void Record_NewDocument_BecomesCurrentAndAddsTheStep()
    {
        var history = new DocumentHistory(Start());
        var after = Next(history.Current);

        var recorded = history.Record(HistoryStepType.DrawRectangle, after);

        Assert.True(recorded);
        Assert.Same(after, history.Current);
        Assert.Equal(1, history.CurrentIndex);
        Assert.Equal(new[] { HistoryStepType.DrawRectangle }, history.Steps);
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Record_TheCurrentInstance_IsNotRecorded()
    {
        var history = new DocumentHistory(Start());

        Assert.False(history.Record(HistoryStepType.MoveShape, history.Current));

        Assert.Empty(history.Steps);
        Assert.Equal(0, history.CurrentIndex);
    }

    [Fact]
    public void Record_AnEqualButDifferentInstance_IsRecorded()
    {
        var history = new DocumentHistory(Start());
        var same = history.Current with { };

        Assert.True(history.Record(HistoryStepType.HideLayer, same));
        Assert.NotSame(same, Start());
    }

    [Fact]
    public void UndoAndRedo_WalkBackAndForth()
    {
        var d0 = Start();
        var history = new DocumentHistory(d0);
        var d1 = Next(d0, 1);
        var d2 = Next(d1, 2);
        history.Record(HistoryStepType.DrawRectangle, d1);
        history.Record(HistoryStepType.DrawEllipse, d2);

        Assert.Same(d1, history.Undo());
        Assert.Same(d0, history.Undo());
        Assert.Null(history.Undo());
        Assert.Same(d1, history.Redo());
        Assert.Same(d2, history.Redo());
        Assert.Null(history.Redo());
        Assert.Equal(2, history.CurrentIndex);
    }

    [Fact]
    public void Redo_AfterUndo_ReturnsTheSameInstance()
    {
        var history = new DocumentHistory(Start());
        var d1 = Next(history.Current);
        history.Record(HistoryStepType.DrawRectangle, d1);

        history.Undo();

        Assert.Same(d1, history.Redo());
    }

    [Fact]
    public void Record_AfterUndo_DropsTheRedoSteps()
    {
        var history = new DocumentHistory(Start());
        var d1 = Next(history.Current, 1);
        var d2 = Next(d1, 2);
        history.Record(HistoryStepType.DrawRectangle, d1);
        history.Record(HistoryStepType.DrawEllipse, d2);
        history.Undo();
        var other = Next(d1, 3);

        history.Record(HistoryStepType.MoveShape, other);

        Assert.False(history.CanRedo);
        Assert.Equal(new[] { HistoryStepType.DrawRectangle, HistoryStepType.MoveShape }, history.Steps);
        Assert.Same(other, history.Current);
        Assert.Same(d1, history.Undo());
    }

    [Fact]
    public void Record_OverTheLimit_AllowsExactlyTheLimitOfUndosAndDropsTheOldest()
    {
        var history = new DocumentHistory(Start(), maxSteps: 100);
        var documents = new List<Document> { history.Current };
        for (var i = 0; i < 150; i++)
        {
            documents.Add(Next(documents[^1], i));
            history.Record(HistoryStepType.DrawRectangle, documents[^1]);
        }

        Assert.Equal(100, history.Steps.Count);
        Assert.Equal(100, history.CurrentIndex);

        var undone = 0;
        while (history.Undo() is not null)
        {
            undone++;
        }

        Assert.Equal(100, undone);
        Assert.Null(history.Undo());
        Assert.Same(documents[50], history.Current);
        Assert.Equal(0, history.CurrentIndex);
    }

    [Fact]
    public void Record_OverTheLimit_AfterUndo_KeepsIndexAndStepsConsistent()
    {
        var history = new DocumentHistory(Start(), maxSteps: 3);
        var d = history.Current;
        for (var i = 0; i < 3; i++)
        {
            d = Next(d, i);
            history.Record(HistoryStepType.DrawRectangle, d);
        }

        history.Undo();
        history.Undo();
        history.Record(HistoryStepType.MoveShape, Next(history.Current, 99));

        Assert.Equal(2, history.CurrentIndex);
        Assert.Equal(2, history.Steps.Count);
        Assert.False(history.CanRedo);
        Assert.True(history.CanUndo);
    }

    [Fact]
    public void MaxSteps_One_KeepsOnlyTheLastStep()
    {
        var history = new DocumentHistory(Start(), maxSteps: 1);
        var d1 = Next(history.Current, 1);
        var d2 = Next(d1, 2);
        history.Record(HistoryStepType.DrawRectangle, d1);
        history.Record(HistoryStepType.DrawEllipse, d2);

        Assert.Equal(new[] { HistoryStepType.DrawEllipse }, history.Steps);
        Assert.Same(d1, history.Undo());
        Assert.Null(history.Undo());
    }

    [Fact]
    public void JumpTo_ForwardBackAndToTheCurrentIndex()
    {
        var history = new DocumentHistory(Start());
        var states = new List<Document> { history.Current };
        for (var i = 0; i < 4; i++)
        {
            states.Add(Next(states[^1], i));
            history.Record(HistoryStepType.DrawRectangle, states[^1]);
        }

        Assert.Same(states[1], history.JumpTo(1));
        Assert.Equal(1, history.CurrentIndex);
        Assert.Null(history.JumpTo(1));
        Assert.Same(states[3], history.JumpTo(3));
        Assert.Same(states[0], history.JumpTo(0));
        Assert.Same(states[4], history.JumpTo(4));
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void JumpTo_OutOfRange_ReturnsNullAndChangesNothing(int index)
    {
        var history = new DocumentHistory(Start());
        history.Record(HistoryStepType.DrawRectangle, Next(history.Current));
        var before = history.CurrentIndex;

        Assert.Null(history.JumpTo(index));
        Assert.Equal(before, history.CurrentIndex);
    }

    [Fact]
    public void Reset_ForgetsEverything()
    {
        var history = new DocumentHistory(Start());
        history.Record(HistoryStepType.DrawRectangle, Next(history.Current));
        var fresh = Document.Create("Other");

        history.Reset(fresh);

        Assert.Same(fresh, history.Current);
        Assert.Equal(0, history.CurrentIndex);
        Assert.Empty(history.Steps);
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Steps_IsALiveReadOnlyView()
    {
        var history = new DocumentHistory(Start());
        var view = history.Steps;

        history.Record(HistoryStepType.LockLayer, Next(history.Current));

        Assert.Equal(new[] { HistoryStepType.LockLayer }, view);
        Assert.IsNotType<List<HistoryStepType>>(view);
    }

    [Fact]
    public void Arguments_Invalid_Throw()
    {
        var history = new DocumentHistory(Start());

        Assert.Throws<ArgumentNullException>(() => new DocumentHistory(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentHistory(Start(), 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DocumentHistory(Start(), -5));
        Assert.Throws<ArgumentNullException>(() => history.Record(HistoryStepType.MoveShape, null!));
        Assert.Throws<ArgumentNullException>(() => history.Reset(null!));
    }

    [Fact]
    public void OriginalDocuments_AreNeverChanged()
    {
        var d0 = Start();
        var history = new DocumentHistory(d0);
        var d1 = Next(d0);
        history.Record(HistoryStepType.DrawRectangle, d1);

        history.Undo();
        history.Redo();

        Assert.Empty(d0.Layers[0].Shapes);
        Assert.Single(d1.Layers[0].Shapes);
    }

    [Fact]
    public void RandomCalls_NeverThrow_AndCurrentIsAlwaysARecordedDocument()
    {
        var random = new Random(20260101);
        var history = new DocumentHistory(Start(), maxSteps: 7);
        var known = new HashSet<Document>(ReferenceEqualityComparer.Instance) { history.Current };

        for (var i = 0; i < 5000; i++)
        {
            switch (random.Next(6))
            {
                case 0:
                case 1:
                    var after = Next(history.Current, i);
                    known.Add(after);
                    history.Record((HistoryStepType)random.Next(8), after);
                    break;
                case 2:
                    history.Undo();
                    break;
                case 3:
                    history.Redo();
                    break;
                case 4:
                    history.JumpTo(random.Next(-2, 12));
                    break;
                default:
                    history.Record(HistoryStepType.MoveShape, history.Current);
                    break;
            }

            Assert.Contains(history.Current, known);
            Assert.InRange(history.Steps.Count, 0, 7);
            Assert.InRange(history.CurrentIndex, 0, history.Steps.Count);
            Assert.Equal(history.CurrentIndex > 0, history.CanUndo);
            Assert.Equal(history.CurrentIndex < history.Steps.Count, history.CanRedo);
        }
    }

    [Fact]
    public void ManyShapes_RecordingIsCheap_BecauseDocumentsShareLayers()
    {
        var d0 = Start();
        var layerId = d0.Layers[0].Id;
        var shapes = Enumerable.Range(0, 2000).Select(i => (Shape)new RectangleShape { X = i, Width = 1, Height = 1 }).ToArray();
        var big = d0 with { Layers = [d0.Layers[0] with { Shapes = [.. shapes] }] };
        var history = new DocumentHistory(big);

        var toggled = big.UpdateLayer(layerId, l => l with { IsLocked = true });
        history.Record(HistoryStepType.LockLayer, toggled);

        Assert.True(toggled.Layers[0].Shapes.Equals(big.Layers[0].Shapes)); // the same array, not a copy
        Assert.Equal(2000, history.Current.Layers[0].Shapes.Length);
        Assert.Equal(2000, history.Undo()!.Layers[0].Shapes.Length);
    }

    [Fact]
    public void RecordOrExtend_FirstStep_IsRecorded()
    {
        var history = new DocumentHistory(Start());
        var after = Next(history.Current);

        Assert.True(history.RecordOrExtend(HistoryStepType.NudgeShapes, after));

        Assert.Equal(new[] { HistoryStepType.NudgeShapes }, history.Steps);
        Assert.Same(after, history.Current);
    }

    [Fact]
    public void RecordOrExtend_SameTypeAgain_ExtendsTheLastStep()
    {
        var d0 = Start();
        var history = new DocumentHistory(d0);
        var d1 = Next(d0, 1);
        var d2 = Next(d1, 2);
        var d3 = Next(d2, 3);
        history.RecordOrExtend(HistoryStepType.NudgeShapes, d1);

        Assert.True(history.RecordOrExtend(HistoryStepType.NudgeShapes, d2));
        Assert.True(history.RecordOrExtend(HistoryStepType.NudgeShapes, d3));

        Assert.Single(history.Steps);
        Assert.Equal(1, history.CurrentIndex);
        Assert.Same(d3, history.Current);
        Assert.Same(d0, history.Undo());
        Assert.Same(d3, history.Redo());
    }

    [Fact]
    public void RecordOrExtend_DifferentType_RecordsANewStep()
    {
        var history = new DocumentHistory(Start());
        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 1));

        history.RecordOrExtend(HistoryStepType.MoveShape, Next(history.Current, 2));
        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 3));

        Assert.Equal(new[] { HistoryStepType.NudgeShapes, HistoryStepType.MoveShape, HistoryStepType.NudgeShapes }, history.Steps);
    }

    [Fact]
    public void RecordOrExtend_AfterUndo_RecordsANewStepAndDropsTheRedoSteps()
    {
        var history = new DocumentHistory(Start());
        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 1));
        history.RecordOrExtend(HistoryStepType.MoveShape, Next(history.Current, 2));
        history.Undo();

        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 3));

        Assert.Equal(new[] { HistoryStepType.NudgeShapes, HistoryStepType.NudgeShapes }, history.Steps);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void RecordOrExtend_AfterUndoOfTheOnlyStepOfThatType_DoesNotExtendTheEarlierStep()
    {
        var history = new DocumentHistory(Start());
        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 1));
        history.Undo();

        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 2));

        Assert.Single(history.Steps);
        Assert.Equal(1, history.CurrentIndex);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void RecordOrExtend_TheCurrentInstance_DoesNothing()
    {
        var history = new DocumentHistory(Start());
        history.RecordOrExtend(HistoryStepType.NudgeShapes, Next(history.Current, 1));

        Assert.False(history.RecordOrExtend(HistoryStepType.NudgeShapes, history.Current));
        Assert.Single(history.Steps);
    }

    [Fact]
    public void RecordOrExtend_KeepsTheLimit()
    {
        var history = new DocumentHistory(Start(), maxSteps: 3);
        for (var i = 0; i < 6; i++)
        {
            history.RecordOrExtend(i % 2 == 0 ? HistoryStepType.NudgeShapes : HistoryStepType.MoveShape, Next(history.Current, i));
        }

        Assert.Equal(3, history.Steps.Count);
        Assert.Equal(3, history.CurrentIndex);
    }

    [Fact]
    public void RecordOrExtend_NullDocument_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DocumentHistory(Start()).RecordOrExtend(HistoryStepType.NudgeShapes, null!));
    }
}
