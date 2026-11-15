using System;
using System.Collections.Generic;
using System.Windows.Input;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.ViewModels.Editor;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// The "..." menu of the tablet toolbar: the file actions, the edit actions and the language. It uses the same texts as the desktop menus
/// (so the translations are shared) and the same commands, shown without Alt keys and shortcut texts through <see cref="MenuItemViewModel.PlainHeader"/>.
/// There is no Quit (an Android app is left with the Back button) and no list of recent files (their references do not survive a restart).
/// An item without a command, or whose command cannot run, is disabled.
/// </summary>
internal sealed class OverflowMenuViewModel : ViewModelBase
{
    public OverflowMenuViewModel(ILocalizer localizer, FileMenuCommands files, EditMenuCommands edit)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(edit);

        MenuItemViewModel Item(string header, ICommand command) => MenuItemViewModel.Item(localizer, header, null, command);

        Items =
        [
            Item(N("_New"), files.New),
            Item(N("_Open…"), files.Open),
            Item(N("_Save"), files.Save),
            Item(N("Save _As…"), files.SaveAs),
            Item(N("_Export as SVG…"), files.ExportSvg),
            MenuItemViewModel.Separator(localizer),
            Item(N("Cu_t"), edit.Cut),
            Item(N("_Copy"), edit.Copy),
            Item(N("_Paste"), edit.Paste),
            Item(N("Paste in Plac_e"), edit.PasteInPlace),
            Item(N("_Duplicate"), edit.Duplicate),
            Item(N("De_lete"), edit.Delete),
            Item(N("Select _All"), edit.SelectAll),
            Item(N("De_select"), edit.Deselect),
            MenuItemViewModel.Separator(localizer),
            MenuItemViewModel.Submenu(localizer, N("_Language"), MenuItemViewModel.LanguageItems(localizer)),
        ];
    }

    /// <summary>The items in the order they are shown, with separators between the groups.</summary>
    public IReadOnlyList<MenuItemViewModel> Items { get; }
}
