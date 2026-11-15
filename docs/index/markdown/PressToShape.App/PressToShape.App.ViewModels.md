# PressToShape.App.ViewModels

Project: PressToShape.App

## LocalizedViewModel (class)

`internal abstract class LocalizedViewModel : ViewModelBase`

Source: `src/PressToShape.App/ViewModels/LocalizedViewModel.cs:12-32`

Base class for view models that show translated text. When the language changes it notifies that every property may have changed, so bindings to translated properties refresh. Only use it for view models that live as long as the application: it never unsubscribes from the localizer.

**Members**

- `protected LocalizedViewModel(ILocalizer localizer)` - _(no documentation)_
- `protected ILocalizer Localizer { get; }` - _(no documentation)_
- `protected void OnLanguageRefreshed()` - Called after the language changed, before property change notifications. Override to refresh cached text.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Inherited by `PressToShape.App.ViewModels.Desktop.MenuItemViewModel`
- Inherited by `PressToShape.App.ViewModels.Desktop.StatusBarViewModel`
- Inherited by `PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.HistoryItemViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.NumberFieldViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.PropertiesViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.ToolItemViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.ZoomViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.PanelButtonViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`
- Inherits `PressToShape.App.ViewModels.ViewModelBase`

## MessageDialogButtonViewModel (record)

`internal sealed record MessageDialogButtonViewModel : IEquatable<MessageDialogButtonViewModel>`

Source: `src/PressToShape.App/ViewModels/MessageDialogButtonViewModel.cs:11-11`

One button of a message dialog. Choosing it closes the dialog with Result.

**Members**

- `public MessageDialogButtonViewModel(string Text, int Result, bool IsDefault, bool IsCancel, ICommand Command)` - One button of a message dialog. Choosing it closes the dialog with Result.
- `public ICommand Command { get; init; }` - Chooses the button.
- `public bool IsCancel { get; init; }` - Whether Escape chooses this button.
- `public bool IsDefault { get; init; }` - Whether Enter chooses this button.
- `public int Result { get; init; }` - The value the dialog returns when this button is chosen.
- `public string Text { get; init; }` - The translated button text.

## MessageDialogViewModel (class)

`internal sealed class MessageDialogViewModel`

Source: `src/PressToShape.App/ViewModels/MessageDialogViewModel.cs:14-82`

The content of a small modal dialog: a title, a message and a row of buttons. It holds no window code. Choosing a button raises PressToShape.App.ViewModels.MessageDialogViewModel.CloseRequested with that button's result. The texts are translated once when the dialog is created, because the dialog is short-lived.

**Members**

- `public EventHandler<int>? CloseRequested` - Raised when a button was chosen. The value is the button's result.
- `public string Title { get; }` - _(no documentation)_
- `public string Message { get; }` - _(no documentation)_
- `public IReadOnlyList<MessageDialogButtonViewModel> Buttons { get; }` - _(no documentation)_
- `public int CancelResult { get; private init; }` - The result that counts when the dialog is closed without choosing a button.
- `public MessageDialogViewModel ForUnsavedChanges(ILocalizer localizer, string documentName, bool escapeCancels = true)` - The question about unsaved changes. The results are the PressToShape.App.Services.UnsavedChoice values.
- `public MessageDialogViewModel ForError(ILocalizer localizer, string message, bool escapeCancels = true)` - An error message with a single OK button. escapeCancels is as for PressToShape.App.ViewModels.MessageDialogViewModel.ForUnsavedChanges.

## ViewModelBase (class)

`internal abstract class ViewModelBase : ObservableObject`

Source: `src/PressToShape.App/ViewModels/ViewModelBase.cs:5-7`

_(no documentation)_

**Relationships**

- Inherited by `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Inherited by `PressToShape.App.ViewModels.Desktop.ToolbarViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.ColorFieldViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.EditorShellViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.HistoryViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.LayerItemViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.LayersViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.PaletteViewModel`
- Inherited by `PressToShape.App.ViewModels.Editor.ToolboxViewModel`
- Inherited by `PressToShape.App.ViewModels.LocalizedViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.MessageDialogHost`
- Inherited by `PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`
- Inherited by `PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`
