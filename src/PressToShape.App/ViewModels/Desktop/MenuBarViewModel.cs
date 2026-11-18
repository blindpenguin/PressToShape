using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// The complete menu structure. The File actions, the Edit actions, the group, arrange, lock and hide items, the Layer menu, Quit, the zoom items, the Window items and the languages have behavior.
/// Everything else has no command and is therefore disabled until its feature exists.
/// </summary>
internal sealed class MenuBarViewModel : ViewModelBase
{
    public MenuBarViewModel(
        EditorState state,
        IAppShutdown shutdown,
        ILocalizer localizer,
        Action<InspectorTab> showTab,
        Action fitToWindow,
        FileMenuCommands files,
        EditMenuCommands edit,
        ObjectMenuCommands objects,
        LayerMenuCommands layers,
        PathMenuCommands paths,
        RecentFiles recent,
        ICommand undo,
        ICommand redo)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shutdown);
        ArgumentNullException.ThrowIfNull(showTab);
        ArgumentNullException.ThrowIfNull(fitToWindow);
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(edit);
        ArgumentNullException.ThrowIfNull(objects);
        ArgumentNullException.ThrowIfNull(layers);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(recent);
        ArgumentNullException.ThrowIfNull(undo);
        ArgumentNullException.ThrowIfNull(redo);

        var quit = new RelayCommand(shutdown.Shutdown);
        var zoomIn = new RelayCommand(state.ZoomIn);
        var zoomOut = new RelayCommand(state.ZoomOut);
        var actualSize = new RelayCommand(state.ZoomToActualSize);
        var fit = new RelayCommand(fitToWindow);
        RelayCommand ShowTab(InspectorTab tab) => new(() => showTab(tab));

        // Local helpers so the menu definition below stays compact. Every header is a message id marked with N(...).
        MenuItemViewModel Item(string header, string? gesture = null, ICommand? command = null) =>
            MenuItemViewModel.Item(localizer, header, gesture, command);
        MenuItemViewModel Submenu(string header, params MenuItemViewModel[] children) =>
            MenuItemViewModel.Submenu(localizer, header, children);
        MenuItemViewModel Separator() => MenuItemViewModel.Separator(localizer);

        // The switches of the View menu read the user settings and toggle them. They follow changes made elsewhere, such as the status bar.
        var snapping = state.Snapping;
        MenuItemViewModel Toggle(string header, string? gesture, Action toggle, Func<bool> isChecked) =>
            MenuItemViewModel.Toggle(localizer, header, gesture, new RelayCommand(toggle), isChecked);
        var showRulersItem = Toggle(N("Show _Rulers"), "Ctrl+R", () => snapping.ShowRulers = !snapping.ShowRulers, () => snapping.ShowRulers);
        var showGridItem = Toggle(N("Show _Grid"), null, () => snapping.ShowGrid = !snapping.ShowGrid, () => snapping.ShowGrid);
        var snapToGridItem = Toggle(N("_Snap to Grid"), null, () => snapping.SnapToGrid = !snapping.SnapToGrid, () => snapping.SnapToGrid);
        var snapToObjectsItem = Toggle(N("Snap to Ob_jects"), null, () => snapping.SnapToObjects = !snapping.SnapToObjects, () => snapping.SnapToObjects);
        MenuItemViewModel[] switches = [showRulersItem, showGridItem, snapToGridItem, snapToObjectsItem];
        snapping.Changed += (_, _) =>
        {
            foreach (var item in switches)
            {
                item.RefreshChecked();
            }
        };

        var recentItems = new ObservableCollection<MenuItemViewModel>();
        void RebuildRecent()
        {
            recentItems.Clear();
            foreach (var path in recent.Paths)
            {
                recentItems.Add(MenuItemViewModel.Fixed(localizer, FileNameOf(path), files.OpenRecent(path), Path.GetDirectoryName(path)));
            }

            if (recentItems.Count == 0)
            {
                recentItems.Add(Item(N("(No recent files)")));
            }
        }

        RebuildRecent();
        recent.Changed += (_, _) => RebuildRecent();

        Menus =
        [
            Submenu(N("_File"),
                Item(N("_New"), "Ctrl+N", files.New),
                Item(N("_Open…"), "Ctrl+O", files.Open),
                MenuItemViewModel.Submenu(localizer, N("Open _Recent"), recentItems),
                Separator(),
                Item(N("_Save"), "Ctrl+S", files.Save),
                Item(N("Save _As…"), "Ctrl+Shift+S", files.SaveAs),
                Item(N("_Export as SVG…"), "Ctrl+Shift+E", files.ExportSvg),
                Separator(),
                Item(N("_Document Settings…")),
                Separator(),
                Item(N("_Quit"), "Ctrl+Q", quit)),
            Submenu(N("_Edit"),
                Item(N("_Undo"), "Ctrl+Z", undo),
                Item(N("_Redo"), "Ctrl+Shift+Z", redo),
                Separator(),
                Item(N("Cu_t"), "Ctrl+X", edit.Cut),
                Item(N("_Copy"), "Ctrl+C", edit.Copy),
                Item(N("_Paste"), "Ctrl+V", edit.Paste),
                Item(N("Paste in Plac_e"), "Ctrl+Shift+V", edit.PasteInPlace),
                Item(N("_Duplicate"), "Ctrl+D", edit.Duplicate),
                Item(N("De_lete"), "Delete", edit.Delete),
                Separator(),
                Item(N("Select _All"), "Ctrl+A", edit.SelectAll),
                Item(N("De_select"), "Ctrl+Shift+A", edit.Deselect),
                Separator(),
                Item(N("Pre_ferences…"))),
            Submenu(N("_Object"),
                Submenu(N("_Transform"),
                    Item(N("Rotate 90° _Clockwise")),
                    Item(N("Rotate 90° C_ounterclockwise")),
                    Item(N("Flip _Horizontal")),
                    Item(N("Flip _Vertical"))),
                Submenu(N("_Arrange"),
                    Item(N("Bring to _Front"), "Home", objects.BringToFront),
                    Item(N("Bring F_orward"), "PageUp", objects.BringForward),
                    Item(N("Send _Backward"), "PageDown", objects.SendBackward),
                    Item(N("Send to Bac_k"), "End", objects.SendToBack)),
                Separator(),
                Item(N("_Group"), "Ctrl+G", objects.Group),
                Item(N("_Ungroup"), "Ctrl+Shift+G", objects.Ungroup),
                Separator(),
                Item(N("_Lock"), "Ctrl+2", objects.Lock),
                Item(N("Unloc_k All"), "Ctrl+Alt+2", objects.UnlockAll),
                Item(N("_Hide"), "Ctrl+3", objects.Hide),
                Item(N("_Show All"), "Ctrl+Alt+3", objects.ShowAll),
                Separator(),
                Submenu(N("Clipping _Mask"),
                    Item(N("_Make")),
                    Item(N("_Release")))),
            Submenu(N("_Layer"),
                Item(N("_Add Layer"), command: layers.Add),
                Item(N("Dupli_cate Layer"), command: layers.Duplicate),
                Item(N("_Delete Layer"), command: layers.Delete),
                Separator(),
                Item(N("_Merge Down"), command: layers.MergeDown),
                Separator(),
                Item(N("Move _Up"), command: layers.MoveUp),
                Item(N("Move Do_wn"), command: layers.MoveDown),
                Separator(),
                Submenu(N("Move _Selection"),
                    Item(N("To Layer _Above"), command: layers.MoveSelectionUp),
                    Item(N("To Layer _Below"), command: layers.MoveSelectionDown))),
            Submenu(N("_Path"),
                Item(N("_Convert Object to Path"), "Ctrl+Shift+O", paths.ConvertToPath),
                Item(N("_Stroke to Path")),
                Separator(),
                Item(N("Com_bine"), command: paths.Combine),
                Item(N("Break _Apart"), command: paths.BreakApart),
                Separator(),
                Item(N("_Union"), command: paths.Union),
                Item(N("_Difference"), command: paths.Difference),
                Item(N("_Intersection"), command: paths.Intersection),
                Item(N("E_xclusion"), command: paths.Exclusion),
                Separator(),
                Item(N("_Reverse Direction"), command: paths.ReverseDirection),
                Item(N("Si_mplify"))),
            Submenu(N("_Text"),
                Item(N("Convert Text to _Path")),
                Separator(),
                Item(N("_Font…")),
                Item(N("_Character Panel"))),
            Submenu(N("_View"),
                Item(N("Zoom _In"), "Ctrl++", zoomIn),
                Item(N("Zoom _Out"), "Ctrl+-", zoomOut),
                Item(N("_Fit Artboard in Window"), "Ctrl+0", fit),
                Item(N("_Actual Size (100%)"), "Ctrl+1", actualSize),
                Separator(),
                showRulersItem,
                showGridItem,
                snapToGridItem,
                snapToObjectsItem,
                Separator(),
                Submenu(N("_Language"), MenuItemViewModel.LanguageItems(localizer))),
            Submenu(N("_Window"),
                Item(N("_Properties"), command: ShowTab(InspectorTab.Properties)),
                Item(N("_Layers"), command: ShowTab(InspectorTab.Layers)),
                Item(N("_Assets"), command: ShowTab(InspectorTab.Assets)),
                Item(N("_History"), command: ShowTab(InspectorTab.History))),
            Submenu(N("_Help"),
                Item(N("_Keyboard Shortcuts")),
                Item(N("_About PressToShape"))),
        ];
    }

    public IReadOnlyList<MenuItemViewModel> Menus { get; }

    // Both separators, so a path from another platform still gives a readable name.
    private static string FileNameOf(string path) => path[(path.LastIndexOfAny(['/', '\\']) + 1)..];
}
