# PressToShape.App.Services

Project: PressToShape.App

## AvaloniaFileDialogs (class)

`internal sealed class AvaloniaFileDialogs : IFileDialogs`

Source: `src/PressToShape.App/Services/AvaloniaFileDialogs.cs:16-88`

The platform's file pickers, opened over the main window. A picker that is not available gives null, which the callers treat like Cancel. A picked file with a local path gives that path. A file without one (Android) gives a reference from the PressToShape.App.Services.StorageFileRegistry, or null if there is no registry.

**Members**

- `public AvaloniaFileDialogs(Func<TopLevel?> owner, ILocalizer localizer, StorageFileRegistry? registry = null)` - _(no documentation)_
- `public Task<string?> PickProjectToOpenAsync()` - _(no documentation)_
- `public Task<string?> PickProjectToSaveAsync(string suggestedFileName)` - _(no documentation)_
- `public Task<string?> PickSvgToExportAsync(string suggestedFileName)` - _(no documentation)_
- `internal FilePickerSaveOptions BuildSaveOptions(string title, string suggestedFileName, string extension, FilePickerFileType fileType)` - The options of the save picker. The default extension is left out when the suggested name has it already, because the Android picker adds it a second time ("plan.p2s.p2s").
- `internal string? ChooseReference(string? localPath, Func<string>? register)` - The local path if there is one, otherwise the registry reference if there is a registry, otherwise null.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Services.StorageFileRegistry`
- Implements `PressToShape.App.Services.IFileDialogs`

## AvaloniaFontCatalog (class)

`internal sealed class AvaloniaFontCatalog : IFontCatalog`

Source: `src/PressToShape.App/Services/AvaloniaFontCatalog.cs:13-28`

The font families of the system, with the bundled default font in front. Only the names are read: making a typeface of every font to see whether it works loads all font files at startup, and a broken one crashes the native font code.

**Members**

- `public IReadOnlyList<string> Families { get; }` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IFontCatalog`

## AvaloniaMessageDialogs (class)

`internal sealed class AvaloniaMessageDialogs : IMessageDialogs`

Source: `src/PressToShape.App/Services/AvaloniaMessageDialogs.cs:11-46`

Shows the questions and messages as modal dialogs over the main window.

**Members**

- `public AvaloniaMessageDialogs(Func<Window?> owner, ILocalizer localizer)` - _(no documentation)_
- `public Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)` - _(no documentation)_
- `public Task ShowErrorAsync(string message)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Implements `PressToShape.App.Services.IMessageDialogs`

## AvaloniaSettingsDialogs (class)

`internal sealed class AvaloniaSettingsDialogs : ISettingsDialogs`

Source: `src/PressToShape.App/Services/AvaloniaSettingsDialogs.cs:10-32`

Shows the settings as a modal window over the main window.

**Members**

- `public AvaloniaSettingsDialogs(Func<Window?> owner)` - _(no documentation)_
- `public Task ShowAsync(SettingsViewModel viewModel)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.ISettingsDialogs`

## AvaloniaTextLayouts (class)

`internal static class AvaloniaTextLayouts`

Source: `src/PressToShape.App/Services/AvaloniaTextLayouts.cs:12-60`

Builds the Avalonia layout of a text. The measurer reads the lines from it, the outliner reads the glyphs, so both see the same layout as the canvas.

**Members**

- `public TextLayout Build(TextShape text)` - _(no documentation)_

## AvaloniaTextMeasurer (class)

`internal sealed class AvaloniaTextMeasurer : ITextMeasurer`

Source: `src/PressToShape.App/Services/AvaloniaTextMeasurer.cs:12-50`

Lays out a text with Avalonia's text engine (see PressToShape.App.Services.AvaloniaTextLayouts). Point text is not wrapped, area text wraps at the width of its box. A font family that is not installed gets the fallback font of the platform. If the engine cannot lay the text out, the result is empty and the document is not touched.

**Members**

- `public TextLayoutResult Layout(TextShape text)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.Core.Shapes.ITextMeasurer`

## AvaloniaTextOutliner (class)

`internal sealed class AvaloniaTextOutliner : ITextOutliner`

Source: `src/PressToShape.App/Services/AvaloniaTextOutliner.cs:20-135`

Gives the glyph contours of a text. Avalonia has no public way to read the contours of a geometry, so this takes the glyphs that Avalonia shaped for the canvas (their ids and positions, see PressToShape.App.Services.AvaloniaTextLayouts) and reads the outline of each glyph from the font file with SkiaSharp. The outlines are exactly where the canvas draws the glyphs.

A font collection file (TTC) is read as its first font, so glyphs of the other fonts of the collection come out wrong. Contours that overlap are not merged, the compound path is filled even-odd.

**Members**

- `public IReadOnlyList<DocumentSubPath>? Outline(TextShape text)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.Core.Shapes.ITextOutliner`

## DelegateAppShutdown (class)

`internal sealed class DelegateAppShutdown : IAppShutdown`

Source: `src/PressToShape.App/Services/DelegateAppShutdown.cs:6-16`

Ends the app by calling a delegate. The Android head gives it the call that finishes the activity.

**Members**

- `public DelegateAppShutdown(Action shutdown)` - _(no documentation)_
- `public void Shutdown()` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IAppShutdown`

