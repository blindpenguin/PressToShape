using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Workspace;
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

        shell.State.HasSelection = true;
        shell.State.SelectionSummary = "1 selected";

        Assert.True(shell.Toolbar.HasSelection);
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
        Assert.Equal(new[] { "Layer 1" }, shell.Inspector.Layers.Items.Select(i => i.Name));
        Assert.Equal(shell.State.ActiveLayerId, shell.Inspector.Layers.SelectedItem!.LayerId);
    }

    [Fact]
    public void HidingALayerInThePanel_ChangesTheCanvasDocument()
    {
        var shell = new DesktopShellViewModel(new FakeShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        var before = shell.Canvas.Document;

        shell.Inspector.Layers.Items[0].ToggleVisibilityCommand.Execute(null);

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
        Assert.True(shell.Toolbar.HasSelection);
        Assert.True(shell.Inspector.Properties.HasSelection);
        Assert.NotNull(shell.Canvas.SelectionOutline);
        Assert.Equal(Tool.Rectangle, shell.Toolbox.ActiveTool);
        Assert.Equal(shell.State.Document.Layers[0].Id, shell.Inspector.Layers.SelectedItem!.LayerId);
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
            Shell.State.Document = Shell.State.Document.AddShape(Shell.State.ActiveLayerId, new PressToShape.Core.Shapes.RectangleShape { Width = 5, Height = 5 });
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
}
