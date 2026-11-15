# PressToShape.App.ViewModels.Desktop

Project: PressToShape.App

## DesktopShellViewModel (class)

`internal partial sealed class DesktopShellViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:16-220`

Root of the desktop layout. Creates the shared PressToShape.App.Workspace.EditorState and the view model of every region, so toolbar, tool column, menu, canvas, inspector and status bar always agree.

**Members**

- `public IRelayCommand ActualSizeCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ActualSize`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ActualSize.
- `public IRelayCommand CopyCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Copy`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Copy.
- `public IRelayCommand CutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Cut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Cut.
- `public IRelayCommand DeleteCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Delete`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Delete.
- `public IRelayCommand DeselectCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Deselect`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Deselect.
- `public IRelayCommand DuplicateCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Duplicate`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Duplicate.
- `public IAsyncRelayCommand ExportSvgCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ExportSvgAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ExportSvgAsync.
- `public IRelayCommand FitArtboardCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.FitArtboard`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.FitArtboard.
- `public IAsyncRelayCommand NewCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.NewAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.NewAsync.
- `public IAsyncRelayCommand OpenCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.OpenAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.OpenAsync.
- `public IRelayCommand PasteCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Paste`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Paste.
- `public IRelayCommand PasteInPlaceCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.PasteInPlace`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.PasteInPlace.
- `public IRelayCommand QuitCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Quit`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Quit.
- `public IRelayCommand RedoCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Redo`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Redo.
- `public IAsyncRelayCommand SaveAsCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.SaveAsAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.SaveAsAsync.
- `public IAsyncRelayCommand SaveCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.SaveAsync`)_ - Gets an CommunityToolkit.Mvvm.Input.IAsyncRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.SaveAsync.
- `public IRelayCommand SelectAllCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.SelectAll`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.SelectAll.
- `public IRelayCommand UndoCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Undo`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Undo.
- `public IRelayCommand ZoomInCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomIn`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomIn.
- `public IRelayCommand ZoomOutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomOut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomOut.
- `public DesktopShellViewModel(IAppShutdown shutdown, ILocalizer localizer, FileServices services)` - _(no documentation)_
- `public EditorState State { get; }` - _(no documentation)_
- `public MenuBarViewModel MenuBar { get; }` - _(no documentation)_
- `public ToolbarViewModel Toolbar { get; }` - _(no documentation)_
- `public ToolboxViewModel Toolbox { get; }` - _(no documentation)_
- `public SelectionEditor Editor { get; }` - The selection edits (copy, paste, nudge, property changes). One instance, shared by the canvas, the inspector and the menus.
- `public CanvasAreaViewModel Canvas { get; }` - _(no documentation)_
- `public InspectorViewModel Inspector { get; }` - _(no documentation)_
- `public StatusBarViewModel StatusBar { get; }` - _(no documentation)_
- `public string WindowTitle { get; }` - The window title: an asterisk while there are unsaved changes, the file name (or "Untitled") and the product name.
- `public Task<bool> ConfirmCloseAsync()` - Asks about unsaved changes before the window closes. Returns whether closing may go on.
- `public void FitToWindow()` - Zooms so the artboard fits the canvas area. Does nothing until the canvas has a size.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.FileServices`
- Depends on (constructor) `PressToShape.App.Services.IAppShutdown`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## EditMenuCommands (record)

`internal sealed record EditMenuCommands : IEquatable<EditMenuCommands>`

Source: `src/PressToShape.App/ViewModels/Desktop/EditMenuCommands.cs:6-14`

What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.

**Members**

- `public EditMenuCommands(ICommand Cut, ICommand Copy, ICommand Paste, ICommand PasteInPlace, ICommand Duplicate, ICommand Delete, ICommand SelectAll, ICommand Deselect)` - What the clipboard, delete and selection items of the Edit menu do. Undo and Redo are passed on their own.
- `public ICommand Cut { get; init; }` - _(no documentation)_
- `public ICommand Copy { get; init; }` - _(no documentation)_
- `public ICommand Paste { get; init; }` - _(no documentation)_
- `public ICommand PasteInPlace { get; init; }` - _(no documentation)_
- `public ICommand Duplicate { get; init; }` - _(no documentation)_
- `public ICommand Delete { get; init; }` - _(no documentation)_
- `public ICommand SelectAll { get; init; }` - _(no documentation)_
- `public ICommand Deselect { get; init; }` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`

