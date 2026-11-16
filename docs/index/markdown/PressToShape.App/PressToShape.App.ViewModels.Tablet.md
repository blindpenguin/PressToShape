# PressToShape.App.ViewModels.Tablet

Project: PressToShape.App

## MessageDialogHost (class)

`internal partial sealed class MessageDialogHost : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Tablet/MessageDialogHost.cs:12-50`

Shows message dialogs inside the layout, for a platform without windows. The layout shows PressToShape.App.ViewModels.Tablet.MessageDialogHost.Current over everything else while it is not null. Only one dialog is shown at a time: a second request waits until the first is closed.

**Members**

- `public MessageDialogViewModel? Current { get; private set; }` - _(no documentation)_
- `public bool IsOpen { get; }` - _(no documentation)_
- `public Task<int?> ShowAsync(MessageDialogViewModel dialog)` - Shows a dialog and waits for the result of the button that was chosen. Gives null if the dialog was dismissed without a choice.
- `public void Dismiss()` - Closes the dialog that is shown without a choice, for example when the Back button is pressed. Does nothing if there is none.

**Relationships**

- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.Services.OverlayMessageDialogs`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletShellViewModel`

## OverflowMenuViewModel (class)

`internal sealed class OverflowMenuViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs:17-65`

The "..." menu of the tablet toolbar: the file actions, the edit actions, group, arrange, lock and hide, and the language. It uses the same texts as the desktop menus (so the translations are shared) and the same commands, shown without Alt keys and shortcut texts through PressToShape.App.ViewModels.Desktop.MenuItemViewModel.PlainHeader. There is no Quit (an Android app is left with the Back button) and no list of recent files (their references do not survive a restart). An item without a command, or whose command cannot run, is disabled.

**Members**

- `public OverflowMenuViewModel(ILocalizer localizer, FileMenuCommands files, EditMenuCommands edit, ObjectMenuCommands objects)` - _(no documentation)_
- `public IReadOnlyList<MenuItemViewModel> Items { get; }` - The items in the order they are shown, with separators between the groups.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.EditMenuCommands`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.FileMenuCommands`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.ObjectMenuCommands`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`

## PanelButtonViewModel (class)

`internal sealed class PanelButtonViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Tablet/PanelButtonViewModel.cs:12-50`

A toolbar icon that opens the floating panel with one tab, or closes it when that tab is the open one.

**Members**

- `public PanelButtonViewModel(ILocalizer localizer, InspectorTab tab, string name, MaterialIconKind icon, TabletPanelViewModel panel)` - _(no documentation)_
- `public InspectorTab Tab { get; }` - _(no documentation)_
- `public string Name { get; }` - The translated name, used as the tooltip and as the accessible name.
- `public MaterialIconKind Icon { get; }` - _(no documentation)_
- `public bool IsActive { get; }` - Whether the panel is open with this button's tab.
- `public ICommand ToggleCommand { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`

## TabletColorSwatchViewModel (class)

`internal partial sealed class TabletColorSwatchViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Tablet/TabletColorSwatchViewModel.cs:18-82`

The small color swatch at the bottom of the tablet tool column: the fill in the middle and the stroke as a ring around it. It shows what the Properties panel shows (the color of the selection, or of the next shape when nothing is selected) by reading the fill and stroke fields of the PressToShape.App.ViewModels.Editor.PropertiesViewModel, so there is one source of truth. A tap opens the Properties panel at its Appearance section, where the colors are edited.

**Members**

- `public IRelayCommand OpenCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.Open`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.Open.
- `public TabletColorSwatchViewModel(PropertiesViewModel properties, TabletPanelViewModel panel, ILocalizer localizer)` - _(no documentation)_
- `public Color FillColor { get; }` - The fill color. Only meaningful while PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.HasFill is true.
- `public bool HasFill { get; }` - Whether the fill has a color. False for no fill and for a selection with different fills.
- `public Color StrokeColor { get; }` - The stroke color. Only meaningful while PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.HasStroke is true.
- `public bool HasStroke { get; }` - Whether the stroke has a color. False for no stroke and for a selection with different strokes.
- `public string Name { get; }` - The translated name, used as the tooltip and as the accessible name.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.PropertiesViewModel`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`

