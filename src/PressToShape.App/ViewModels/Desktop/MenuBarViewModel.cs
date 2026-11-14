using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// The complete menu structure. Only Quit, the zoom items and the Window items have behavior.
/// Everything else has no command and is therefore disabled until its feature exists.
/// </summary>
internal sealed class MenuBarViewModel : ViewModelBase
{
    public MenuBarViewModel(EditorState state, IAppShutdown shutdown, ILocalizer localizer, Action<InspectorTab> showTab, Action fitToWindow)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shutdown);
        ArgumentNullException.ThrowIfNull(showTab);
        ArgumentNullException.ThrowIfNull(fitToWindow);

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
        MenuItemViewModel[] LanguageItems() => localizer.Languages
            .Select(language => MenuItemViewModel.Checkable(
                localizer,
                language.DisplayName,
                new RelayCommand(() => localizer.SetLanguage(language.Code)),
                () => localizer.LanguageCode == language.Code))
            .ToArray();

        Menus =
        [
            Submenu(N("_File"),
                Item(N("_New"), "Ctrl+N"),
                Item(N("_Open…"), "Ctrl+O"),
                Submenu(N("Open _Recent"), Item(N("(No recent files)"))),
                Separator(),
                Item(N("_Save"), "Ctrl+S"),
                Item(N("Save _As…"), "Ctrl+Shift+S"),
                Item(N("_Export as SVG…"), "Ctrl+Shift+E"),
                Separator(),
                Item(N("_Document Settings…")),
                Separator(),
                Item(N("_Quit"), "Ctrl+Q", quit)),
            Submenu(N("_Edit"),
                Item(N("_Undo"), "Ctrl+Z"),
                Item(N("_Redo"), "Ctrl+Shift+Z"),
                Separator(),
                Item(N("Cu_t"), "Ctrl+X"),
                Item(N("_Copy"), "Ctrl+C"),
                Item(N("_Paste"), "Ctrl+V"),
                Item(N("Paste in Plac_e"), "Ctrl+Shift+V"),
                Item(N("_Duplicate"), "Ctrl+D"),
                Item(N("De_lete"), "Delete"),
                Separator(),
                Item(N("Select _All"), "Ctrl+A"),
                Item(N("De_select"), "Ctrl+Shift+A"),
                Separator(),
                Item(N("Pre_ferences…"))),
            Submenu(N("_Object"),
                Submenu(N("_Transform"),
                    Item(N("Rotate 90° _Clockwise")),
                    Item(N("Rotate 90° C_ounterclockwise")),
                    Item(N("Flip _Horizontal")),
                    Item(N("Flip _Vertical"))),
                Submenu(N("_Arrange"),
                    Item(N("Bring to _Front")),
                    Item(N("Bring F_orward")),
                    Item(N("Send _Backward")),
                    Item(N("Send to Bac_k"))),
                Separator(),
                Item(N("_Group"), "Ctrl+G"),
                Item(N("_Ungroup"), "Ctrl+Shift+G"),
                Separator(),
                Item(N("_Lock")),
                Item(N("Unloc_k All")),
                Item(N("_Hide")),
                Item(N("_Show All")),
                Separator(),
                Submenu(N("Clipping _Mask"),
                    Item(N("_Make")),
                    Item(N("_Release")))),
            Submenu(N("_Path"),
                Item(N("_Convert Object to Path"), "Ctrl+Shift+O"),
                Item(N("_Stroke to Path")),
                Separator(),
                Item(N("Com_bine")),
                Item(N("Break _Apart")),
                Separator(),
                Item(N("_Union")),
                Item(N("_Difference")),
                Item(N("_Intersection")),
                Item(N("E_xclusion")),
                Separator(),
                Item(N("_Reverse Direction")),
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
                Item(N("Show _Rulers"), "Ctrl+R"),
                Item(N("Show _Grid")),
                Item(N("_Snap to Grid")),
                Item(N("Snap to Ob_jects")),
                Separator(),
                Submenu(N("_Language"), LanguageItems())),
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
}
