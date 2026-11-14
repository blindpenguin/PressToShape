using System.Collections.Generic;
using System.Windows.Input;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// One entry of the menu bar. A leaf item without a command is disabled. A submenu is always enabled so it
/// can be opened and its (possibly disabled) items inspected. A separator is rendered with the header "-".
/// </summary>
internal sealed class MenuItemViewModel
{
    public const string SeparatorHeader = "-";

    private MenuItemViewModel(string header, string? gesture, ICommand? command, IReadOnlyList<MenuItemViewModel> children)
    {
        Header = header;
        Gesture = gesture;
        Command = command;
        Children = children;
    }

    public string Header { get; }

    /// <summary>Shortcut text shown next to the item. Display only.</summary>
    public string? Gesture { get; }

    public ICommand? Command { get; }

    public IReadOnlyList<MenuItemViewModel> Children { get; }

    public bool IsSeparator => Header == SeparatorHeader;

    public bool IsEnabled => !IsSeparator && (Command is not null || Children.Count > 0);

    public static MenuItemViewModel Item(string header, string? gesture = null, ICommand? command = null) =>
        new(header, gesture, command, []);

    public static MenuItemViewModel Submenu(string header, params MenuItemViewModel[] children) =>
        new(header, null, null, children);

    public static MenuItemViewModel Separator() => new(SeparatorHeader, null, null, []);
}
