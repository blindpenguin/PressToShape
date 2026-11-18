# PressToShape.App.Settings

Project: PressToShape.App

## AppSettings (class)

`internal sealed class AppSettings`

Source: `src/PressToShape.App/Settings/AppSettings.cs:8-37`

User settings stored in settings.json. Unknown entries are kept, so newer settings survive a save by an older version.

**Members**

- `public string? Language { get; set; }` - The language code chosen by the user, for example "de". Null means no choice has been made.
- `public JsonElement? RecentFiles { get; set; }` - The recently used files, most recent first, as a JSON array of paths. It is kept as a raw JSON value on purpose: a damaged value must not make the whole settings file unreadable. PressToShape.App.Workspace.RecentFiles reads and writes it.
- `public JsonElement? ShowGrid { get; set; }` - Whether the grid is shown. Kept as a raw JSON value on purpose, like PressToShape.App.Settings.AppSettings.RecentFiles: a damaged value must not make the whole file unreadable.
- `public JsonElement? SnapToGrid { get; set; }` - Whether moves and drawing snap to the grid (raw JSON value).
- `public JsonElement? SnapToObjects { get; set; }` - Whether moves and drawing snap to other objects (raw JSON value).
- `public JsonElement? GridSpacing { get; set; }` - The grid spacing in document units (raw JSON value).
- `public JsonElement? ShowRulers { get; set; }` - Whether the rulers are shown (raw JSON value). Missing means the default of the layout.
- `public Dictionary<string, JsonElement>? ExtraData { get; set; }` - Entries this version does not know about.

## ISettingsStore (interface)

`internal interface ISettingsStore`

Source: `src/PressToShape.App/Settings/ISettingsStore.cs:4-11`

Loads and saves PressToShape.App.Settings.AppSettings. Neither operation throws.

**Members**

- `public AppSettings Load()` - Returns the saved settings, or default settings if there are none or they cannot be read.
- `public void Save(AppSettings settings)` - Saves the settings. Failures are ignored: the settings then only apply to the current session.

**Relationships**

- Implemented by `PressToShape.App.Settings.JsonFileSettingsStore`
- Implemented by `PressToShape.App.Tests.LanguagePreferenceTests.MemoryStore`
- Implemented by `PressToShape.App.Tests.MemorySettingsStore`
- Implemented by `PressToShape.App.Tests.RecentFilesTests.MemoryStore`
- Implemented by `PressToShape.App.Workspace.SnapPreferences.NoStore`
- Injected into `PressToShape.App.Localization.LanguagePreference`
- Injected into `PressToShape.App.Services.FileServices`
- Injected into `PressToShape.App.Workspace.RecentFiles`
- Injected into `PressToShape.App.Workspace.SnapPreferences`

## JsonFileSettingsStore (class)

`internal sealed class JsonFileSettingsStore : ISettingsStore`

Source: `src/PressToShape.App/Settings/JsonFileSettingsStore.cs:13-96`

Stores settings as JSON in a file. The file content is treated as untrusted input: anything unreadable gives default settings. Saving writes a temporary file and then replaces the target, so a failed write does not damage an existing file.

**Members**

- `public JsonFileSettingsStore(string path)` - _(no documentation)_
- `public string DefaultPath { get; }` - The settings file in the user's application-data folder: PressToShape/settings.json.
- `public AppSettings Load()` - _(no documentation)_
- `public void Save(AppSettings settings)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Settings.ISettingsStore`
