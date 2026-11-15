using System;
using System.Collections.Generic;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class DocumentSessionTests
{
    private static (EditorState State, DocumentSession Session) Create()
    {
        var state = new EditorState(Document.Create("Layer 1"));
        return (state, new DocumentSession(state));
    }

    private static List<string?> Record(DocumentSession session)
    {
        var raised = new List<string?>();
        session.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        return raised;
    }

    [Fact]
    public void New_StartDocument_IsCleanWithoutAFile()
    {
        var (_, session) = Create();

        Assert.False(session.IsDirty);
        Assert.Null(session.Path);
        Assert.Equal(string.Empty, session.DisplayName);
    }

    [Fact]
    public void IsDirty_AfterAnEdit_IsTrueAndNotifies()
    {
        var (state, session) = Create();
        var raised = Record(session);

        state.Document = state.Document.AddShape(state.ActiveLayerId, new RectangleShape { Width = 1, Height = 1 });

        Assert.True(session.IsDirty);
        Assert.Contains(nameof(DocumentSession.IsDirty), raised);
    }

    [Fact]
    public void IsDirty_AfterALayerToggle_IsTrue()
    {
        var (state, session) = Create();

        state.UpdateLayer(state.ActiveLayerId, l => l with { IsVisible = false }, HistoryStepType.HideLayer);

        Assert.True(session.IsDirty);
    }

    [Fact]
    public void IsDirty_EditThenChangedBackToAnEqualDocument_StaysDirty()
    {
        var (state, session) = Create();
        var original = state.Document;

        state.UpdateLayer(state.ActiveLayerId, l => l with { IsLocked = true }, HistoryStepType.LockLayer);
        state.UpdateLayer(state.ActiveLayerId, l => l with { IsLocked = false }, HistoryStepType.UnlockLayer);

        Assert.NotSame(original, state.Document);
        Assert.True(session.IsDirty);
    }

    [Fact]
    public void IsDirty_ReplacingTheDocumentWithTheSavedInstance_IsClean()
    {
        var (state, session) = Create();
        var saved = state.Document;
        state.UpdateLayer(state.ActiveLayerId, l => l with { IsLocked = true }, HistoryStepType.LockLayer);

        state.Document = saved;

        Assert.False(session.IsDirty);
    }

    [Fact]
    public void MarkSaved_WithTheWrittenInstance_MakesItCleanAndSetsThePath()
    {
        var (state, session) = Create();
        state.UpdateLayer(state.ActiveLayerId, l => l with { IsLocked = true }, HistoryStepType.LockLayer);
        var raised = Record(session);

        session.MarkSaved("/home/a/drawing.p2s", state.Document);

        Assert.False(session.IsDirty);
        Assert.Equal("/home/a/drawing.p2s", session.Path);
        Assert.Equal("drawing.p2s", session.DisplayName);
        Assert.Contains(nameof(DocumentSession.Path), raised);
        Assert.Contains(nameof(DocumentSession.DisplayName), raised);
        Assert.Contains(nameof(DocumentSession.IsDirty), raised);
    }

    [Fact]
    public void MarkSaved_WithAnOlderInstanceWhileTheDocumentChanged_StaysDirty()
    {
        var (state, session) = Create();
        var written = state.Document;
        state.UpdateLayer(state.ActiveLayerId, l => l with { IsLocked = true }, HistoryStepType.LockLayer);

        session.MarkSaved("/a.p2s", written);

        Assert.True(session.IsDirty);
        Assert.Equal("/a.p2s", session.Path);
    }

    [Fact]
    public void Reset_SetsThePathAndClearsDirty()
    {
        var (state, session) = Create();
        state.UpdateLayer(state.ActiveLayerId, l => l with { IsLocked = true }, HistoryStepType.LockLayer);
        var opened = Document.Create("Opened");
        state.Document = opened;

        session.Reset("/x/opened.p2s", opened);

        Assert.False(session.IsDirty);
        Assert.Equal("opened.p2s", session.DisplayName);
    }

    [Fact]
    public void Reset_WithoutAPath_HasNoFile()
    {
        var (state, session) = Create();
        session.MarkSaved("/a.p2s", state.Document);

        session.Reset(null, state.Document);

        Assert.Null(session.Path);
        Assert.Equal(string.Empty, session.DisplayName);
    }

    [Theory]
    [InlineData("/a/b/c.p2s", "c.p2s")]
    [InlineData("C:\\Users\\me\\c.p2s", "c.p2s")]
    [InlineData("c.p2s", "c.p2s")]
    [InlineData("/a/b/with space.p2s", "with space.p2s")]
    [InlineData("/a/b/mixed\\c.p2s", "c.p2s")]
    public void DisplayName_IsTheFileNameWithoutTheFolder(string path, string expected)
    {
        var (state, session) = Create();

        session.MarkSaved(path, state.Document);

        Assert.Equal(expected, session.DisplayName);
    }

    [Fact]
    public void Arguments_Invalid_Throw()
    {
        var (state, session) = Create();

        Assert.Throws<ArgumentNullException>(() => new DocumentSession(null!));
        Assert.ThrowsAny<ArgumentException>(() => session.MarkSaved(" ", state.Document));
        Assert.Throws<ArgumentNullException>(() => session.MarkSaved("/a.p2s", null!));
        Assert.Throws<ArgumentNullException>(() => session.Reset("/a.p2s", null!));
    }

    [Fact]
    public void ZoomOrToolChanges_DoNotMakeTheDocumentDirty()
    {
        var (state, session) = Create();

        state.SetZoom(200);
        state.ActiveTool = Tool.Rectangle;
        state.Select(null);

        Assert.False(session.IsDirty);
    }

    [Fact]
    public void Undo_BackToTheSavedInstance_IsClean_AndRedoIsDirtyAgain()
    {
        var (state, session) = Create();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 1, Height = 1 }));
        Assert.True(session.IsDirty);

        state.Undo();
        Assert.False(session.IsDirty);

        state.Redo();
        Assert.True(session.IsDirty);
    }

    [Fact]
    public void Save_ThenUndo_IsDirty_AndRedoIsCleanAgain()
    {
        var (state, session) = Create();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 1, Height = 1 }));
        session.MarkSaved("/a.p2s", state.Document);

        state.Undo();
        Assert.True(session.IsDirty);
        Assert.True(state.CanRedo);

        state.Redo();
        Assert.False(session.IsDirty);
    }

    [Fact]
    public void Save_DoesNotTouchTheHistory()
    {
        var (state, session) = Create();
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, new RectangleShape { Width = 1, Height = 1 }));

        session.MarkSaved("/a.p2s", state.Document);

        Assert.Single(state.History.Steps);
        Assert.True(state.CanUndo);
    }
}