## DesktopAppShutdown (class)

`internal sealed class DesktopAppShutdown : IAppShutdown`

Source: `src/PressToShape.App/Services/DesktopAppShutdown.cs:10-19`

Closes the application when it runs with a classic desktop lifetime. Does nothing otherwise. It closes the main window instead of shutting down directly, so the window's closing handler can ask about unsaved changes.

**Members**

- `public void Shutdown()` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IAppShutdown`

## FileServices (record)

`internal sealed record FileServices : IEquatable<FileServices>`

Source: `src/PressToShape.App/Services/FileServices.cs:12-21`

What the file actions need from the outside: settings, files, file pickers and message dialogs.

**Members**

- `public FileServices(ISettingsStore Settings, IFileStore Files, IFileDialogs Dialogs, IMessageDialogs Messages, bool RemembersRecentFiles = true, ISettingsDialogs? SettingsDialogs = null, ITextMeasurer? TextMeasurer = null, ITextOutliner? TextOutliner = null, IFontCatalog? FontCatalog = null)` - What the file actions need from the outside: settings, files, file pickers and message dialogs.
- `public ISettingsStore Settings { get; init; }` - _(no documentation)_
- `public IFileStore Files { get; init; }` - _(no documentation)_
- `public IFileDialogs Dialogs { get; init; }` - _(no documentation)_
- `public IMessageDialogs Messages { get; init; }` - _(no documentation)_
- `public bool RemembersRecentFiles { get; init; }` - Whether the recently used files are kept in the settings. Off where a file reference does not survive a restart (Android).
- `public ISettingsDialogs? SettingsDialogs { get; init; }` - Shows the settings window. null where there is none: "Preferences…" is then disabled.
- `public ITextMeasurer? TextMeasurer { get; init; }` - Measures texts. null uses Avalonia's text engine, tests pass a fake.
- `public ITextOutliner? TextOutliner { get; init; }` - Gives the glyph contours of texts. null uses Avalonia's fonts, tests pass a fake.
- `public IFontCatalog? FontCatalog { get; init; }` - The font families to choose from. null uses the fonts of the system, tests pass a fake.

**Relationships**

- Depends on (constructor) `PressToShape.App.Services.IFileDialogs`
- Depends on (constructor) `PressToShape.App.Services.IFileStore`
- Depends on (constructor) `PressToShape.App.Services.IFontCatalog`
- Depends on (constructor) `PressToShape.App.Services.IMessageDialogs`
- Depends on (constructor) `PressToShape.App.Services.ISettingsDialogs`
- Depends on (constructor) `PressToShape.App.Settings.ISettingsStore`
- Depends on (constructor) `PressToShape.Core.Shapes.ITextMeasurer`
- Depends on (constructor) `PressToShape.Core.Shapes.ITextOutliner`
- Injected into `PressToShape.App.Tests.EditorShellViewModelRingTests.BareShell`
- Injected into `PressToShape.App.Tests.EditorShellViewModelTests.BareShell`
- Injected into `PressToShape.App.Tests.OpenSettingsCommandTests.BareShell`
- Injected into `PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.EditorShellViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletShellViewModel`

## GlyphOutlines (class)

`internal static class GlyphOutlines`

Source: `src/PressToShape.App/Services/GlyphOutlines.cs:9-126`

Reads the contours of a Skia path (the outline of a glyph) into sub-paths of the document.

**Members**

- `public IReadOnlyList<DocumentSubPath> FromPath(SKPath path, double offsetX, double offsetY)` - The contours of the path, moved by the offset. Every contour is closed: it ends at its start, or a closing line joins it. Quadratic curves become cubic ones, conic curves are cut into quadratic ones. A contour with fewer than two points is left out.

## IAppShutdown (interface)

`internal interface IAppShutdown`

Source: `src/PressToShape.App/Services/IAppShutdown.cs:4-7`

Lets view models close the application without referencing Avalonia types.

**Members**

- `public void Shutdown()` - _(no documentation)_

**Relationships**

- Implemented by `PressToShape.App.Services.DelegateAppShutdown`
- Implemented by `PressToShape.App.Services.DesktopAppShutdown`
- Implemented by `PressToShape.App.Tests.CanvasAreaViewSnapTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.DesktopShellKeyBindingTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.DesktopShellViewModelTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.EditorShellViewModelRingTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.EditorStateSnapTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.LayersViewTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.MenuBarSmartTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.MenuBarSnapTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.MenuBarViewModelTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.ObjectCommandsTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.SelectionAndPropertiesIntegrationTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.SettingsDialogHostTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.TabletShellViewModelTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.TabletStatusBarViewTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.TabletViewsTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.TranslationConsistencyTests.NoShutdown`
- Injected into `PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletShellViewModel`

## IFileDialogs (interface)

`internal interface IFileDialogs`

Source: `src/PressToShape.App/Services/IFileDialogs.cs:6-16`

The file pickers. Each returns a local file path, or null if the user cancelled.

**Members**

- `public Task<string?> PickProjectToOpenAsync()` - Lets the user pick a project file (.p2s) to open.
- `public Task<string?> PickProjectToSaveAsync(string suggestedFileName)` - Lets the user choose where to save a project file. The suggested name includes the .p2s ending.
- `public Task<string?> PickSvgToExportAsync(string suggestedFileName)` - Lets the user choose where to export the SVG. The suggested name includes the .svg ending.

**Relationships**

- Implemented by `PressToShape.App.Services.AvaloniaFileDialogs`
- Implemented by `PressToShape.App.Tests.FakeFileDialogs`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## IFileStore (interface)

`internal interface IFileStore`

Source: `src/PressToShape.App/Services/IFileStore.cs:7-20`

Reads and writes whole files. Lets the file actions be tested without a real file system, and lets a platform without file paths supply its own storage.

**Members**

- `public Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default(CancellationToken))` - Reads a whole file.
- `public Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))` - Writes a whole file so that an existing file is only replaced once the new content is complete. If writing fails, the existing file stays as it was and no partial file stays behind.

**Relationships**

- Implemented by `PressToShape.App.Services.LocalFileStore`
- Implemented by `PressToShape.App.Services.StorageFileStore`
- Implemented by `PressToShape.App.Tests.FakeFileStore`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.Tests.DocumentFileServiceTests.Setup`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## IFontCatalog (interface)

