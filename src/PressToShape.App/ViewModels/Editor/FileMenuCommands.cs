using System;
using System.Windows.Input;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>What the File menu items do. <paramref name="OpenRecent"/> creates the command for one entry of the recent list.</summary>
internal sealed record FileMenuCommands(
    ICommand New,
    ICommand Open,
    ICommand Save,
    ICommand SaveAs,
    ICommand ExportSvg,
    Func<string, ICommand> OpenRecent);
