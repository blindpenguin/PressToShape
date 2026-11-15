# PressToShape.App.Services

Project: PressToShape.App

## AvaloniaFileDialogs (class)

`internal sealed class AvaloniaFileDialogs : IFileDialogs`

Source: `src/PressToShape.App/Services/AvaloniaFileDialogs.cs:15-73`

The platform's file pickers, opened over the main window. A picker that is not available, or a result that is not a local file, gives null, which the callers treat like Cancel.

**Members**

- `public AvaloniaFileDialogs(Func<TopLevel?> owner, ILocalizer localizer)` - _(no documentation)_
- `public Task<string?> PickProjectToOpenAsync()` - _(no documentation)_
- `public Task<string?> PickProjectToSaveAsync(string suggestedFileName)` - _(no documentation)_
- `public Task<string?> PickSvgToExportAsync(string suggestedFileName)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Implements `PressToShape.App.Services.IFileDialogs`

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

Source: `src/PressToShape.App/Services/FileServices.cs:6-6`

What the file actions need from the outside: settings, files, file pickers and message dialogs.

**Members**

- `public FileServices(ISettingsStore Settings, IFileStore Files, IFileDialogs Dialogs, IMessageDialogs Messages)` - What the file actions need from the outside: settings, files, file pickers and message dialogs.
- `public IFileDialogs Dialogs { get; init; }` - _(no documentation)_
- `public IFileStore Files { get; init; }` - _(no documentation)_
- `public IMessageDialogs Messages { get; init; }` - _(no documentation)_
- `public ISettingsStore Settings { get; init; }` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Services.IFileDialogs`
- Depends on (constructor) `PressToShape.App.Services.IFileStore`
- Depends on (constructor) `PressToShape.App.Services.IMessageDialogs`
- Depends on (constructor) `PressToShape.App.Settings.ISettingsStore`
- Injected into `PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`

## IAppShutdown (interface)

`internal interface IAppShutdown`

Source: `src/PressToShape.App/Services/IAppShutdown.cs:4-7`

Lets view models close the application without referencing Avalonia types.

**Members**

- `public void Shutdown()` - _(no documentation)_

**Relationships**

- Implemented by `PressToShape.App.Services.DesktopAppShutdown`
- Implemented by `PressToShape.App.Tests.DesktopShellViewModelTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.MenuBarViewModelTests.FakeShutdown`
- Implemented by `PressToShape.App.Tests.SelectionAndPropertiesIntegrationTests.NoShutdown`
- Implemented by `PressToShape.App.Tests.TranslationConsistencyTests.NoShutdown`
- Injected into `PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`

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

Source: `src/PressToShape.App/Services/IFileStore.cs:4-20`

Reads and writes whole files. Lets the file actions be tested without a real file system.

**Members**

- `public byte[] ReadAllBytes(string path, long maxBytes)` - Reads a whole file.
- `public void WriteAllBytesAtomic(string path, byte[] bytes)` - Writes a whole file so that an existing file is only replaced once the new content is complete. If writing fails, the existing file stays as it was and no partial file stays behind.
- `public bool Exists(string path)` - Whether a file exists at the path.

**Relationships**

- Implemented by `PressToShape.App.Services.LocalFileStore`
- Implemented by `PressToShape.App.Tests.FakeFileStore`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.Tests.DocumentFileServiceTests.Setup`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## IMessageDialogs (interface)

`internal interface IMessageDialogs`

Source: `src/PressToShape.App/Services/IMessageDialogs.cs:6-13`

The questions and messages shown to the user. The texts are translated by the caller.

**Members**

- `public Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)` - Asks whether unsaved changes of the named document should be saved. Closing the question counts as Cancel.
- `public Task ShowErrorAsync(string message)` - Shows an error message and returns when the user has dismissed it.

**Relationships**

- Implemented by `PressToShape.App.Services.AvaloniaMessageDialogs`
- Implemented by `PressToShape.App.Tests.FakeMessageDialogs`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## LocalFileStore (class)

`internal sealed class LocalFileStore : IFileStore`

Source: `src/PressToShape.App/Services/LocalFileStore.cs:10-73`

The real file system. Writing goes to a temporary file in the same folder first and then replaces the target, so a failed write cannot damage an existing file. Errors are thrown, the caller shows them to the user.

**Members**

- `public byte[] ReadAllBytes(string path, long maxBytes)` - _(no documentation)_
- `public void WriteAllBytesAtomic(string path, byte[] bytes)` - _(no documentation)_
- `public bool Exists(string path)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IFileStore`

## UnsavedChoice (enum)

`internal enum UnsavedChoice`

Source: `src/PressToShape.App/Services/UnsavedChoice.cs:4-14`

The user's answer to the question what to do with unsaved changes.

**Members**

- `public Save` - Save the changes, then go on.
- `public DontSave` - Throw the changes away and go on.
- `public Cancel` - Do not go on.