`internal interface IFontCatalog`

Source: `src/PressToShape.App/Services/IFontCatalog.cs:6-10`

The font families the user can choose for a text. A small interface of ours, so the Properties view model needs no fonts in tests.

**Members**

- `public IReadOnlyList<string> Families { get; }` - The names of the font families, the default font first, then the installed fonts in alphabetical order. No name twice.

**Relationships**

- Implemented by `PressToShape.App.Services.AvaloniaFontCatalog`
- Implemented by `PressToShape.App.Tests.PropertiesTextTests.FakeFontCatalog`
- Implemented by `PressToShape.App.Tests.PropertiesViewTextTests.Fonts`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PropertiesViewModel`

## IMessageDialogs (interface)

`internal interface IMessageDialogs`

Source: `src/PressToShape.App/Services/IMessageDialogs.cs:6-13`

The questions and messages shown to the user. The texts are translated by the caller.

**Members**

- `public Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)` - Asks whether unsaved changes of the named document should be saved. Closing the question counts as Cancel.
- `public Task ShowErrorAsync(string message)` - Shows an error message and returns when the user has dismissed it.

**Relationships**

- Implemented by `PressToShape.App.Services.AvaloniaMessageDialogs`
- Implemented by `PressToShape.App.Services.OverlayMessageDialogs`
- Implemented by `PressToShape.App.Tests.FakeMessageDialogs`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.Workspace.DocumentFileService`
- Injected into `PressToShape.App.Workspace.PathEditor`

