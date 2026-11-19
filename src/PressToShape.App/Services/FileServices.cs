using PressToShape.App.Settings;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Services;

/// <summary>What the file actions need from the outside: settings, files, file pickers and message dialogs.</summary>
/// <param name="RemembersRecentFiles">Whether the recently used files are kept in the settings. Off where a file reference does not survive a restart (Android).</param>
/// <param name="SettingsDialogs">Shows the settings window. <c>null</c> where there is none: "Preferences…" is then disabled.</param>
/// <param name="TextMeasurer">Measures texts. <c>null</c> uses Avalonia's text engine, tests pass a fake.</param>
/// <param name="TextOutliner">Gives the glyph contours of texts. <c>null</c> uses Avalonia's fonts, tests pass a fake.</param>
/// <param name="FontCatalog">The font families to choose from. <c>null</c> uses the fonts of the system, tests pass a fake.</param>
internal sealed record FileServices(
    ISettingsStore Settings,
    IFileStore Files,
    IFileDialogs Dialogs,
    IMessageDialogs Messages,
    bool RemembersRecentFiles = true,
    ISettingsDialogs? SettingsDialogs = null,
    ITextMeasurer? TextMeasurer = null,
    ITextOutliner? TextOutliner = null,
    IFontCatalog? FontCatalog = null);
