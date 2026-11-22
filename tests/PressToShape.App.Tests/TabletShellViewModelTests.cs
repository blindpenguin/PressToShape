using System;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class TabletShellViewModelTests
{
    private sealed class FakeShutdown : IAppShutdown
    {
        public int Calls { get; private set; }

        public void Shutdown() => Calls++;
    }

    private sealed class Fixture
    {
        public Fixture(bool overlayDialogs = false)
        {
            Localizer = FakeLocalizer.Create();
            IMessageDialogs messages = overlayDialogs ? new OverlayMessageDialogs(Host, Localizer) : Messages;
            var services = new FileServices(new MemorySettingsStore(), Files, Dialogs, messages);
            Shell = new TabletShellViewModel(Shutdown, Localizer, services, Host);
        }

        public ILocalizer Localizer { get; }

        public FakeFileStore Files { get; } = new();

        public FakeFileDialogs Dialogs { get; } = new();

        public FakeMessageDialogs Messages { get; } = new();

        public MessageDialogHost Host { get; } = new();

        public FakeShutdown Shutdown { get; } = new();

        public TabletShellViewModel Shell { get; }

        public void Edit() => Shell.State.Edit(
            PressToShape.Core.History.HistoryStepType.DrawRectangle,
            d => d.AddShape(Shell.State.ActiveLayerId, new PressToShape.Core.Shapes.RectangleShape { Width = 10, Height = 10 }));
    }

    // ---- the regions share one state ----

    [Fact]
    public void Shell_CreatesAllRegions()
    {
        var f = new Fixture();

        Assert.NotNull(f.Shell.Toolbar);
        Assert.NotNull(f.Shell.OverflowMenu);
        Assert.NotNull(f.Shell.Panel);
        Assert.NotNull(f.Shell.Column);
        Assert.NotNull(f.Shell.Column.ColorSwatch);
        Assert.NotNull(f.Shell.Dialogs);
        Assert.NotNull(f.Shell.Toolbox);
        Assert.NotNull(f.Shell.Canvas);
        Assert.Same(f.Host, f.Shell.Dialogs);
        Assert.Same(f.Shell.OverflowMenu, f.Shell.Toolbar.OverflowMenu);
        Assert.Same(f.Shell.Inspector, f.Shell.Panel.Inspector);
        Assert.Equal(3, f.Shell.Toolbar.PanelButtons.Count);
    }

    [Fact]
    public void TheColorSwatch_OpensThePanelOfTheShell_OnTheSharedProperties()
    {
        var f = new Fixture();

        f.Shell.Column.ColorSwatch.OpenCommand.Execute(null);

        Assert.True(f.Shell.Panel.IsOpen);
        Assert.Same(f.Shell.Inspector.Properties, f.Shell.Panel.Content);
    }

    [Fact]
    public void ATool_ChosenInTheToolColumn_IsTheActiveToolOfTheEditor()
    {
        var f = new Fixture();

        f.Shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Ellipse).SelectCommand.Execute(null);

        Assert.Equal(Tool.Ellipse, f.Shell.State.ActiveTool);
        Assert.Equal(Tool.Ellipse, f.Shell.Toolbox.ActiveTool);
    }

    [Fact]
    public void AnEdit_ShowsInTheToolbar_TheTitle_AndTheCanvas()
    {
        var f = new Fixture();
        Assert.False(f.Shell.Toolbar.UndoButton.IsEnabled);

        f.Edit();

        Assert.True(f.Shell.Toolbar.UndoButton.IsEnabled);
        Assert.Equal("* Untitled", f.Shell.Toolbar.DocumentTitle);
        Assert.Equal("* Untitled — PressToShape", f.Shell.WindowTitle);
        Assert.Same(f.Shell.State.Document, f.Shell.Canvas.Document);
    }

    [Fact]
    public void ASelection_ShowsInThePropertiesTab()
    {
        var f = new Fixture();
        f.Edit();
        var shape = f.Shell.State.Document.Layers.SelectMany(l => l.Shapes).Single();

        f.Shell.State.SelectOnly(shape.Id);

        Assert.True(f.Shell.Inspector.Properties.HasSelection);
        Assert.Equal("1 selected", f.Shell.Inspector.Properties.Header);
        Assert.Equal("10", f.Shell.Inspector.Properties.Width.Text);
        Assert.True(f.Shell.Toolbar.DeleteButton.IsEnabled);
    }

    [Fact]
    public void TheZoomOfTheStatusBar_IsTheZoomOfTheEditor()
    {
        var f = new Fixture();
        Assert.Equal("100%", f.Shell.StatusBar.ZoomText);

        f.Shell.StatusBar.ZoomInCommand.Execute(null);

        Assert.Equal(150, f.Shell.State.ZoomPercent);
        Assert.Equal(150, f.Shell.Canvas.ZoomPercent);
        Assert.Equal("150%", f.Shell.StatusBar.ZoomText);
        Assert.Equal(PressToShape.App.Workspace.ZoomLevels.Presets, f.Shell.StatusBar.ZoomPresets.Select(p => p.Percent));
    }

    [Fact]
    public void TheToolbar_HasNoZoom_AndTheStatusBarIsMadeWithThePathEditor()
    {
        var f = new Fixture();

        Assert.Null(typeof(PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel).GetProperty("Zoom"));
        Assert.False(f.Shell.StatusBar.IsBusy);
    }

    // ---- leaving the app ----

    [Fact]
    public async Task Leave_WithoutChanges_EndsTheAppWithoutAQuestion()
    {
        var f = new Fixture();

        var left = await f.Shell.RequestLeaveAsync();

        Assert.True(left);
        Assert.Equal(1, f.Shutdown.Calls);
        Assert.Empty(f.Messages.Questions);
    }

    [Fact]
    public async Task Leave_WithChanges_Save_SavesAndEndsTheApp()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Save;
        f.Dialogs.SaveAnswers.Enqueue("/work/plan.p2s");

        var left = await f.Shell.RequestLeaveAsync();

        Assert.True(left);
        Assert.Equal(1, f.Shutdown.Calls);
        Assert.True(f.Files.Files.ContainsKey("/work/plan.p2s"));
        Assert.Single(f.Messages.Questions);
    }

    [Fact]
    public async Task Leave_WithChanges_DontSave_EndsTheAppWithoutSaving()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.DontSave;

        var left = await f.Shell.RequestLeaveAsync();

        Assert.True(left);
        Assert.Equal(1, f.Shutdown.Calls);
        Assert.Empty(f.Files.Files);
    }

    [Fact]
    public async Task Leave_WithChanges_Cancel_KeepsTheAppAndTheDrawing()
    {
        var f = new Fixture();
        f.Edit();
        var document = f.Shell.State.Document;
        f.Messages.Answer = UnsavedChoice.Cancel;

        var left = await f.Shell.RequestLeaveAsync();

        Assert.False(left);
        Assert.Equal(0, f.Shutdown.Calls);
        Assert.Same(document, f.Shell.State.Document);
        Assert.True(f.Shell.IsDirty);
    }

    [Fact]
    public async Task Leave_WithChanges_SaveFailing_KeepsTheAppAndShowsTheError()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Save;
        f.Dialogs.SaveAnswers.Enqueue("/work/plan.p2s");
        f.Files.WriteFailure = new System.IO.IOException("disk full");

        var left = await f.Shell.RequestLeaveAsync();

        Assert.False(left);
        Assert.Equal(0, f.Shutdown.Calls);
        Assert.Single(f.Messages.Errors);
        Assert.True(f.Shell.IsDirty);
    }

    [Fact]
    public async Task Leave_WhileADialogIsOpen_ClosesItAsCancel_AndTheAppStays()
    {
        var f = new Fixture();
        var dialog = MessageDialogViewModel.ForError(f.Localizer, "Something went wrong");
        var shown = f.Host.ShowAsync(dialog);

        var left = await f.Shell.RequestLeaveAsync();

        Assert.False(left);
        Assert.Null(await shown);
        Assert.Equal(0, f.Shutdown.Calls);
        Assert.False(f.Host.IsOpen);
    }

    [Fact]
    public async Task Leave_WithTheOverlayDialog_DontSaveEndsTheApp()
    {
        var f = new Fixture(overlayDialogs: true);
        f.Edit();

        var leaving = f.Shell.RequestLeaveAsync();
        Assert.True(f.Host.IsOpen);
        Assert.Equal("Unsaved changes", f.Host.Current!.Title);
        f.Host.Current.Buttons.Single(b => b.Text == "Don't save").Command.Execute(null);

        Assert.True(await leaving);
        Assert.Equal(1, f.Shutdown.Calls);
        Assert.False(f.Host.IsOpen);
    }

    [Fact]
    public async Task Leave_WithTheOverlayDialog_BackWhileTheQuestionIsOpen_CancelsIt()
    {
        var f = new Fixture(overlayDialogs: true);
        f.Edit();
        var first = f.Shell.RequestLeaveAsync();
        Assert.True(f.Host.IsOpen);

        var second = await f.Shell.RequestLeaveAsync();

        Assert.False(second);
        Assert.False(await first);
        Assert.Equal(0, f.Shutdown.Calls);
        Assert.True(f.Shell.IsDirty);
    }

    [Fact]
    public async Task Leave_ARequestAfterACancel_AsksAgain()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Cancel;
        await f.Shell.RequestLeaveAsync();

        f.Messages.Answer = UnsavedChoice.DontSave;
        var left = await f.Shell.RequestLeaveAsync();

        Assert.True(left);
        Assert.Equal(2, f.Messages.Questions.Count);
    }

    // ---- the Back button ----

    [Fact]
    public async Task Back_WhileADialogIsOpen_ClosesOnlyTheDialog()
    {
        var f = new Fixture();
        f.Shell.Panel.Toggle(InspectorTab.Layers);
        f.Shell.Column.Groups[0].OpenFlyoutCommand.Execute(null);
        var shown = f.Host.ShowAsync(MessageDialogViewModel.ForError(f.Localizer, "Oops"));

        var left = await f.Shell.RequestBackAsync();

        Assert.False(left);
        Assert.Null(await shown);
        Assert.True(f.Shell.Column.HasOpenFlyout);
        Assert.True(f.Shell.Panel.IsOpen);
    }

    [Fact]
    public async Task TryBack_WhileThePanelIsOpen_ClosesThePanelWithoutAMessage()
    {
        var f = new Fixture();
        f.Shell.Panel.Toggle(InspectorTab.History);

        var left = await f.Shell.TryRequestBackAsync();

        Assert.False(left);
        Assert.False(f.Shell.Panel.IsOpen);
        Assert.Empty(f.Messages.Errors);
    }

    [Fact]
    public async Task TryBack_WithoutChanges_LeavesTheApp()
    {
        var f = new Fixture();

        var left = await f.Shell.TryRequestBackAsync();

        Assert.True(left);
        Assert.Equal(1, f.Shutdown.Calls);
        Assert.Empty(f.Messages.Errors);
    }

    [Fact]
    public async Task TryBack_TheUnsavedQuestionFails_StaysAndShowsOneError()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.AskFailure = new InvalidOperationException("broken");

        var left = await f.Shell.TryRequestBackAsync();

        Assert.False(left);
        Assert.Equal(0, f.Shutdown.Calls);
        Assert.Equal(["The operation could not be done."], f.Messages.Errors);
    }

    [Fact]
    public async Task Back_WhileAToolFlyoutIsOpen_ClosesOnlyTheFlyout()
    {
        var f = new Fixture();
        f.Edit();
        f.Shell.Panel.Toggle(InspectorTab.Layers);
        f.Shell.Column.Groups[3].OpenFlyoutCommand.Execute(null);

        var left = await f.Shell.RequestBackAsync();

        Assert.False(left);
        Assert.False(f.Shell.Column.HasOpenFlyout);
        Assert.True(f.Shell.Panel.IsOpen);
        Assert.Empty(f.Messages.Questions);
        Assert.Equal(0, f.Shutdown.Calls);
    }

    [Fact]
    public async Task Back_WhileThePanelIsOpen_ClosesOnlyThePanel_EvenWithUnsavedChanges()
    {
        var f = new Fixture();
        f.Edit();
        f.Shell.Panel.Toggle(InspectorTab.History);

        var left = await f.Shell.RequestBackAsync();

        Assert.False(left);
        Assert.False(f.Shell.Panel.IsOpen);
        Assert.Empty(f.Messages.Questions);
        Assert.Equal(0, f.Shutdown.Calls);
    }

    [Fact]
    public async Task Back_WithNothingOpen_LeavesLikeBefore()
    {
        var f = new Fixture();

        var left = await f.Shell.RequestBackAsync();

        Assert.True(left);
        Assert.Equal(1, f.Shutdown.Calls);
    }

    [Fact]
    public async Task Back_WithNothingOpenAndUnsavedChanges_Asks()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Cancel;

        var left = await f.Shell.RequestBackAsync();

        Assert.False(left);
        Assert.Single(f.Messages.Questions);
    }

    [Fact]
    public async Task Back_PressedRepeatedly_ClosesFlyoutThenPanelThenAsks()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Cancel;
        f.Shell.Panel.Toggle(InspectorTab.Properties);
        f.Shell.Column.Groups[1].OpenFlyoutCommand.Execute(null);

        await f.Shell.RequestBackAsync();
        Assert.False(f.Shell.Column.HasOpenFlyout);
        Assert.True(f.Shell.Panel.IsOpen);

        await f.Shell.RequestBackAsync();
        Assert.False(f.Shell.Panel.IsOpen);
        Assert.Empty(f.Messages.Questions);

        await f.Shell.RequestBackAsync();
        Assert.Single(f.Messages.Questions);
    }

    // ---- a new document while things are open ----

    [Fact]
    public async Task NewDocument_WhileTheLayersPanelIsOpen_KeepsThePanelAndShowsTheNewDocument()
    {
        var f = new Fixture();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.DontSave;
        f.Shell.Panel.Toggle(InspectorTab.Layers);

        await f.Shell.NewCommand.ExecuteAsync(null);

        Assert.True(f.Shell.Panel.IsOpen);
        Assert.Same(f.Shell.Inspector.Layers, f.Shell.Panel.Content);
        Assert.Empty(f.Shell.State.Document.Layers.SelectMany(l => l.Shapes));
    }

    // ---- recent files ----

    private static async Task<MemorySettingsStore> SaveOneFile(bool remembersRecentFiles)
    {
        var localizer = FakeLocalizer.Create();
        var settings = new MemorySettingsStore();
        var dialogs = new FakeFileDialogs();
        dialogs.SaveAnswers.Enqueue("/work/plan.p2s");
        var services = new FileServices(settings, new FakeFileStore(), dialogs, new FakeMessageDialogs(), remembersRecentFiles);
        var shell = new TabletShellViewModel(new FakeShutdown(), localizer, services, new MessageDialogHost());

        await shell.SaveAsCommand.ExecuteAsync(null);

        return settings;
    }

    [Fact]
    public async Task RecentFiles_AreKept_ByDefault()
    {
        var settings = await SaveOneFile(remembersRecentFiles: true);

        Assert.NotNull(settings.Current.RecentFiles);
    }

    [Fact]
    public async Task RecentFiles_AreNotKept_WhenTheServicesSayNo()
    {
        var settings = await SaveOneFile(remembersRecentFiles: false);

        Assert.Null(settings.Current.RecentFiles);
    }

    [Fact]
    public void TheOverflowMenu_HasNoRecentFilesList()
    {
        var f = new Fixture();

        var all = f.Shell.OverflowMenu.Items.SelectMany(i => new[] { i }.Concat(i.Descendants())).Select(i => i.PlainHeader);

        Assert.DoesNotContain(all, h => h.Contains("Recent"));
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var f = new Fixture();
        var services = new FileServices(new MemorySettingsStore(), f.Files, f.Dialogs, f.Messages);

        Assert.Throws<ArgumentNullException>(() => new TabletShellViewModel(null!, f.Localizer, services, f.Host));
        Assert.Throws<ArgumentNullException>(() => new TabletShellViewModel(f.Shutdown, null!, services, f.Host));
        Assert.Throws<ArgumentNullException>(() => new TabletShellViewModel(f.Shutdown, f.Localizer, null!, f.Host));
        Assert.Throws<ArgumentNullException>(() => new TabletShellViewModel(f.Shutdown, f.Localizer, services, null!));
    }
}
