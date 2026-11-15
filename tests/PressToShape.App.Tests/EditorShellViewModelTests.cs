using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class EditorShellViewModelTests
{
    /// <summary>The smallest shell: only what the base class gives every layout.</summary>
    private sealed class BareShell : EditorShellViewModel
    {
        public BareShell(ILocalizer localizer, FileServices services)
            : base(localizer, services)
        {
        }
    }

    private sealed class Fixture
    {
        public Fixture(ILocalizer? localizer = null)
        {
            Shell = new BareShell(localizer ?? FakeLocalizer.Create(), TestFileServices.Create(Files, Dialogs, Messages));
        }

        public FakeFileStore Files { get; } = new();

        public FakeFileDialogs Dialogs { get; } = new();

        public FakeMessageDialogs Messages { get; } = new();

        public BareShell Shell { get; }

        public void Edit() =>
            Shell.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(Shell.State.ActiveLayerId, new RectangleShape { Width = 5, Height = 5 }));
    }

    [Fact]
    public void NewShell_IsUntitledAndClean()
    {
        var f = new Fixture();

        Assert.Equal("Untitled", f.Shell.DocumentName);
        Assert.False(f.Shell.IsDirty);
        Assert.Equal("Untitled — PressToShape", f.Shell.WindowTitle);
    }

    [Fact]
    public void AnEdit_MakesTheShellDirty_AndRaisesTheNameNotifications()
    {
        var f = new Fixture();
        var raised = new List<string?>();
        f.Shell.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Edit();

        Assert.True(f.Shell.IsDirty);
        Assert.Equal("* Untitled — PressToShape", f.Shell.WindowTitle);
        Assert.Contains(nameof(EditorShellViewModel.IsDirty), raised);
        Assert.Contains(nameof(EditorShellViewModel.WindowTitle), raised);
    }

    [Fact]
    public async Task SaveAs_GivesTheFileNameAndClearsTheDirtyMark()
    {
        var f = new Fixture();
        f.Edit();
        f.Dialogs.SaveAnswers.Enqueue("/work/plan.p2s");

        await f.Shell.SaveAsCommand.ExecuteAsync(null);

        Assert.Equal("plan.p2s", f.Shell.DocumentName);
        Assert.False(f.Shell.IsDirty);
    }

    [Fact]
    public void DocumentName_FollowsTheLanguage()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Untitled"] = "Unbenannt" });
        var f = new Fixture(localizer);
        var raised = new List<string?>();
        f.Shell.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        localizer.SetLanguage("de");

        Assert.Equal("Unbenannt", f.Shell.DocumentName);
        Assert.Contains(nameof(EditorShellViewModel.DocumentName), raised);
    }

    [Fact]
    public void SharedRegions_UseTheSameEditorState()
    {
        var f = new Fixture();

        f.Shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Rectangle).SelectCommand.Execute(null);

        Assert.Equal(Tool.Rectangle, f.Shell.State.ActiveTool);
        Assert.Equal(f.Shell.State.Document, f.Shell.Canvas.Document);
    }

    [Fact]
    public void UndoAndRedo_FollowTheHistory()
    {
        var f = new Fixture();
        Assert.False(f.Shell.UndoCommand.CanExecute(null));
        Assert.False(f.Shell.RedoCommand.CanExecute(null));

        f.Edit();
        Assert.True(f.Shell.UndoCommand.CanExecute(null));

        f.Shell.UndoCommand.Execute(null);
        Assert.False(f.Shell.UndoCommand.CanExecute(null));
        Assert.True(f.Shell.RedoCommand.CanExecute(null));
    }

    [Fact]
    public async Task ConfirmClose_WithoutChanges_AllowsClosingWithoutAQuestion()
    {
        var f = new Fixture();

        Assert.True(await f.Shell.ConfirmCloseAsync());
        Assert.Empty(f.Messages.Questions);
    }

    [Fact]
    public async Task ConfirmClose_WithChanges_AsksAndFollowsTheAnswer()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Cancel;

        Assert.False(await f.Shell.ConfirmCloseAsync());
        Assert.Single(f.Messages.Questions);

        f.Messages.Answer = UnsavedChoice.DontSave;
        Assert.True(await f.Shell.ConfirmCloseAsync());
    }

    [Fact]
    public void FitToWindow_UsesTheCanvasViewport()
    {
        var f = new Fixture();
        f.Shell.Canvas.ViewportWidth = 600;
        f.Shell.Canvas.ViewportHeight = 400;
        f.Shell.State.SetZoom(400);

        f.Shell.FitToWindow();

        Assert.True(f.Shell.State.ZoomPercent < 100);
    }
}
