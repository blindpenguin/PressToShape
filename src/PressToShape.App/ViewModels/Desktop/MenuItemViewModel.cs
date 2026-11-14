using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using PressToShape.App.Localization;

namespace PressToShape.App.ViewModels.Desktop;

/// <summary>
/// One entry of the menu bar. The header is translated and refreshes when the language changes.
/// A leaf item without a command is disabled. A submenu is always enabled so it can be opened and its
/// (possibly disabled) items inspected. A separator is rendered with the header "-".
/// </summary>
internal sealed class MenuItemViewModel : LocalizedViewModel
{
    public const string SeparatorHeader = "-";

    private readonly string _header;
    private readonly bool _translateHeader;
    private readonly Func<bool>? _isChecked;

    private MenuItemViewModel(
        ILocalizer localizer,
        string header,
        bool translateHeader,
        string? gesture,
        ICommand? command,
        IReadOnlyList<MenuItemViewModel> children,
        Func<bool>? isChecked)
        : base(localizer)
    {
        _header = header;
        _translateHeader = translateHeader && header != SeparatorHeader;
        Gesture = gesture;
        Command = command;
        Children = children;
        _isChecked = isChecked;
    }

    /// <summary>The shown header: the translation of the message id, or a fixed text for language names.</summary>
    public string Header => _translateHeader ? Localizer.T(_header) : _header;

    /// <summary>Shortcut text shown next to the item. Display only.</summary>
    public string? Gesture { get; }

    public ICommand? Command { get; }

    public IReadOnlyList<MenuItemViewModel> Children { get; }

    public bool IsSeparator => _header == SeparatorHeader;

    public bool IsEnabled => !IsSeparator && (Command is not null || Children.Count > 0);

    /// <summary>True for items that show a check state, such as the languages in the Language submenu.</summary>
    public bool IsCheckable => _isChecked is not null;

    /// <summary>
    /// The check state. Setting it has no effect: the item's command changes the state, and the state is then read again.
    /// </summary>
    public bool IsChecked
    {
        get => _isChecked?.Invoke() ?? false;
        set => OnPropertyChanged();
    }

    /// <summary>A menu item whose header is a message id (an English text, usually with an Alt-key underscore).</summary>
    public static MenuItemViewModel Item(ILocalizer localizer, string header, string? gesture = null, ICommand? command = null) =>
        new(localizer, header, true, gesture, command, [], null);

    public static MenuItemViewModel Submenu(ILocalizer localizer, string header, params MenuItemViewModel[] children) =>
        new(localizer, header, true, null, null, children, null);

    public static MenuItemViewModel Separator(ILocalizer localizer) =>
        new(localizer, SeparatorHeader, false, null, null, [], null);

    /// <summary>A checkable item with a fixed, untranslated header, for language names such as "Deutsch".</summary>
    public static MenuItemViewModel Checkable(ILocalizer localizer, string fixedHeader, ICommand command, Func<bool> isChecked) =>
        new(localizer, fixedHeader, false, null, command, [], isChecked ?? throw new ArgumentNullException(nameof(isChecked)));

    /// <summary>All items of this item and its descendants.</summary>
    public IEnumerable<MenuItemViewModel> Descendants() =>
        Children.SelectMany(c => new[] { c }.Concat(c.Descendants()));
}
