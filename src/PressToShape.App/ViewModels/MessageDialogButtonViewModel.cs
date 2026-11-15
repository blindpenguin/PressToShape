using System.Windows.Input;

namespace PressToShape.App.ViewModels;

/// <summary>One button of a message dialog. Choosing it closes the dialog with <paramref name="Result"/>.</summary>
/// <param name="Text">The translated button text.</param>
/// <param name="Result">The value the dialog returns when this button is chosen.</param>
/// <param name="IsDefault">Whether Enter chooses this button.</param>
/// <param name="IsCancel">Whether Escape chooses this button.</param>
/// <param name="Command">Chooses the button.</param>
internal sealed record MessageDialogButtonViewModel(string Text, int Result, bool IsDefault, bool IsCancel, ICommand Command);