## IPickedFile (interface)

`internal interface IPickedFile`

Source: `src/PressToShape.App/Services/IPickedFile.cs:10-20`

A file that the user picked, seen through the two things the app needs from it: its name and its streams. It exists because Avalonia's IStorageFile cannot be implemented by other code, so the file store could not be tested with a fake.

**Members**

- `public string Name { get; }` - The file name without a folder.
- `public Task<Stream> OpenReadAsync()` - Opens the file for reading.
- `public Task<Stream> OpenWriteAsync()` - Opens the file for writing. This empties the file, as it does on Android.

**Relationships**

- Implemented by `PressToShape.App.Services.PickedStorageFile`
- Implemented by `PressToShape.App.Tests.FakePickedFile`

## ISettingsDialogs (interface)

`internal interface ISettingsDialogs`

Source: `src/PressToShape.App/Services/ISettingsDialogs.cs:7-14`

Shows the settings window: a modal window on the desktop, an overlay inside the layout where there are no windows.

**Members**

- `public Task ShowAsync(SettingsViewModel viewModel)` - Shows the settings and returns when they are closed. The result is not returned: the caller listens to PressToShape.App.ViewModels.Editor.SettingsViewModel.CloseRequested and applies the values only after OK. A dialog closed in any other way counts as Cancel.

**Relationships**

- Implemented by `PressToShape.App.Services.AvaloniaSettingsDialogs`
- Implemented by `PressToShape.App.Services.OverlaySettingsDialogs`
- Implemented by `PressToShape.App.Tests.FakeSettingsDialogs`
- Injected into `PressToShape.App.Services.FileServices`

## LocalFileStore (class)

`internal sealed class LocalFileStore : IFileStore`

Source: `src/PressToShape.App/Services/LocalFileStore.cs:13-80`

The real file system. Writing goes to a temporary file in the same folder first and then replaces the target, so a failed write cannot damage an existing file. Errors are thrown, the caller shows them to the user. The file work runs on a pool thread, so the calling (UI) thread never waits for the disk.

**Members**

- `public Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default(CancellationToken))` - _(no documentation)_
- `public Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IFileStore`

## OverlayMessageDialogs (class)

`internal sealed class OverlayMessageDialogs : IMessageDialogs`

Source: `src/PressToShape.App/Services/OverlayMessageDialogs.cs:13-37`

The questions and messages for a platform without windows: they are shown inside the layout through a PressToShape.App.ViewModels.Tablet.MessageDialogHost. The texts and the results are the same as in PressToShape.App.Services.AvaloniaMessageDialogs.

**Members**

- `public OverlayMessageDialogs(MessageDialogHost host, ILocalizer localizer)` - _(no documentation)_
- `public Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)` - _(no documentation)_
- `public Task ShowErrorAsync(string message)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.MessageDialogHost`
- Implements `PressToShape.App.Services.IMessageDialogs`

## OverlaySettingsDialogs (class)

`internal sealed class OverlaySettingsDialogs : ISettingsDialogs`

Source: `src/PressToShape.App/Services/OverlaySettingsDialogs.cs:9-19`

