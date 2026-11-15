using PressToShape.App.Settings;

namespace PressToShape.App.Services;

/// <summary>What the file actions need from the outside: settings, files, file pickers and message dialogs.</summary>
internal sealed record FileServices(ISettingsStore Settings, IFileStore Files, IFileDialogs Dialogs, IMessageDialogs Messages);