## FileMenuCommands (record)

`internal sealed record FileMenuCommands : IEquatable<FileMenuCommands>`

Source: `src/PressToShape.App/ViewModels/Desktop/FileMenuCommands.cs:7-13`

What the File menu items do. OpenRecent creates the command for one entry of the recent list.

**Members**

- `public FileMenuCommands(ICommand New, ICommand Open, ICommand Save, ICommand SaveAs, ICommand ExportSvg, Func<string, ICommand> OpenRecent)` - What the File menu items do. OpenRecent creates the command for one entry of the recent list.
- `public ICommand New { get; init; }` - _(no documentation)_
- `public ICommand Open { get; init; }` - _(no documentation)_
- `public ICommand Save { get; init; }` - _(no documentation)_
- `public ICommand SaveAs { get; init; }` - _(no documentation)_
- `public ICommand ExportSvg { get; init; }` - _(no documentation)_
- `public Func<string, ICommand> OpenRecent { get; init; }` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`

## MenuBarViewModel (class)

`internal sealed class MenuBarViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs:19-181`

The complete menu structure. The File actions, the Edit actions, Quit, the zoom items, the Window items and the languages have behavior. Everything else has no command and is therefore disabled until its feature exists.

**Members**

- `public MenuBarViewModel(EditorState state, IAppShutdown shutdown, ILocalizer localizer, Action<InspectorTab> showTab, Action fitToWindow, FileMenuCommands files, EditMenuCommands edit, RecentFiles recent, ICommand undo, ICommand redo)` - _(no documentation)_
- `public IReadOnlyList<MenuItemViewModel> Menus { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.IAppShutdown`
- Depends on (constructor) `PressToShape.App.ViewModels.Desktop.EditMenuCommands`
- Depends on (constructor) `PressToShape.App.ViewModels.Desktop.FileMenuCommands`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.RecentFiles`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## MenuItemViewModel (class)

`internal sealed class MenuItemViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/MenuItemViewModel.cs:15-107`

One entry of the menu bar. The header is translated and refreshes when the language changes. A leaf item without a command is disabled. A submenu is always enabled so it can be opened and its (possibly disabled) items inspected. A separator is rendered with the header "-".

**Members**

- `public string SeparatorHeader` - _(no documentation)_
- `public string Header { get; }` - The shown header: the translation of the message id, or a fixed text for language names.
- `public string? Gesture { get; }` - Shortcut text shown next to the item. Display only.
- `public string? ToolTip { get; }` - Tooltip text, or null. Not translated: it is used for data such as a folder path.
- `public ICommand? Command { get; }` - _(no documentation)_
- `public IReadOnlyList<MenuItemViewModel> Children { get; }` - _(no documentation)_
- `public bool IsSeparator { get; }` - _(no documentation)_
- `public bool IsEnabled { get; }` - A submenu is always enabled. A leaf item is enabled while its command can execute, and an item without a command is disabled.
- `public bool IsCheckable { get; }` - True for items that show a check state, such as the languages in the Language submenu.
- `public bool IsChecked { get; set; }` - The check state. Setting it has no effect: the item's command changes the state, and the state is then read again.
- `public MenuItemViewModel Item(ILocalizer localizer, string header, string? gesture = null, ICommand? command = null)` - A menu item whose header is a message id (an English text, usually with an Alt-key underscore).
- `public MenuItemViewModel Submenu(ILocalizer localizer, string header, params MenuItemViewModel[] children)` - _(no documentation)_
- `public MenuItemViewModel Submenu(ILocalizer localizer, string header, ObservableCollection<MenuItemViewModel> children)` - A submenu whose items are replaced while the app runs, such as Open Recent. The caller changes the collection, and the menu follows. Give it at least one item at all times so the submenu stays enabled.
- `public MenuItemViewModel Fixed(ILocalizer localizer, string text, ICommand command, string? toolTip = null)` - An item with a fixed, untranslated text, for data such as a file name, and an optional tooltip.
- `public MenuItemViewModel Separator(ILocalizer localizer)` - _(no documentation)_
- `public MenuItemViewModel Checkable(ILocalizer localizer, string fixedHeader, ICommand command, Func<bool> isChecked)` - A checkable item with a fixed, untranslated header, for language names such as "Deutsch".
- `public IEnumerable<MenuItemViewModel> Descendants()` - All items of this item and its descendants.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## StatusBarViewModel (class)

`internal partial sealed class StatusBarViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/StatusBarViewModel.cs:14-86`

Status bar: selection summary on the left, zoom controls on the right.

**Members**

- `public IRelayCommand ActivateHandToolCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ActivateHandTool`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ActivateHandTool.
- `public IRelayCommand FitToWindowCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.FitToWindow`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.StatusBarViewModel.FitToWindow.
- `public IRelayCommand<double> SetZoomCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.SetZoom(System.Double)`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand`1 instance wrapping PressToShape.App.ViewModels.Desktop.StatusBarViewModel.SetZoom.
- `public IRelayCommand ZoomInCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomIn`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomIn.
- `public IRelayCommand ZoomOutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomOut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ZoomOut.
- `public StatusBarViewModel(EditorState state, ILocalizer localizer, Action fitToWindow)` - _(no documentation)_
- `public string SelectionSummary { get; }` - The translated selection text: "Nothing selected", "1 selected" or "N selected".
- `public string PathLengthText { get; }` - Path length of the selection, for example "Path length: 2,480.6 px". Empty until selections exist.
- `public bool ShowPathLength { get; }` - _(no documentation)_
- `public double ZoomPercent { get; }` - _(no documentation)_
- `public string ZoomText { get; }` - The zoom as shown in the status bar, for example "100%" or "133.33%".
- `public IReadOnlyList<ZoomPresetViewModel> ZoomPresets { get; }` - The zoom values offered in the drop-down. Each entry carries its own command.
- `public string FormatZoom(double percent, CultureInfo culture)` - Formats a zoom value with the decimal separator of the given culture, for example "80.75%" or "80,75%".

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## ToolbarButtonViewModel (class)

`internal sealed class ToolbarButtonViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/ToolbarButtonViewModel.cs:8-36`

A toolbar button. Without a command it is disabled until its feature exists.

**Members**

- `public ToolbarButtonViewModel(ILocalizer localizer, string name, MaterialIconKind icon, ICommand? command = null)` - _(no documentation)_
- `public string Name { get; }` - The translated name, used as the tooltip and as the accessible name.
- `public MaterialIconKind Icon { get; }` - _(no documentation)_
- `public ICommand? Command { get; }` - _(no documentation)_
- `public bool IsEnabled { get; }` - Enabled while the command can execute. A button without a command is disabled.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## ToolbarViewModel (class)

`internal sealed class ToolbarViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Desktop/ToolbarViewModel.cs:17-117`

The toolbar groups from the mockup, left to right: file actions, history, tool toggles, horizontal alignment, transform buttons, vertical alignment, other actions. Position and size are edited in the Properties tab. Undo and Redo work, the other buttons are disabled for now. The Select and Node toggles are wired to the active tool.

**Members**

- `public ToolbarViewModel(EditorState state, ILocalizer localizer, ICommand undo, ICommand redo)` - _(no documentation)_
- `public IReadOnlyList<ToolbarButtonViewModel> FileActions { get; }` - _(no documentation)_
- `public IReadOnlyList<ToolbarButtonViewModel> HistoryActions { get; }` - _(no documentation)_
- `public IReadOnlyList<ToolbarButtonViewModel> HorizontalAlignActions { get; }` - _(no documentation)_
- `public IReadOnlyList<ToolbarButtonViewModel> TransformActions { get; }` - _(no documentation)_
- `public IReadOnlyList<ToolbarButtonViewModel> VerticalAlignActions { get; }` - _(no documentation)_
- `public IReadOnlyList<ToolbarButtonViewModel> OtherActions { get; }` - _(no documentation)_
- `public bool IsSelectToolActive { get; set; }` - Two-way for the Select toggle. Unchecking has no effect, because one tool is always active.
- `public bool IsNodeToolActive { get; set; }` - Two-way for the Node toggle. Unchecking has no effect, because one tool is always active.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## ZoomPresetViewModel (class)

`internal sealed class ZoomPresetViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/ZoomPresetViewModel.cs:8-23`

One entry of the zoom drop-down. It carries its own command so the menu needs no parent lookup.

**Members**

- `public ZoomPresetViewModel(double percent, Action apply)` - _(no documentation)_
- `public double Percent { get; }` - _(no documentation)_
- `public string Text { get; }` - _(no documentation)_
- `public ICommand SelectCommand { get; }` - _(no documentation)_
