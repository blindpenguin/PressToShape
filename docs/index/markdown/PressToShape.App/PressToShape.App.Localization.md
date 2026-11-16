# PressToShape.App.Localization

Project: PressToShape.App

## EmbeddedCatalogSource (class)

`internal sealed class EmbeddedCatalogSource : ICatalogSource`

Source: `src/PressToShape.App/Localization/EmbeddedCatalogSource.cs:13-53`

Loads the compiled catalog of a language from the embedded resource PressToShape.App.Translations.CODE.mo. Never throws: a missing or unreadable catalog means English.

**Members**

- `public EmbeddedCatalogSource()` - _(no documentation)_
- `public EmbeddedCatalogSource(Assembly assembly)` - _(no documentation)_
- `public ITranslationCatalog? Load(string languageCode)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Localization.ICatalogSource`

## GettextCatalog (class)

`internal sealed class GettextCatalog : ITranslationCatalog`

Source: `src/PressToShape.App/Localization/GettextCatalog.cs:9-42`

A translation catalog backed by a compiled gettext .mo file, through GetText.NET.

**Members**

- `public GettextCatalog(Stream moStream, CultureInfo culture)` - Reads the catalog from a .mo stream. Throws if the stream is not a valid .mo file.
- `public string? Translate(string text)` - _(no documentation)_
- `public string? Translate(string context, string text)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Localization.ITranslationCatalog`

## ICatalogSource (interface)

`internal interface ICatalogSource`

Source: `src/PressToShape.App/Localization/ICatalogSource.cs:4-8`

Finds the translation catalog of a language. English has no catalog, because the source text is English.

**Members**

- `public ITranslationCatalog? Load(string languageCode)` - Loads the catalog for a language code such as "de". Returns null when there is none.

**Relationships**

- Implemented by `PressToShape.App.Localization.EmbeddedCatalogSource`
- Implemented by `PressToShape.App.Localization.LocalizationHost.NoCatalogSource`
- Implemented by `PressToShape.App.Tests.FakeCatalogSource`
- Injected into `PressToShape.App.Localization.Localizer`

## ILocalizer (interface)

`internal interface ILocalizer`

Source: `src/PressToShape.App/Localization/ILocalizer.cs:8-29`

Translates user-visible texts. The English source text is the message id.

**Members**

- `public CultureInfo Culture { get; }` - The culture of the current language, used for number formatting.
- `public string LanguageCode { get; }` - The code of the current language, for example "en" or "de".
- `public IReadOnlyList<LanguageInfo> Languages { get; }` - _(no documentation)_
- `public string T(string text)` - Translates a text. Returns the English text when there is no translation.
- `public string TC(string context, string text)` - Translates a text with a message context, for equal texts that need different translations.
- `public void SetLanguage(string code)` - Switches the language. Unknown codes and the current language are ignored.
- `public EventHandler? LanguageChanged` - Raised after the language has changed.

**Relationships**

- Implemented by `PressToShape.App.Localization.Localizer`
- Injected into `PressToShape.App.Localization.LanguagePreference`
- Injected into `PressToShape.App.Localization.LocalizedTextObservable`
- Injected into `PressToShape.App.Services.AvaloniaFileDialogs`
- Injected into `PressToShape.App.Services.AvaloniaMessageDialogs`
- Injected into `PressToShape.App.Services.OverlayMessageDialogs`
- Injected into `PressToShape.App.Tests.EditorShellViewModelRingTests.BareShell`
- Injected into `PressToShape.App.Tests.EditorShellViewModelTests.BareShell`
- Injected into `PressToShape.App.Tests.EditorShellViewModelTests.Fixture`
- Injected into `PressToShape.App.Tests.RingMenuControlTests.BareShell`
- Injected into `PressToShape.App.Tests.RingMenuControlTests.Scene`
- Injected into `PressToShape.App.Tests.TestShell`
- Injected into `PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuBarViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.MenuItemViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.StatusBarViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel`
- Injected into `PressToShape.App.ViewModels.Desktop.ToolbarViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.EditorShellViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.HistoryItemViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.HistoryViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.InspectorViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.LayersViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.NumberFieldViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.PropertiesViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.RingMenuViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.RingSlotViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ToolItemViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ToolboxViewModel`
- Injected into `PressToShape.App.ViewModels.Editor.ZoomViewModel`
- Injected into `PressToShape.App.ViewModels.LocalizedViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.PanelButtonViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletShellViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`
- Injected into `PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`
- Injected into `PressToShape.App.Workspace.DocumentFileService`

## ITranslationCatalog (interface)

`internal interface ITranslationCatalog`

Source: `src/PressToShape.App/Localization/ITranslationCatalog.cs:4-9`

A set of translations for one language. Returns null when a text has no translation.

**Members**

- `public string? Translate(string text)` - _(no documentation)_
- `public string? Translate(string context, string text)` - _(no documentation)_

**Relationships**

- Implemented by `PressToShape.App.Localization.GettextCatalog`
- Implemented by `PressToShape.App.Tests.FakeCatalog`

