using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using static PressToShape.App.ViewModels.Desktop.MenuItemViewModel;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// The complete menu structure. Only Quit, the zoom items and the Window items have behavior.
/// Everything else has no command and is therefore disabled until its feature exists.
/// </summary>
internal sealed class MenuBarViewModel : ViewModelBase
{
    public MenuBarViewModel(EditorState state, IAppShutdown shutdown, Action<InspectorTab> showTab, Action fitToWindow)
    {
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

        Menus =
        [
            Submenu("_File",
                Item("_New", "Ctrl+N"),
                Item("_Open…", "Ctrl+O"),
                Submenu("Open _Recent", Item("(No recent files)")),
                Separator(),
                Item("_Save", "Ctrl+S"),
                Item("Save _As…", "Ctrl+Shift+S"),
                Item("_Export as SVG…", "Ctrl+Shift+E"),
                Separator(),
                Item("_Document Settings…"),
                Separator(),
                Item("_Quit", "Ctrl+Q", quit)),
            Submenu("_Edit",
                Item("_Undo", "Ctrl+Z"),
                Item("_Redo", "Ctrl+Shift+Z"),
                Separator(),
                Item("Cu_t", "Ctrl+X"),
                Item("_Copy", "Ctrl+C"),
                Item("_Paste", "Ctrl+V"),
                Item("Paste in P_lace", "Ctrl+Shift+V"),
                Item("_Duplicate", "Ctrl+D"),
                Item("De_lete", "Delete"),
                Separator(),
                Item("Select _All", "Ctrl+A"),
                Item("D_eselect", "Ctrl+Shift+A"),
                Separator(),
                Item("_Preferences…")),
            Submenu("_Object",
                Submenu("_Transform",
                    Item("Rotate 90° _Clockwise"),
                    Item("Rotate 90° C_ounterclockwise"),
                    Item("Flip _Horizontal"),
                    Item("Flip _Vertical")),
                Submenu("_Arrange",
                    Item("Bring to _Front"),
                    Item("Bring _Forward"),
                    Item("Send _Backward"),
                    Item("Send to Bac_k")),
                Separator(),
                Item("_Group", "Ctrl+G"),
                Item("_Ungroup", "Ctrl+Shift+G"),
                Separator(),
                Item("_Lock"),
                Item("Unlock _All"),
                Item("_Hide"),
                Item("_Show All"),
                Separator(),
                Submenu("Clipping _Mask",
                    Item("_Make"),
                    Item("_Release"))),
            Submenu("_Path",
                Item("_Convert Object to Path", "Ctrl+Shift+O"),
                Item("_Stroke to Path"),
                Separator(),
                Item("Com_bine"),
                Item("_Break Apart"),
                Separator(),
                Item("_Union"),
                Item("_Difference"),
                Item("_Intersection"),
                Item("E_xclusion"),
                Separator(),
                Item("_Reverse Direction"),
                Item("Si_mplify")),
            Submenu("_Text",
                Item("Convert Text to _Path"),
                Separator(),
                Item("_Font…"),
                Item("_Character Panel")),
            Submenu("_View",
                Item("Zoom _In", "Ctrl++", zoomIn),
                Item("Zoom _Out", "Ctrl+-", zoomOut),
                Item("_Fit Artboard in Window", "Ctrl+0", fit),
                Item("_Actual Size (100%)", "Ctrl+1", actualSize),
                Separator(),
                Item("Show _Rulers", "Ctrl+R"),
                Item("Show _Grid"),
                Item("Snap to G_rid"),
                Item("Snap to _Objects")),
            Submenu("_Window",
                Item("_Properties", command: ShowTab(InspectorTab.Properties)),
                Item("_Layers", command: ShowTab(InspectorTab.Layers)),
                Item("_Assets", command: ShowTab(InspectorTab.Assets)),
                Item("_History", command: ShowTab(InspectorTab.History))),
            Submenu("_Help",
                Item("_Keyboard Shortcuts"),
                Item("_About PressToShape")),
        ];
    }

    public IReadOnlyList<MenuItemViewModel> Menus { get; }
}
