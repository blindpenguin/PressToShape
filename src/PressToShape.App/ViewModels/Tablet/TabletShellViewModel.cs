using System;
using System.Threading.Tasks;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// Root of the tablet layout. Adds the compact toolbar with its overflow menu, the tool column, the floating panel and the
/// in-layout message dialog to what every shell has (see <see cref="EditorShellViewModel"/>). The Back button goes through
/// <see cref="RequestBackAsync"/>.
/// </summary>
internal sealed class TabletShellViewModel : EditorShellViewModel
{
    private readonly IAppShutdown _shutdown;

    /// <param name="shutdown">Ends the app. Called only when leaving is allowed.</param>
    /// <param name="localizer">The translator.</param>
    /// <param name="services">Files, pickers and dialogs. On the tablet the dialogs are shown through <paramref name="dialogs"/>.</param>
    /// <param name="dialogs">Holds the message dialog that the layout shows over everything else.</param>
    public TabletShellViewModel(IAppShutdown shutdown, ILocalizer localizer, FileServices services, MessageDialogHost dialogs)
        : base(localizer, services)
    {
        _shutdown = shutdown ?? throw new ArgumentNullException(nameof(shutdown));
        Dialogs = dialogs ?? throw new ArgumentNullException(nameof(dialogs));

        OverflowMenu = new OverflowMenuViewModel(localizer, FileCommands, EditCommands);
        Panel = new TabletPanelViewModel(Inspector, localizer);
        Column = new TabletToolColumnViewModel(Toolbox, State, localizer, new TabletColorSwatchViewModel(Inspector.Properties, Panel, localizer));
        Toolbar = new TabletToolbarViewModel(localizer, this, UndoCommand, RedoCommand, new ZoomViewModel(State, localizer, FitToWindow), OverflowMenu, Panel);
    }

    public TabletToolbarViewModel Toolbar { get; }

    /// <summary>The "..." menu. It is also reachable as <see cref="Toolbar"/>.OverflowMenu.</summary>
    public OverflowMenuViewModel OverflowMenu { get; }

    public TabletPanelViewModel Panel { get; }

    /// <summary>The tool column with its tool groups and the swatches.</summary>
    public TabletToolColumnViewModel Column { get; }

    /// <summary>The message dialog that is shown over the layout, or nothing.</summary>
    public MessageDialogHost Dialogs { get; }

    /// <summary>
    /// The Back button was pressed (after the view closed a toolbar menu, if one was open). It closes the topmost thing that is open and
    /// stops there: a message dialog (as Cancel), then a tool group flyout, then the panel. Only when none of them is open does it do what
    /// <see cref="RequestLeaveAsync"/> does.
    /// </summary>
    /// <returns>Whether the app was told to end.</returns>
    public Task<bool> RequestBackAsync()
    {
        if (Dialogs.IsOpen)
        {
            Dialogs.Dismiss();
            return Task.FromResult(false);
        }

        if (Column.HasOpenFlyout)
        {
            Column.CloseFlyouts();
            return Task.FromResult(false);
        }

        if (Panel.IsOpen)
        {
            Panel.Close();
            return Task.FromResult(false);
        }

        return RequestLeaveAsync();
    }

    /// <summary>
    /// Leaving was asked for. While a message dialog is open, Back closes it like Cancel and the app stays. Otherwise: without unsaved
    /// changes the app is left at once, with them the user is asked to save, discard or cancel, and the app is left only after a
    /// successful save or after discarding.
    /// </summary>
    /// <returns>Whether the app was told to end.</returns>
    public async Task<bool> RequestLeaveAsync()
    {
        if (Dialogs.IsOpen)
        {
            Dialogs.Dismiss();
            return false;
        }

        if (!await ConfirmCloseAsync())
        {
            return false;
        }

        _shutdown.Shutdown();
        return true;
    }
}
