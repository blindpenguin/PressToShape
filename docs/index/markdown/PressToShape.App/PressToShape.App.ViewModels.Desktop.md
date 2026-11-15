# PressToShape.App.ViewModels.Desktop

Project: PressToShape.App

## DesktopShellViewModel (class)

`internal partial sealed class DesktopShellViewModel : EditorShellViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/DesktopShellViewModel.cs:12-47`

Root of the desktop layout. Adds the menu bar, the toolbar and the status bar to what every shell has (see PressToShape.App.ViewModels.Editor.EditorShellViewModel).

**Members**

- `public IRelayCommand ActualSizeCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ActualSize`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ActualSize.
- `public IRelayCommand FitArtboardCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.FitArtboard`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.FitArtboard.
- `public IRelayCommand QuitCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Quit`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.Quit.
- `public IRelayCommand ZoomInCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomIn`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomIn.
- `public IRelayCommand ZoomOutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomOut`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.DesktopShellViewModel.ZoomOut.
- `public DesktopShellViewModel(IAppShutdown shutdown, ILocalizer localizer, FileServices services)` - _(no documentation)_
- `public MenuBarViewModel MenuBar { get; }` - _(no documentation)_
- `public ToolbarViewModel Toolbar { get; }` - _(no documentation)_
- `public StatusBarViewModel StatusBar { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.FileServices`
- Depends on (constructor) `PressToShape.App.Services.IAppShutdown`
- Inherits `PressToShape.App.ViewModels.Editor.EditorShellViewModel`

## MenuBarViewModel (class)

`internal sealed class MenuBarViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Desktop/MenuBarViewModel.cs:20-175`

The complete menu structure. The File actions, the Edit actions, Quit, the zoom items, the Window items and the languages have behavior. Everything else has no command and is therefore disabled until its feature exists.

**Members**

- `public MenuBarViewModel(EditorState state, IAppShutdown shutdown, ILocalizer localizer, Action<InspectorTab> showTab, Action fitToWindow, FileMenuCommands files, EditMenuCommands edit, RecentFiles recent, ICommand undo, ICommand redo)` - _(no documentation)_
- `public IReadOnlyList<MenuItemViewModel> Menus { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.IAppShutdown`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.EditMenuCommands`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.FileMenuCommands`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.RecentFiles`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## MenuItemViewModel (class)

`internal sealed class MenuItemViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/MenuItemViewModel.cs:15-150`

One entry of the menu bar. The header is translated and refreshes when the language changes. A leaf item without a command is disabled. A submenu is always enabled so it can be opened and its (possibly disabled) items inspected. A separator is rendered with the header "-".

**Members**

- `public string SeparatorHeader` - _(no documentation)_
- `public string Header { get; }` - The shown header: the translation of the message id, or a fixed text for language names.
- `public string PlainHeader { get; }` - The header without the Alt-key underscore, for a menu that has no Alt keys, such as the overflow menu of the tablet layout. A single underscore marks the key and is dropped, a double underscore is one underscore. A fixed text (a file or language name) is data and stays as it is.
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
- `internal string RemoveMnemonic(string text)` - Removes the mnemonic marker of a menu text: "_Save" gives "Save", "Save __ As" gives "Save _ As".
- `public MenuItemViewModel[] LanguageItems(ILocalizer localizer)` - One checkable item for every language of the app, named in its own language. Choosing one switches the language.
- `public IEnumerable<MenuItemViewModel> Descendants()` - All items of this item and its descendants.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## StatusBarViewModel (class)

`internal partial sealed class StatusBarViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Desktop/StatusBarViewModel.cs:13-74`

Status bar: selection summary on the left, zoom controls on the right.

**Members**

- `public IRelayCommand ActivateHandToolCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ActivateHandTool`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Desktop.StatusBarViewModel.ActivateHandTool.
- `public StatusBarViewModel(EditorState state, ILocalizer localizer, Action fitToWindow)` - _(no documentation)_
- `public string SelectionSummary { get; }` - The translated selection text: "Nothing selected", "1 selected" or "N selected".
- `public string PathLengthText { get; }` - Path length of the selection, for example "Path length: 2,480.6 px". Empty until selections exist.
- `public bool ShowPathLength { get; }` - _(no documentation)_
- `public double ZoomPercent { get; }` - _(no documentation)_
- `public string ZoomText { get; }` - The zoom as shown in the status bar, for example "100%" or "133.33%".
- `public IReadOnlyList<ZoomPresetViewModel> ZoomPresets { get; }` - The zoom values offered in the drop-down. Each entry carries its own command.
- `public IRelayCommand ZoomInCommand { get; }` - _(no documentation)_
- `public IRelayCommand ZoomOutCommand { get; }` - _(no documentation)_
- `public IRelayCommand FitToWindowCommand { get; }` - _(no documentation)_
- `public IRelayCommand<double> SetZoomCommand { get; }` - _(no documentation)_
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
