using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        Func<bool>? isChecked,
        string? toolTip = null)
        : base(localizer)
    {
        _header = header;
        _translateHeader = translateHeader && header != SeparatorHeader;
        Gesture = gesture;
        Command = command;
        Children = children;
        _isChecked = isChecked;
        ToolTip = toolTip;

        if (command is not null)
        {
            // Menu items live as long as the app, so this subscription is never removed.
            command.CanExecuteChanged += (_, _) => OnPropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>The shown header: the translation of the message id, or a fixed text for language names.</summary>
    public string Header => _translateHeader ? Localizer.T(_header) : _header;

    /// <summary>
    /// The header without the Alt-key underscore, for a menu that has no Alt keys, such as the overflow menu of the tablet layout.
    /// A single underscore marks the key and is dropped, a double underscore is one underscore. A fixed text (a file or language name) is
    /// data and stays as it is.
    /// </summary>
    public string PlainHeader => _translateHeader ? RemoveMnemonic(Header) : Header;

    /// <summary>Shortcut text shown next to the item. Display only.</summary>
    public string? Gesture { get; }

    /// <summary>Tooltip text, or <c>null</c>. Not translated: it is used for data such as a folder path.</summary>
    public string? ToolTip { get; }

    public ICommand? Command { get; }

    public IReadOnlyList<MenuItemViewModel> Children { get; }

    public bool IsSeparator => _header == SeparatorHeader;

    /// <summary>A submenu is always enabled. A leaf item is enabled while its command can execute, and an item without a command is disabled.</summary>
    public bool IsEnabled => !IsSeparator && ((Command?.CanExecute(null) ?? false) || Children.Count > 0);

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

    /// <summary>
    /// A submenu whose items are replaced while the app runs, such as Open Recent. The caller changes the collection,
    /// and the menu follows. Give it at least one item at all times so the submenu stays enabled.
    /// </summary>
    public static MenuItemViewModel Submenu(ILocalizer localizer, string header, ObservableCollection<MenuItemViewModel> children) =>
        new(localizer, header, true, null, null, children ?? throw new ArgumentNullException(nameof(children)), null);

    /// <summary>An item with a fixed, untranslated text, for data such as a file name, and an optional tooltip.</summary>
    public static MenuItemViewModel Fixed(ILocalizer localizer, string text, ICommand command, string? toolTip = null) =>
        new(localizer, text, false, null, command ?? throw new ArgumentNullException(nameof(command)), [], null, toolTip);

    public static MenuItemViewModel Separator(ILocalizer localizer) =>
        new(localizer, SeparatorHeader, false, null, null, [], null);

    /// <summary>A checkable item with a fixed, untranslated header, for language names such as "Deutsch".</summary>
    public static MenuItemViewModel Checkable(ILocalizer localizer, string fixedHeader, ICommand command, Func<bool> isChecked) =>
        new(localizer, fixedHeader, false, null, command, [], isChecked ?? throw new ArgumentNullException(nameof(isChecked)));

    /// <summary>Removes the mnemonic marker of a menu text: "_Save" gives "Save", "Save __ As" gives "Save _ As".</summary>
    internal static string RemoveMnemonic(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var plain = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '_')
            {
                plain.Append(text[i]);
            }
            else if (i + 1 < text.Length && text[i + 1] == '_')
            {
                plain.Append('_');
                i++;
            }
        }

        return plain.ToString();
    }

    /// <summary>One checkable item for every language of the app, named in its own language. Choosing one switches the language.</summary>
    public static MenuItemViewModel[] LanguageItems(ILocalizer localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);

        return localizer.Languages
            .Select(language => Checkable(
                localizer,
                language.DisplayName,
                new CommunityToolkit.Mvvm.Input.RelayCommand(() => localizer.SetLanguage(language.Code)),
                () => localizer.LanguageCode == language.Code))
            .ToArray();
    }

    /// <summary>All items of this item and its descendants.</summary>
    public IEnumerable<MenuItemViewModel> Descendants() =>
        Children.SelectMany(c => new[] { c }.Concat(c.Descendants()));
}
