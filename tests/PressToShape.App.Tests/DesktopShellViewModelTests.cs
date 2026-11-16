using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using Xunit;

namespace PressToShape.App.Tests;

public class DesktopShellViewModelTests
{
    private sealed class FakeShutdown : IAppShutdown
    {
        public int Calls { get; private set; }

        public void Shutdown() => Calls++;
    }

    private static MenuItemViewModel FindItem(DesktopShellViewModel shell, string header)
    {
        static System.Collections.Generic.IEnumerable<MenuItemViewModel> Flatten(System.Collections.Generic.IEnumerable<MenuItemViewModel> items) =>
            items.SelectMany(i => new[] { i }.Concat(Flatten(i.Children)));

        return Flatten(shell.MenuBar.Menus).Single(i => i.Header.Replace("_", string.Empty) == header);
    }

    [Fact]
    public void Shell_CreatesAllRegions()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        Assert.NotNull(shell.MenuBar);
        Assert.NotNull(shell.Toolbar);
        Assert.NotNull(shell.Toolbox);
        Assert.NotNull(shell.Canvas);
        Assert.NotNull(shell.Inspector);
        Assert.NotNull(shell.StatusBar);
    }

    [Fact]
    public void ToolSelectedInToolbox_IsReflectedInToolbar()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Node).SelectCommand.Execute(null);

        Assert.True(shell.Toolbar.IsNodeToolActive);
        Assert.False(shell.Toolbar.IsSelectToolActive);
    }

    [Fact]
    public void ToolToggledInToolbar_IsReflectedInToolbox()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        shell.Toolbar.IsNodeToolActive = true;

        Assert.Equal(Tool.Node, shell.Toolbox.ActiveTool);
        Assert.Equal(Tool.Node, shell.Toolbox.Tools.Single(t => t.IsActive).Definition.Tool);
    }

    [Fact]
    public void ZoomFromEveryRoute_ShowsTheSameValue()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        // menu route
        FindItem(shell, "Zoom In").Command!.Execute(null);
        Assert.Equal("150%", shell.StatusBar.ZoomText);
        Assert.Equal(150, shell.Canvas.ZoomPercent);

        // status bar route
        shell.StatusBar.ZoomOutCommand.Execute(null);
        shell.StatusBar.ZoomOutCommand.Execute(null);
        Assert.Equal("75%", shell.StatusBar.ZoomText);
        Assert.Equal(75, shell.Canvas.ZoomPercent);
        Assert.Equal(75, shell.State.ZoomPercent);

        // shortcut-style route through the state
        shell.State.SetZoom(200);
        Assert.Equal("200%", shell.StatusBar.ZoomText);
        Assert.Equal(200, shell.Canvas.ZoomPercent);
    }

    [Fact]
    public void FitToWindow_UsesTheCanvasViewport_FromMenuAndStatusBar()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        shell.Canvas.ViewportWidth = 648;  // available 600 x 400 with the default 24 px margin
        shell.Canvas.ViewportHeight = 448;

        shell.StatusBar.FitToWindowCommand.Execute(null);
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);

        shell.State.SetZoom(300);
        FindItem(shell, "Fit Artboard in Window").Command!.Execute(null);
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);
    }

    [Fact]
    public void FitToWindow_WithoutViewport_LeavesZoomUnchanged()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        shell.State.SetZoom(200);

        shell.FitToWindow();

        Assert.Equal(200, shell.State.ZoomPercent);
    }

    [Fact]
    public void WindowMenu_SwitchesTheInspectorTab()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        FindItem(shell, "Layers").Command!.Execute(null);

        Assert.Equal(InspectorTab.Layers, shell.Inspector.SelectedTab);
    }

    [Fact]
    public void Quit_CallsTheInjectedShutdown()
    {
        var shutdown = new FakeShutdown();
        var shell = new DesktopShellViewModel(shutdown, FakeLocalizer.Create(), TestFileServices.Create());

        FindItem(shell, "Quit").Command!.Execute(null);

        Assert.Equal(1, shutdown.Calls);
    }

    [Fact]
    public void SelectionState_FlowsToAllRegions()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        TestSelection.AddAndSelect(shell.State);

        Assert.True(shell.Inspector.Properties.HasSelection);
        Assert.Equal("1 selected", shell.StatusBar.SelectionSummary);
    }

    [Fact]
    public void ShortcutCommands_DoWhatTheMenuItemsDo()
    {
        var shutdown = new FakeShutdown();
        var shell = new DesktopShellViewModel(shutdown, FakeLocalizer.Create(), TestFileServices.Create());
        shell.Canvas.ViewportWidth = 648;
        shell.Canvas.ViewportHeight = 448;
        shell.State.ZoomToActualSize(); // the first viewport size fitted the artboard already

        shell.ZoomInCommand.Execute(null);
        Assert.Equal(150, shell.State.ZoomPercent);

        shell.ZoomOutCommand.Execute(null);
        Assert.Equal(100, shell.State.ZoomPercent);

        shell.FitArtboardCommand.Execute(null);
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);

        shell.ActualSizeCommand.Execute(null);
        Assert.Equal(100, shell.State.ZoomPercent);

        shell.QuitCommand.Execute(null);
        Assert.Equal(1, shutdown.Calls);
    }

    [Fact]
    public void FirstViewportSize_FitsTheArtboardOnce()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        Assert.Equal(100, shell.State.ZoomPercent);

        shell.Canvas.ViewportWidth = 648;
        Assert.Equal(100, shell.State.ZoomPercent); // height still unknown, nothing to fit yet

        shell.Canvas.ViewportHeight = 448;
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);

        // later size changes (for example resizing the window) do not change the zoom
        shell.State.SetZoom(200);
        shell.Canvas.ViewportWidth = 1000;
        Assert.Equal(200, shell.State.ZoomPercent);
    }

    [Fact]
    public void LanguageSwitch_ChangesTextInEveryRegion()
    {
        var localizer = FakeLocalizer.Create("en", new System.Collections.Generic.Dictionary<string, string>
        {
            ["_File"] = "_Datei",
            ["Select"] = "Auswählen",
            ["Undo"] = "Rückgängig",
            ["Nothing selected"] = "Nichts ausgewählt",
            ["No assets yet"] = "Noch keine Assets",
        });
        var shell = new DesktopShellViewModel(new FakeShutdown(), localizer, TestFileServices.Create());

        localizer.SetLanguage("de");

        Assert.Equal("_Datei", shell.MenuBar.Menus[0].Header);
        Assert.Equal("Auswählen", shell.Toolbox.Tools[0].Name);
        Assert.Equal("Rückgängig", shell.Toolbar.HistoryActions[0].Name);
        Assert.Equal("Nichts ausgewählt", shell.StatusBar.SelectionSummary);
        Assert.Equal("Noch keine Assets", shell.Inspector.Assets.Message);
    }

    [Fact]
    public void Shell_Starts_WithOneEmptyLayerNamedInTheCurrentLanguage()
    {
        var german = new System.Collections.Generic.Dictionary<string, string> { ["Layer 1"] = "Ebene 1" };
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create("de", german), TestFileServices.Create());

        var layer = Assert.Single(shell.State.Document.Layers);
        Assert.Equal("Ebene 1", layer.Name);
        Assert.Empty(layer.Shapes);
        Assert.Equal(1200, shell.State.ArtboardWidth);
        Assert.Equal(800, shell.State.ArtboardHeight);
        Assert.Equal(layer.Id, shell.State.ActiveLayerId);
    }

    [Fact]
    public void Shell_Starts_WithTheSameDocumentInCanvasAndLayersPanel()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

        Assert.Same(shell.State.Document, shell.Canvas.Document);
        Assert.Equal(new[] { "Layer 1" }, shell.Inspector.Layers.Rows.Where(r => r.IsLayer).Select(r => r.Name));
        Assert.Equal(shell.State.ActiveLayerId, shell.Inspector.Layers.Rows.Single(r => r.IsActiveLayer).Id);
    }

    [Fact]
    public void HidingALayerInThePanel_ChangesTheCanvasDocument()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        var before = shell.Canvas.Document;

        shell.Inspector.Layers.Rows[0].ToggleVisibilityCommand.Execute(null);

        Assert.NotSame(before, shell.Canvas.Document);
        Assert.False(shell.Canvas.Document.Layers[0].IsVisible);
        Assert.True(before.Layers[0].IsVisible);
    }

    [Fact]
    public void DrawingARectangle_ShowsInDocumentLayersPanelAndSelection()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        shell.Canvas.ViewportWidth = 648;  // fits the 1200 x 800 artboard at 50%, origin (24, 24)
        shell.Canvas.ViewportHeight = 448;
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Rectangle).SelectCommand.Execute(null);

        shell.Canvas.PointerPressed(24 + 50, 24 + 50);
        shell.Canvas.PointerMoved(24 + 150, 24 + 100);
        shell.Canvas.PointerReleased(24 + 250, 24 + 150);

        var shape = Assert.Single(shell.State.Document.Layers[0].Shapes);
        Assert.Equal((100, 100, 400, 200), (shape.X, shape.Y, shape.Width, shape.Height));
        Assert.Same(shell.State.Document, shell.Canvas.Document);
        Assert.Equal(shape.Id, shell.State.SelectedShapeId);
        Assert.True(shell.Inspector.Properties.HasSelection);
        Assert.NotEmpty(shell.Canvas.Overlay.Outlines);
        Assert.Equal(Tool.Rectangle, shell.Toolbox.ActiveTool);
        Assert.Equal(shell.State.Document.Layers[0].Id, shell.Inspector.Layers.Rows.Single(r => r.IsActiveLayer).Id);
    }

    private sealed class FileShell
    {
        public FileShell(Localizer? localizer = null)
        {
            Shell = new DesktopShellViewModel(new FakeShutdown(), localizer ?? FakeLocalizer.Create(), TestFileServices.Create(Files, Dialogs, Messages, Settings));
        }

        public FakeFileStore Files { get; } = new();

        public FakeFileDialogs Dialogs { get; } = new();

        public FakeMessageDialogs Messages { get; } = new();

        public MemorySettingsStore Settings { get; } = new();

        public DesktopShellViewModel Shell { get; }

        public void Edit() =>
            Shell.State.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(Shell.State.ActiveLayerId, new PressToShape.Core.Shapes.RectangleShape { Width = 5, Height = 5 }));
    }

    [Fact]
    public void WindowTitle_Untitled_ShowsUntitledAndTheProductName()
    {
        var f = new FileShell();

        Assert.Equal("Untitled — PressToShape", f.Shell.WindowTitle);
    }

    [Fact]
    public void WindowTitle_AfterAnEdit_HasAnAsterisk()
    {
        var f = new FileShell();
        var raised = new List<string?>();
        f.Shell.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Edit();

        Assert.Equal("* Untitled — PressToShape", f.Shell.WindowTitle);
        Assert.Contains(nameof(DesktopShellViewModel.WindowTitle), raised);
    }

    [Fact]
    public async Task WindowTitle_AfterSaveAs_ShowsTheFileNameWithoutAsterisk()
    {
        var f = new FileShell();
        f.Edit();
        f.Dialogs.SaveAnswers.Enqueue("/work/plan.p2s");

        await f.Shell.SaveAsCommand.ExecuteAsync(null);

        Assert.Equal("plan.p2s — PressToShape", f.Shell.WindowTitle);
    }

    [Fact]
    public async Task WindowTitle_AfterOpen_ShowsTheOpenedFile()
    {
        var f = new FileShell();
        f.Files.Files["/work/old.p2s"] = PressToShape.Core.IO.ProjectFile.Write(PressToShape.Core.Documents.Document.Create("L"));
        f.Dialogs.OpenAnswers.Enqueue("/work/old.p2s");

        await f.Shell.OpenCommand.ExecuteAsync(null);

        Assert.Equal("old.p2s — PressToShape", f.Shell.WindowTitle);
        Assert.Equal("L", f.Shell.State.Document.Layers[0].Name);
    }

    [Fact]
    public void WindowTitle_FollowsTheLanguage()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Untitled"] = "Unbenannt" });
        var f = new FileShell(localizer);
        var raised = new List<string?>();
        f.Shell.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        localizer.SetLanguage("de");

        Assert.Equal("Unbenannt — PressToShape", f.Shell.WindowTitle);
        Assert.Contains(nameof(DesktopShellViewModel.WindowTitle), raised);
    }

    [Fact]
    public async Task Commands_CallTheService()
    {
        var f = new FileShell();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.DontSave;

        await f.Shell.NewCommand.ExecuteAsync(null);
        Assert.Single(f.Shell.State.Document.Layers);
        Assert.Empty(f.Shell.State.Document.Layers[0].Shapes);

        f.Dialogs.ExportAnswers.Enqueue("/out/x.svg");
        await f.Shell.ExportSvgCommand.ExecuteAsync(null);
        Assert.True(f.Files.Files.ContainsKey("/out/x.svg"));

        f.Dialogs.SaveAnswers.Enqueue("/work/s.p2s");
        await f.Shell.SaveCommand.ExecuteAsync(null);
        Assert.True(f.Files.Files.ContainsKey("/work/s.p2s"));
    }

    [Fact]
    public async Task OpenRecentMenuEntry_OpensTheFile()
    {
        var f = new FileShell();
        f.Files.Files["/work/a.p2s"] = PressToShape.Core.IO.ProjectFile.Write(PressToShape.Core.Documents.Document.Create("FromRecent"));
        f.Dialogs.OpenAnswers.Enqueue("/work/a.p2s");
        await f.Shell.OpenCommand.ExecuteAsync(null);
        await f.Shell.NewCommand.ExecuteAsync(null);

        var entry = FindItem(f.Shell, "Open Recent").Children.Single();
        await ((CommunityToolkit.Mvvm.Input.IAsyncRelayCommand)entry.Command!).ExecuteAsync(null);

        Assert.Equal("a.p2s", entry.Header);
        Assert.Equal("FromRecent", f.Shell.State.Document.Layers[0].Name);
    }

    [Fact]
    public async Task ConfirmCloseAsync_Clean_IsTrueWithoutAQuestion()
    {
        var f = new FileShell();

        Assert.True(await f.Shell.ConfirmCloseAsync());
        Assert.Empty(f.Messages.Questions);
    }

    [Fact]
    public async Task ConfirmCloseAsync_Dirty_FollowsTheAnswer()
    {
        var f = new FileShell();
        f.Edit();

        f.Messages.Answer = UnsavedChoice.Cancel;
        Assert.False(await f.Shell.ConfirmCloseAsync());

        f.Messages.Answer = UnsavedChoice.DontSave;
        Assert.True(await f.Shell.ConfirmCloseAsync());

        Assert.Equal(2, f.Messages.Questions.Count);
    }

    [Fact]
    public async Task ConfirmCloseAsync_SaveAnswer_SavesFirst()
    {
        var f = new FileShell();
        f.Edit();
        f.Messages.Answer = UnsavedChoice.Save;
        f.Dialogs.SaveAnswers.Enqueue("/work/c.p2s");

        Assert.True(await f.Shell.ConfirmCloseAsync());
        Assert.True(f.Files.Files.ContainsKey("/work/c.p2s"));
    }

    [Fact]
    public async Task OpeningAFile_FitsTheArtboardToTheWindow()
    {
        var f = new FileShell();
        f.Shell.Canvas.ViewportWidth = 648;
        f.Shell.Canvas.ViewportHeight = 448;
        f.Shell.State.SetZoom(300);
        f.Files.Files["/work/a.p2s"] = PressToShape.Core.IO.ProjectFile.Write(PressToShape.Core.Documents.Document.Create("L"));
        f.Dialogs.OpenAnswers.Enqueue("/work/a.p2s");

        await f.Shell.OpenCommand.ExecuteAsync(null);

        Assert.Equal(50, f.Shell.State.ZoomPercent, precision: 6);
    }

    [Fact]
    public async Task RecentFiles_ArePersistedInTheSettings()
    {
        var f = new FileShell();
        f.Dialogs.SaveAnswers.Enqueue("/work/a.p2s");

        await f.Shell.SaveAsCommand.ExecuteAsync(null);

        Assert.Equal("/work/a.p2s", f.Settings.Current.RecentFiles!.Value[0].GetString());
    }

    [Fact]
    public void RecentFiles_FromTheSettings_AppearInTheMenuAtStart()
    {
        var settings = new MemorySettingsStore { Current = System.Text.Json.JsonSerializer.Deserialize<PressToShape.App.Settings.AppSettings>("{\"RecentFiles\":[\"/x/one.p2s\"]}")! };
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create(settings: settings));

        Assert.Equal("one.p2s", FindItem(shell, "Open Recent").Children.Single().Header);
    }

    // ---- undo and redo ----

    private static MenuItemViewModel EditItem(DesktopShellViewModel shell, string header) => FindItem(shell, header);

    private static void DrawRectangle(DesktopShellViewModel shell, double x1, double y1, double x2, double y2)
    {
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Rectangle).SelectCommand.Execute(null);
        shell.Canvas.PointerPressed(24 + x1, 24 + y1);
        shell.Canvas.PointerMoved(24 + (x1 + x2) / 2, 24 + (y1 + y2) / 2);
        shell.Canvas.PointerReleased(24 + x2, 24 + y2);
    }

    private static DesktopShellViewModel NewFittedShell(FakeShutdown? shutdown = null)
    {
        var shell = new DesktopShellViewModel(shutdown ?? new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        shell.Canvas.ViewportWidth = 648;  // fits the artboard at 50%, origin (24, 24)
        shell.Canvas.ViewportHeight = 448;
        return shell;
    }

    [Fact]
    public void UndoAndRedo_AtStart_AreDisabledInMenuToolbarAndAsCommands()
    {
        var shell = NewFittedShell();

        Assert.False(shell.UndoCommand.CanExecute(null));
        Assert.False(shell.RedoCommand.CanExecute(null));
        Assert.False(EditItem(shell, "Undo").IsEnabled);
        Assert.False(EditItem(shell, "Redo").IsEnabled);
        Assert.False(shell.Toolbar.HistoryActions[0].IsEnabled);
        Assert.False(shell.Toolbar.HistoryActions[1].IsEnabled);
    }

    [Fact]
    public void UndoAndRedo_EnableAndDisableWithTheHistory()
    {
        var shell = NewFittedShell();
        var raised = new List<string?>();
        EditItem(shell, "Undo").PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        DrawRectangle(shell, 100, 100, 300, 200);

        Assert.True(shell.UndoCommand.CanExecute(null));
        Assert.True(EditItem(shell, "Undo").IsEnabled);
        Assert.True(shell.Toolbar.HistoryActions[0].IsEnabled);
        Assert.False(EditItem(shell, "Redo").IsEnabled);
        Assert.Contains(nameof(MenuItemViewModel.IsEnabled), raised);

        EditItem(shell, "Undo").Command!.Execute(null);

        Assert.False(EditItem(shell, "Undo").IsEnabled);
        Assert.True(EditItem(shell, "Redo").IsEnabled);
        Assert.True(shell.Toolbar.HistoryActions[1].IsEnabled);
        Assert.Empty(shell.State.Document.Layers[0].Shapes);

        shell.Toolbar.HistoryActions[1].Command!.Execute(null);

        Assert.Single(shell.State.Document.Layers[0].Shapes);
        Assert.False(EditItem(shell, "Redo").IsEnabled);
    }

    [Fact]
    public void UndoAndRedo_AreDisabledWhileADragIsInProgress()
    {
        var shell = NewFittedShell();
        DrawRectangle(shell, 100, 100, 300, 200);
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Rectangle).SelectCommand.Execute(null);

        shell.Canvas.PointerPressed(24 + 400, 24 + 300);

        Assert.False(shell.UndoCommand.CanExecute(null));
        Assert.False(EditItem(shell, "Undo").IsEnabled);
        Assert.False(shell.Toolbar.HistoryActions[0].IsEnabled);

        shell.Canvas.PointerReleased(24 + 400, 24 + 300);

        Assert.True(shell.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void UndoRedoAndJump_EndToEnd_ThroughCanvasLayersHistoryAndTitle()
    {
        var shell = NewFittedShell();
        DrawRectangle(shell, 100, 100, 300, 200);
        DrawRectangle(shell, 400, 100, 600, 200);
        var twoShapes = shell.State.Document;
        Assert.Equal("* Untitled — PressToShape", shell.WindowTitle);
        Assert.Equal(new[] { "Start", "Draw rectangle", "Draw rectangle" }, shell.Inspector.History.Items.Select(i => i.Name));

        shell.UndoCommand.Execute(null);
        shell.UndoCommand.Execute(null);

        Assert.Empty(shell.State.Document.Layers[0].Shapes);
        Assert.Same(shell.State.Document, shell.Canvas.Document);
        Assert.Equal("Untitled — PressToShape", shell.WindowTitle); // back to the start instance: clean
        Assert.False(shell.State.HasSelection);
        Assert.True(shell.Inspector.History.Items[0].IsCurrent);
        Assert.True(shell.Inspector.History.Items[2].IsRedoable);

        shell.RedoCommand.Execute(null);
        Assert.Single(shell.State.Document.Layers[0].Shapes);
        Assert.Equal("* Untitled — PressToShape", shell.WindowTitle);

        shell.Inspector.History.SelectedItem = shell.Inspector.History.Items[2];
        Assert.Same(twoShapes, shell.State.Document);

        shell.Inspector.History.SelectedItem = shell.Inspector.History.Items[0];
        Assert.Empty(shell.State.Document.Layers[0].Shapes);
        Assert.Equal("Untitled — PressToShape", shell.WindowTitle);
    }

    [Fact]
    public void Undo_AfterAMoveDrag_RestoresThePositionInOneStep()
    {
        var shell = NewFittedShell();
        DrawRectangle(shell, 100, 100, 300, 200);
        var shape = shell.State.Document.Layers[0].Shapes[0];
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Select).SelectCommand.Execute(null);

        shell.Canvas.PointerPressed(24 + 150, 24 + 150);
        for (var i = 1; i <= 30; i++)
        {
            shell.Canvas.PointerMoved(24 + 150 + i, 24 + 150);
        }

        shell.Canvas.PointerReleased(24 + 180, 24 + 150);
        Assert.Equal(new[] { "Start", "Draw rectangle", "Move shape" }, shell.Inspector.History.Items.Select(i => i.Name));

        shell.UndoCommand.Execute(null);

        Assert.Equal(shape.X, shell.State.Document.Layers[0].Shapes[0].X);
    }

    [Fact]
    public void LayerToggle_IsUndoableFromTheLayersPanel()
    {
        var shell = NewFittedShell();

        shell.Inspector.Layers.Rows[0].ToggleVisibilityCommand.Execute(null);
        Assert.False(shell.State.Document.Layers[0].IsVisible);

        shell.UndoCommand.Execute(null);

        Assert.True(shell.State.Document.Layers[0].IsVisible);
        Assert.True(shell.Inspector.Layers.Rows[0].IsVisible);
    }

    [Fact]
    public async Task NewAndOpen_ClearTheHistoryAndSaveKeepsIt()
    {
        var files = new FakeFileStore();
        var dialogs = new FakeFileDialogs();
        var messages = new FakeMessageDialogs { Answer = UnsavedChoice.DontSave };
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create(files, dialogs, messages));
        shell.Canvas.ViewportWidth = 648;
        shell.Canvas.ViewportHeight = 448;
        DrawRectangle(shell, 100, 100, 300, 200);
        dialogs.SaveAnswers.Enqueue("/work/a.p2s");

        await shell.SaveAsCommand.ExecuteAsync(null);
        Assert.True(shell.UndoCommand.CanExecute(null));
        Assert.Equal("a.p2s — PressToShape", shell.WindowTitle);

        shell.UndoCommand.Execute(null);
        Assert.Equal("* a.p2s — PressToShape", shell.WindowTitle);
        shell.RedoCommand.Execute(null);
        Assert.Equal("a.p2s — PressToShape", shell.WindowTitle);

        await shell.NewCommand.ExecuteAsync(null);

        Assert.False(shell.UndoCommand.CanExecute(null));
        Assert.False(shell.RedoCommand.CanExecute(null));
        Assert.Equal(new[] { "Start" }, shell.Inspector.History.Items.Select(i => i.Name));
    }

    [Fact]
    public void ShortcutCommands_ExistForUndoAndRedo()
    {
        var shell = NewFittedShell();
        DrawRectangle(shell, 100, 100, 300, 200);

        shell.UndoCommand.Execute(null);
        Assert.Empty(shell.State.Document.Layers[0].Shapes);

        shell.RedoCommand.Execute(null);
        Assert.Single(shell.State.Document.Layers[0].Shapes);
    }

    [Fact]
    public async Task NewAndOpen_CenterTheArtboardAgain()
    {
        var files = new FakeFileStore();
        var dialogs = new FakeFileDialogs();
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create(files, dialogs));
        shell.Canvas.ViewportWidth = 648;
        shell.Canvas.ViewportHeight = 448;
        files.Files["/work/a.p2s"] = PressToShape.Core.IO.ProjectFile.Write(PressToShape.Core.Documents.Document.Create("L"));

        shell.State.SetZoom(200);
        shell.State.SetPan(150, 90);
        await shell.NewCommand.ExecuteAsync(null);
        Assert.Equal((0, 0), (shell.State.PanX, shell.State.PanY));
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);

        shell.State.SetZoom(200);
        shell.State.SetPan(-150, -90);
        dialogs.OpenAnswers.Enqueue("/work/a.p2s");
        await shell.OpenCommand.ExecuteAsync(null);
        Assert.Equal((0, 0), (shell.State.PanX, shell.State.PanY));
    }

    [Fact]
    public void FitArtboardMenuItem_ResetsTheOffset()
    {
        var shell = NewFittedShell();
        shell.State.SetZoom(300);
        shell.State.SetPan(200, 100);

        FindItem(shell, "Fit Artboard in Window").Command!.Execute(null);

        Assert.Equal((0, 0), (shell.State.PanX, shell.State.PanY));
        Assert.Equal(50, shell.State.ZoomPercent, precision: 6);
    }

    // ---- view: zoom routes, pan and wheel together ----

    // The middle of the canvas area of NewFittedShell is (324, 224).
    private static double DocumentXAt(DesktopShellViewModel shell, double screenX) =>
        CanvasMapping.ToDocument(screenX, shell.Canvas.OriginX, shell.Canvas.ZoomPercent);

    private static double DocumentYAt(DesktopShellViewModel shell, double screenY) =>
        CanvasMapping.ToDocument(screenY, shell.Canvas.OriginY, shell.Canvas.ZoomPercent);

    private static void PanAwayFromTheCenter(DesktopShellViewModel shell)
    {
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Hand).SelectCommand.Execute(null);
        shell.Canvas.PointerPressed(200, 200);
        shell.Canvas.PointerMoved(260, 170);
        shell.Canvas.PointerReleased(260, 170);
    }

    [Fact]
    public void EveryZoomRoute_KeepsTheCenterOfTheCanvasFixed()
    {
        var routes = new (string Name, System.Action<DesktopShellViewModel> Run)[]
        {
            ("menu zoom in", s => FindItem(s, "Zoom In").Command!.Execute(null)),
            ("menu zoom out", s => FindItem(s, "Zoom Out").Command!.Execute(null)),
            ("menu actual size", s => FindItem(s, "Actual Size (100%)").Command!.Execute(null)),
            ("status bar zoom in", s => s.StatusBar.ZoomInCommand.Execute(null)),
            ("status bar zoom out", s => s.StatusBar.ZoomOutCommand.Execute(null)),
            ("status bar preset 400", s => s.StatusBar.ZoomPresets.Single(p => p.Percent == 400).SelectCommand.Execute(null)),
            ("status bar preset 25", s => s.StatusBar.ZoomPresets.Single(p => p.Percent == 25).SelectCommand.Execute(null)),
            ("shortcut zoom in", s => s.ZoomInCommand.Execute(null)),
            ("shortcut zoom out", s => s.ZoomOutCommand.Execute(null)),
            ("shortcut actual size", s => s.ActualSizeCommand.Execute(null)),
        };

        foreach (var (name, run) in routes)
        {
            var shell = NewFittedShell();
            PanAwayFromTheCenter(shell);
            var (beforeX, beforeY) = (DocumentXAt(shell, 324), DocumentYAt(shell, 224));
            var zoomBefore = shell.State.ZoomPercent;

            run(shell);

            Assert.True(shell.State.ZoomPercent != zoomBefore, name);
            Assert.True(System.Math.Abs(beforeX - DocumentXAt(shell, 324)) < 1e-6, name);
            Assert.True(System.Math.Abs(beforeY - DocumentYAt(shell, 224)) < 1e-6, name);
        }
    }

    [Fact]
    public void FitRoutes_ResetTheOffset_ActualSizeDoesNot()
    {
        var shell = NewFittedShell();
        PanAwayFromTheCenter(shell);
        Assert.NotEqual(0, shell.State.PanX);

        FindItem(shell, "Actual Size (100%)").Command!.Execute(null);
        Assert.True(shell.State.PanX != 0 || shell.State.PanY != 0);

        shell.StatusBar.FitToWindowCommand.Execute(null);
        Assert.Equal((0, 0), (shell.State.PanX, shell.State.PanY));

        PanAwayFromTheCenter(shell);
        shell.FitArtboardCommand.Execute(null);
        Assert.Equal((0, 0), (shell.State.PanX, shell.State.PanY));
    }

    [Fact]
    public void WheelZoomAndPan_ThenDrawing_LandsUnderThePointerInDocumentCoordinates()
    {
        var shell = NewFittedShell();
        shell.Canvas.Wheel(0, 3, control: true, shift: false, 200, 150);
        shell.Canvas.Wheel(0, -1, control: false, shift: false, 200, 150);
        shell.Canvas.Wheel(1, 0, control: false, shift: false, 200, 150);
        var zoom = shell.Canvas.ZoomPercent;
        Assert.True(zoom > 50);
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Rectangle).SelectCommand.Execute(null);
        var (ox, oy) = (shell.Canvas.OriginX, shell.Canvas.OriginY);

        shell.Canvas.PointerPressed(ox + 100, oy + 50);
        shell.Canvas.PointerMoved(ox + 200, oy + 100);
        shell.Canvas.PointerReleased(ox + 300, oy + 150);

        var shape = Assert.Single(shell.State.Document.Layers[0].Shapes);
        Assert.Equal(100 / (zoom / 100), shape.X, precision: 6);
        Assert.Equal(50 / (zoom / 100), shape.Y, precision: 6);
        Assert.Equal(200 / (zoom / 100), shape.Width, precision: 6);
    }

    [Fact]
    public void PanAndZoom_DoNotCreateHistoryOrDirtyState_AndUndoIsBlockedDuringAPan()
    {
        var shell = NewFittedShell();
        DrawRectangle(shell, 100, 100, 300, 200);
        var rows = shell.Inspector.History.Items.Count;
        var title = shell.WindowTitle;

        shell.Canvas.Wheel(0, 2, control: true, shift: false, 300, 200);
        shell.Canvas.Wheel(0, 1, control: false, shift: false, 300, 200);
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Hand).SelectCommand.Execute(null);
        shell.Canvas.PointerPressed(100, 100);
        shell.Canvas.PointerMoved(140, 130);

        Assert.False(shell.UndoCommand.CanExecute(null));
        Assert.False(FindItem(shell, "Undo").IsEnabled);
        Assert.False(shell.Toolbar.HistoryActions[0].IsEnabled);

        shell.Canvas.PointerReleased(140, 130);

        Assert.True(shell.UndoCommand.CanExecute(null));
        Assert.Equal(rows, shell.Inspector.History.Items.Count);
        Assert.Equal(title, shell.WindowTitle);
    }

    [Fact]
    public void HandAndZoomTools_DoNotEditTheDocument()
    {
        var shell = NewFittedShell();
        DrawRectangle(shell, 100, 100, 300, 200);
        shell.State.Select(null);
        var document = shell.State.Document;

        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Hand).SelectCommand.Execute(null);
        shell.Canvas.PointerPressed(24 + 150, 24 + 150);
        shell.Canvas.PointerMoved(24 + 200, 24 + 150);
        shell.Canvas.PointerReleased(24 + 200, 24 + 150);
        shell.Toolbox.Tools.Single(t => t.Definition.Tool == Tool.Zoom).SelectCommand.Execute(null);
        shell.Canvas.PointerPressed(24 + 150, 24 + 150);
        shell.Canvas.PointerPressed(24 + 150, 24 + 150, alt: true);

        Assert.Same(document, shell.State.Document);
        Assert.False(shell.State.HasSelection);
        Assert.Single(shell.State.History.Steps);
    }

    [Fact]
    public void ResizingTheCanvas_LimitsTheOffsetAgain()
    {
        var shell = NewFittedShell();
        shell.State.SetZoom(400);
        shell.State.SetPan(100000, 0);
        var wide = shell.State.PanX;

        shell.Canvas.ViewportWidth = 300;

        Assert.True(shell.State.PanX < wide);
        Assert.Equal(ViewGeometry.PanLimit(300, 4800), shell.State.PanX);
    }

    [Fact]
    public void ScrollBarValues_FollowTheStatusBarZoomAndTheWheel()
    {
        var shell = NewFittedShell();
        var before = shell.Canvas.HorizontalScrollMaximum;

        shell.StatusBar.ZoomPresets.Single(p => p.Percent == 400).SelectCommand.Execute(null);
        Assert.True(shell.Canvas.HorizontalScrollMaximum > before);

        shell.Canvas.Wheel(0, 1, control: false, shift: true, 300, 200);
        Assert.Equal(-ViewGeometry.WheelStep, shell.Canvas.HorizontalScrollValue, precision: 9);
    }

    [Fact]
    public void TheShell_SharesOneSelectionEditor_WithTheCanvasAndTheInspector()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        var ids = TestSelection.AddAndSelect(shell.State);

        // An arrow key on the canvas and a typed value in the Properties tab are both edits of the same editor: they are separate steps.
        shell.Canvas.KeyPressed(CanvasKey.Right, shift: false);
        shell.Inspector.Properties.Y.Text = "40";
        shell.Inspector.Properties.Y.Commit();

        var shape = shell.State.Document.FindShape(ids[0])!.Shape;
        Assert.Equal((1, 40), (shape.X, shape.Y));
        Assert.Equal(HistoryStepType.ChangePosition, shell.State.History.Steps[^1]);
        Assert.Equal(HistoryStepType.NudgeShapes, shell.State.History.Steps[^2]);
        Assert.NotNull(shell.Editor);
    }

    // ---- Edit commands ----

    private static DesktopShellViewModel NewShell() => new(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());

    private static IReadOnlyList<System.Windows.Input.ICommand> SelectionCommands(DesktopShellViewModel shell) =>
        [shell.CutCommand, shell.CopyCommand, shell.DuplicateCommand, shell.DeleteCommand, shell.DeselectCommand];

    [Fact]
    public void EditCommands_AreDisabledWithoutSelectionOrClipboard()
    {
        var shell = NewShell();

        Assert.All(SelectionCommands(shell), c => Assert.False(c.CanExecute(null)));
        Assert.False(shell.PasteCommand.CanExecute(null));
        Assert.False(shell.PasteInPlaceCommand.CanExecute(null));
        Assert.False(shell.SelectAllCommand.CanExecute(null));
    }

    [Fact]
    public void EditCommands_FollowTheSelection_AndNotify()
    {
        var shell = NewShell();
        var changed = new List<string>();
        shell.CopyCommand.CanExecuteChanged += (_, _) => changed.Add("copy");
        shell.DeselectCommand.CanExecuteChanged += (_, _) => changed.Add("deselect");

        TestSelection.AddAndSelect(shell.State);

        Assert.All(SelectionCommands(shell), c => Assert.True(c.CanExecute(null)));
        Assert.Contains("copy", changed);
        Assert.Contains("deselect", changed);
        Assert.True(shell.SelectAllCommand.CanExecute(null));

        shell.State.ClearSelection();
        Assert.All(SelectionCommands(shell), c => Assert.False(c.CanExecute(null)));
    }

    [Fact]
    public void CopyAndPaste_CommandsWork_AndPasteFollowsTheClipboardAndTheActiveLayer()
    {
        var shell = NewShell();
        TestSelection.AddAndSelect(shell.State);
        var pasteChanged = 0;
        shell.PasteCommand.CanExecuteChanged += (_, _) => pasteChanged++;

        shell.CopyCommand.Execute(null);

        Assert.True(shell.PasteCommand.CanExecute(null));
        Assert.True(shell.PasteInPlaceCommand.CanExecute(null));
        Assert.True(pasteChanged > 0);

        shell.PasteCommand.Execute(null);
        Assert.Equal(2, shell.State.Document.Layers.SelectMany(l => l.Shapes).Count());

        var layer = shell.State.ActiveLayerId;
        shell.State.Document = shell.State.Document.UpdateLayer(layer, l => l with { IsLocked = true });
        Assert.False(shell.PasteCommand.CanExecute(null));
        Assert.False(shell.PasteInPlaceCommand.CanExecute(null));
    }

    [Fact]
    public void CutDuplicateDeleteAndSelectCommands_DoTheirWork()
    {
        var shell = NewShell();
        TestSelection.AddAndSelect(shell.State, 2);

        shell.DuplicateCommand.Execute(null);
        Assert.Equal(4, shell.State.Document.Layers.SelectMany(l => l.Shapes).Count());

        shell.DeselectCommand.Execute(null);
        Assert.False(shell.State.HasSelection);

        shell.SelectAllCommand.Execute(null);
        Assert.Equal(4, shell.State.SelectedCount);

        shell.CutCommand.Execute(null);
        Assert.Empty(shell.State.Document.Layers.SelectMany(l => l.Shapes));
        Assert.True(shell.PasteCommand.CanExecute(null));

        shell.PasteInPlaceCommand.Execute(null);
        Assert.Equal(4, shell.State.Document.Layers.SelectMany(l => l.Shapes).Count());

        shell.DeleteCommand.Execute(null);
        Assert.Empty(shell.State.Document.Layers.SelectMany(l => l.Shapes));
    }

    [Fact]
    public void TheEditMenuItems_UseTheShellCommands()
    {
        var shell = NewShell();
        TestSelection.AddAndSelect(shell.State);
        var edit = shell.MenuBar.Menus.Single(m => m.Header.Replace("_", string.Empty) == "Edit");

        MenuItemViewModel Item(string header) => edit.Children.Single(c => c.Header.Replace("_", string.Empty) == header);

        Assert.True(Item("Copy").IsEnabled);
        Assert.True(Item("Select All").IsEnabled);
        Assert.False(Item("Paste").IsEnabled);

        Item("Copy").Command!.Execute(null);
        Assert.True(Item("Paste").IsEnabled);
    }
}

