using PressToShape.App.Settings;

namespace PressToShape.App.Services;

/// <summary>What the file actions need from the outside: settings, files, file pickers and message dialogs.</summary>
/// <param name="RemembersRecentFiles">Whether the recently used files are kept in the settings. Off where a file reference does not survive a restart (Android).</param>
internal sealed record FileServices(ISettingsStore Settings, IFileStore Files, IFileDialogs Dialogs, IMessageDialogs Messages, bool RemembersRecentFiles = true);