## TabletPanelViewModel (class)

`internal partial sealed class TabletPanelViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Tablet/TabletPanelViewModel.cs:15-95`

The panel of the tablet layout, which floats over the canvas and shows one of the inspector's tabs (Properties, Layers or History) while it is open. A toolbar icon toggles it. The shown tab is kept in PressToShape.App.ViewModels.Editor.InspectorViewModel.SelectedTab, as on the desktop. There is no Assets panel on the tablet until Assets has content.

**Members**

- `public TabletPanelViewModel(InspectorViewModel inspector, ILocalizer localizer)` - _(no documentation)_
- `public InspectorViewModel Inspector { get; }` - _(no documentation)_
- `public InspectorTab? OpenTab { get; private set; }` - _(no documentation)_
- `public bool IsOpen { get; }` - _(no documentation)_
- `public object? Content { get; }` - The view model of the open tab. The view shows it through the view locator.
- `public string Title { get; }` - The translated name of the open tab.
- `public void Toggle(InspectorTab tab)` - The icon of a tab was tapped. The panel opens with that tab, shows it if another one was open, or closes if it was the open one. The Assets tab is not offered and is ignored.
- `public void Open(InspectorTab tab)` - Opens the panel with a tab, or shows that tab if another one is open. Unlike PressToShape.App.ViewModels.Tablet.TabletPanelViewModel.Toggle it never closes. Assets is ignored.
- `public void Close()` - Closes the panel. A number field that is still being typed in is applied first.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.PanelButtonViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`

## TabletShellViewModel (class)

`internal sealed class TabletShellViewModel : EditorShellViewModel`

Source: `src/PressToShape.App/ViewModels/Tablet/TabletShellViewModel.cs:14-98`

Root of the tablet layout. Adds the compact toolbar with its overflow menu, the tool column, the floating panel and the in-layout message dialog to what every shell has (see PressToShape.App.ViewModels.Editor.EditorShellViewModel). The Back button goes through PressToShape.App.ViewModels.Tablet.TabletShellViewModel.RequestBackAsync.

**Members**

- `public TabletShellViewModel(IAppShutdown shutdown, ILocalizer localizer, FileServices services, MessageDialogHost dialogs)` - _(no documentation)_
- `public TabletToolbarViewModel Toolbar { get; }` - _(no documentation)_
- `public OverflowMenuViewModel OverflowMenu { get; }` - The "..." menu. It is also reachable as PressToShape.App.ViewModels.Tablet.TabletShellViewModel.Toolbar.OverflowMenu.
- `public TabletPanelViewModel Panel { get; }` - _(no documentation)_
- `public TabletToolColumnViewModel Column { get; }` - The tool column with its tool groups and the swatches.
- `public MessageDialogHost Dialogs { get; }` - The message dialog that is shown over the layout, or nothing.
- `public Task<bool> RequestBackAsync()` - The Back button was pressed (after the view closed a toolbar menu, if one was open). It closes the topmost thing that is open and stops there: a message dialog (as Cancel), then a tool group flyout, then the panel. Only when none of them is open does it do what PressToShape.App.ViewModels.Tablet.TabletShellViewModel.RequestLeaveAsync does.
- `public Task<bool> RequestLeaveAsync()` - Leaving was asked for. While a message dialog is open, Back closes it like Cancel and the app stays. Otherwise: without unsaved changes the app is left at once, with them the user is asked to save, discard or cancel, and the app is left only after a successful save or after discarding.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.FileServices`
- Depends on (constructor) `PressToShape.App.Services.IAppShutdown`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.MessageDialogHost`
- Inherits `PressToShape.App.ViewModels.Editor.EditorShellViewModel`

## TabletToolColumnViewModel (class)

`internal sealed class TabletToolColumnViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Tablet/TabletToolColumnViewModel.cs:15-65`

The tool column of the tablet layout: one icon per tool group (see PressToShape.App.Workspace.ToolGroups), and the small fill and stroke swatch below. Only one group flyout is open at a time.

**Members**

- `public TabletToolColumnViewModel(ToolboxViewModel toolbox, EditorState state, ILocalizer localizer, TabletColorSwatchViewModel colorSwatch)` - _(no documentation)_
- `public IReadOnlyList<ToolGroupViewModel> Groups { get; }` - The icons from top to bottom.
- `public TabletColorSwatchViewModel ColorSwatch { get; }` - The small fill and stroke swatch below the icons. A tap opens the Properties panel.
- `public bool HasOpenFlyout { get; }` - Whether a group flyout is open.
- `public void CloseFlyouts()` - Closes every open group flyout.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.ToolboxViewModel`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## TabletToolbarViewModel (class)