Shows the settings for a platform without windows: inside the layout, through a PressToShape.App.ViewModels.Tablet.SettingsDialogHost.

**Members**

- `public OverlaySettingsDialogs(SettingsDialogHost host)` - _(no documentation)_
- `public Task ShowAsync(SettingsViewModel viewModel)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.ViewModels.Tablet.SettingsDialogHost`
- Implements `PressToShape.App.Services.ISettingsDialogs`

## PickedStorageFile (class)

`internal sealed class PickedStorageFile : IPickedFile`

Source: `src/PressToShape.App/Services/PickedStorageFile.cs:9-20`

A file from the platform's file picker, as an PressToShape.App.Services.IPickedFile.

**Members**

- `public PickedStorageFile(IStorageFile file)` - _(no documentation)_
- `public string Name { get; }` - _(no documentation)_
- `public Task<Stream> OpenReadAsync()` - _(no documentation)_
- `public Task<Stream> OpenWriteAsync()` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IPickedFile`

## StorageFileRegistry (class)

`internal sealed class StorageFileRegistry`

Source: `src/PressToShape.App/Services/StorageFileRegistry.cs:12-43`

Remembers the files that the platform's file picker returned, and hands out a text reference for each. The rest of the app works with file paths as text, but the Android picker gives no path, only a handle. A reference looks like storage:<id>/<file name>, so code that shows "the name after the last slash" shows the real file name. References only live as long as the app runs.

**Members**

- `public string Register(IStorageFile file)` - Remembers a file from the platform's file picker and returns its reference.
- `public string Register(IPickedFile file)` - Remembers a picked file and returns its reference.
- `public bool IsReference(string path)` - Whether a path is a reference of this registry. Such a name is the user's choice in the system picker and is never changed by the app.
- `public bool TryGet(string reference, out IPickedFile? file)` - Finds the file of a reference that PressToShape.App.Services.StorageFileRegistry.Register gave out.

**Relationships**

- Injected into `PressToShape.App.Services.AvaloniaFileDialogs`
- Injected into `PressToShape.App.Services.StorageFileStore`

## StorageFileStore (class)

`internal sealed class StorageFileStore : IFileStore`

Source: `src/PressToShape.App/Services/StorageFileStore.cs:14-131`

Reads and writes the files that the user picked (see PressToShape.App.Services.StorageFileRegistry). A picked file cannot be replaced through a temporary file, because the app has no access to its folder, and opening it for writing empties it at once. So a write first reads the old content, and if the write fails the old content is written back. If the app is killed in the middle of a write, the file can stay cut short. Errors of the platform are passed on as System.IO.IOException, so callers handle them like any other file error.

**Members**

- `public StorageFileStore(StorageFileRegistry registry)` - _(no documentation)_
- `public Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default(CancellationToken))` - _(no documentation)_
- `public Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Services.StorageFileRegistry`
- Implements `PressToShape.App.Services.IFileStore`

## TextFonts (class)

`internal static class TextFonts`

Source: `src/PressToShape.App/Services/TextFonts.cs:9-58`

Turns the font settings of a text into the typeface Avalonia draws with.

**Members**

- `public Typeface TypefaceFor(string name, bool bold, bool italic)` - The typeface for the font settings. A font that is on the system but that Avalonia cannot use (for example a Type 1 font such as "Courier 10 Pitch") would throw when text is laid out or drawn, so the default font is used for it instead. The name in the document is not changed.
- `public bool IsUsable(string name, bool bold, bool italic)` - Whether Avalonia can make a glyph typeface for the font in this style. The answer is remembered.
- `public FontFamily FamilyFor(string name)` - _(no documentation)_
- `internal FontFamily FamilyFor(string name, bool bundledAvailable)` - The family for the name, given whether the bundled font is registered.

## UnsavedChoice (enum)

`internal enum UnsavedChoice`

Source: `src/PressToShape.App/Services/UnsavedChoice.cs:4-14`

The user's answer to the question what to do with unsaved changes.

**Members**

- `public Save` - Save the changes, then go on.
- `public DontSave` - Throw the changes away and go on.
- `public Cancel` - Do not go on.