## LanguageInfo (record)

`internal sealed record LanguageInfo : IEquatable<LanguageInfo>`

Source: `src/PressToShape.App/Localization/LanguageInfo.cs:4-4`

A language the app can show. The display name is written in the language itself and is not translated.

**Members**

- `public LanguageInfo(string Code, string DisplayName)` - A language the app can show. The display name is written in the language itself and is not translated.
- `public string Code { get; init; }` - _(no documentation)_
- `public string DisplayName { get; init; }` - _(no documentation)_

## LanguagePreference (class)

`internal sealed class LanguagePreference`

Source: `src/PressToShape.App/Localization/LanguagePreference.cs:10-46`

Connects the localizer to the settings: it chooses the start language and saves the language when it changes.

**Members**

- `public string ChooseStartLanguage(ISettingsStore store, CultureInfo systemUiCulture)` - The start language: the saved choice, otherwise German for a German system, otherwise English.
- `public LanguagePreference Attach(ISettingsStore store, ILocalizer localizer)` - Saves the language whenever it changes. Keep the returned object alive for the app's lifetime.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Settings.ISettingsStore`

## LanguageSelector (class)

`internal static class LanguageSelector`

Source: `src/PressToShape.App/Localization/LanguageSelector.cs:7-25`

The startup rule for the language: saved choice, then German for a German system, otherwise English.

**Members**

- `public string Choose(string? savedCode, CultureInfo systemUiCulture)` - _(no documentation)_

## LocalizationHost (class)

`internal static class LocalizationHost`

Source: `src/PressToShape.App/Localization/LocalizationHost.cs:9-25`

Holds the application's localizer for XAML markup extensions, which cannot receive injected services. View models must not use this class: they get an PressToShape.App.Localization.ILocalizer through their constructor.

**Members**

- `public ILocalizer Localizer { get; }` - The application localizer. Before startup sets it, this is an English localizer without catalogs.
- `public void Initialize(ILocalizer localizer)` - _(no documentation)_

## LocalizedTextObservable (class)

`internal sealed class LocalizedTextObservable : IObservable<string>`

Source: `src/PressToShape.App/Localization/LocalizedTextObservable.cs:9-46`

An observable text that emits the current translation when subscribed and again after every language change. It is the source of the bindings created by PressToShape.App.Localization.TExtension.

**Members**

- `public LocalizedTextObservable(ILocalizer localizer, string? context, string text)` - _(no documentation)_
- `public IDisposable Subscribe(IObserver<string> observer)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`

## Localizer (class)

`internal sealed class Localizer : ILocalizer`

Source: `src/PressToShape.App/Localization/Localizer.cs:11-69`

Holds the current language and translates texts through the catalog of that language. English has no catalog: the English source text is returned unchanged.

**Members**

- `public Localizer(ICatalogSource source, string initialLanguage = "en")` - _(no documentation)_
- `public EventHandler? LanguageChanged` - _(no documentation)_
- `public CultureInfo Culture { get; private set; }` - _(no documentation)_
- `public string LanguageCode { get; private set; }` - _(no documentation)_
- `public IReadOnlyList<LanguageInfo> Languages { get; }` - _(no documentation)_
- `public string N(string text)` - Marks a text as translatable without translating it now. Use it in data tables, then translate with PressToShape.App.Localization.Localizer.T when the text is shown. The marker lets the translation checks find the text.
- `public string T(string text)` - _(no documentation)_
- `public string TC(string context, string text)` - _(no documentation)_
- `public void SetLanguage(string code)` - _(no documentation)_

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ICatalogSource`
- Implements `PressToShape.App.Localization.ILocalizer`
- Injected into `PressToShape.App.Tests.DesktopShellViewModelTests.FileShell`
- Injected into `PressToShape.App.Tests.DocumentFileServiceTests.Setup`
- Injected into `PressToShape.App.Tests.LayersViewModelTests.Fixture`

## SupportedLanguages (class)

`internal static class SupportedLanguages`

Source: `src/PressToShape.App/Localization/SupportedLanguages.cs:10-26`

The languages the app supports. To add a language: add its .po file to Translations/ and one entry here.

**Members**

- `public string DefaultCode` - _(no documentation)_
- `public IReadOnlyList<LanguageInfo> All { get; }` - _(no documentation)_
- `public bool IsSupported(string? code)` - _(no documentation)_
- `public LanguageInfo? Find(string? code)` - _(no documentation)_

## TExtension (class)

`internal sealed class TExtension : MarkupExtension`

Source: `src/PressToShape.App/Localization/TExtension.cs:11-33`

XAML markup extension for translated text: Text="{loc:T 'Open'}". The English text is the message id. The value updates when the language changes. An optional PressToShape.App.Localization.TExtension.Context selects a message context.

**Members**

- `public TExtension()` - _(no documentation)_
- `public TExtension(string text)` - _(no documentation)_
- `public string Text { get; set; }` - The English source text, used as the message id.
- `public string? Context { get; set; }` - The gettext message context, or null for none.
- `public object ProvideValue(IServiceProvider serviceProvider)` - _(no documentation)_