`internal sealed class TabletToolbarViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/Tablet/TabletToolbarViewModel.cs:19-82`

The compact toolbar of the tablet layout: the name of the document (with a star while it has unsaved changes), undo and redo, Duplicate and Delete, the icons that open the panel, the zoom, and the overflow menu. Everything else of the desktop toolbar and menu bar is in the overflow menu.

**Members**

- `public TabletToolbarViewModel(ILocalizer localizer, EditorShellViewModel shell, ICommand undo, ICommand redo, ZoomViewModel zoom, OverflowMenuViewModel overflowMenu, TabletPanelViewModel panel)` - _(no documentation)_
- `public string DocumentTitle { get; }` - The document name, with "* " in front while the document has unsaved changes.
- `public ToolbarButtonViewModel UndoButton { get; }` - _(no documentation)_
- `public ToolbarButtonViewModel RedoButton { get; }` - _(no documentation)_
- `public ToolbarButtonViewModel DuplicateButton { get; }` - Duplicates the selection. Enabled only while something is selected.
- `public ToolbarButtonViewModel DeleteButton { get; }` - Deletes the selection (one undo step). Enabled only while something is selected.
- `public IReadOnlyList<PanelButtonViewModel> PanelButtons { get; }` - The icons that open the panel: Properties, Layers, History.
- `public ZoomViewModel Zoom { get; }` - _(no documentation)_
- `public OverflowMenuViewModel OverflowMenu { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.EditorShellViewModel`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.ZoomViewModel`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## ToolGroupViewModel (class)

`internal partial sealed class ToolGroupViewModel : LocalizedViewModel`

Source: `src/PressToShape.App/ViewModels/Tablet/ToolGroupViewModel.cs:19-109`

One icon of the tablet tool column. It shows the current tool of its group; a tap uses that tool, a long press opens a flyout with all the tools of the group. The active tool of PressToShape.App.Workspace.EditorState stays the only truth: this view model only remembers which member of the group was used last, and follows the active tool when it changes in any other way (shortcut, command).

**Members**

- `public IRelayCommand OpenFlyoutCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.OpenFlyout`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.OpenFlyout.
- `public IRelayCommand SelectCurrentCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.SelectCurrent`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand instance wrapping PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.SelectCurrent.
- `public IRelayCommand<ToolItemViewModel> SelectMemberCommand { get; }` _(generated from `M:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.SelectMember(PressToShape.App.ViewModels.Editor.ToolItemViewModel)`)_ - Gets an CommunityToolkit.Mvvm.Input.IRelayCommand`1 instance wrapping PressToShape.App.ViewModels.Tablet.ToolGroupViewModel.SelectMember.
- `public ToolGroupViewModel(ToolGroupDefinition definition, ToolboxViewModel toolbox, EditorState state, ILocalizer localizer)` - _(no documentation)_
- `public IReadOnlyList<ToolItemViewModel> Members { get; }` - The tools of the group, in order. The flyout lists them.
- `public bool IsGroup { get; }` - Whether the group has more than one tool, so it shows a mark and has a flyout.
- `public ToolItemViewModel Current { get; private set; }` - The tool a tap uses: the last one chosen in this group.
- `public bool IsActive { get; set; }` - _(no documentation)_
- `public bool IsFlyoutOpen { get; set; }` - _(no documentation)_
- `public string Name { get; }` - The translated name of the current tool. It is also the accessible name of the icon.
- `public string ToolTip { get; }` - _(no documentation)_
- `public MaterialIconKind Icon { get; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Editor.ToolboxViewModel`
- Depends on (constructor) `PressToShape.App.Workspace.EditorState`
- Depends on (constructor) `PressToShape.App.Workspace.ToolGroupDefinition`
- Inherits `PressToShape.App.ViewModels.LocalizedViewModel`
