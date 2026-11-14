using Avalonia.Controls;
using Avalonia.Input;
using PressToShape.App.ViewModels.Desktop;

namespace PressToShape.App.Views.Desktop;

public partial class DesktopShellView : UserControl
{
    public DesktopShellView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Single-key tool shortcuts. The view only decides whether the key press is meant for the shell
    /// (no modifiers, no text field focused). The view model decides which tool it selects.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Handled || DataContext is not DesktopShellViewModel shell)
        {
            return;
        }

        if (e.KeyModifiers != KeyModifiers.None && e.KeyModifiers != KeyModifiers.Shift)
        {
            return;
        }

        if (TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is TextBox)
        {
            return;
        }

        if (e.KeySymbol is { Length: 1 } symbol && shell.Toolbox.TrySelectByKey(symbol[0]))
        {
            e.Handled = true;
        }
    }
}
