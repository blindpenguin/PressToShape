using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using PressToShape.App.ViewModels.Editor;

namespace PressToShape.App.Controls;

/// <summary>
/// What a number text box does with focus and keys, for every view that shows <see cref="NumberFieldViewModel"/>s: the value is applied when the
/// box is left or Enter is pressed, and the shown value comes back on Escape. A text box takes part when its <c>Tag</c> holds its field view
/// model. The handlers sit on the view, so keys in other text boxes keep their meaning. The view model decides what the value means.
/// </summary>
internal static class NumberFieldBehavior
{
    /// <summary>Lets the number fields below <paramref name="view"/> commit and revert.</summary>
    public static void Attach(Interactive view)
    {
        view.AddHandler(InputElement.GotFocusEvent, OnGotFocus);
        view.AddHandler(InputElement.LostFocusEvent, OnLostFocus);
        view.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private static NumberFieldViewModel? FieldOf(RoutedEventArgs e) => (e.Source as TextBox)?.Tag as NumberFieldViewModel;

    private static void OnGotFocus(object? sender, FocusChangedEventArgs e)
    {
        if (FieldOf(e) is { } field)
        {
            field.IsEditing = true;
        }
    }

    private static void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (FieldOf(e) is { } field)
        {
            field.IsEditing = false;
            field.Commit();
        }
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (FieldOf(e) is not { } field)
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            field.Commit();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            field.Revert();
            e.Handled = true;
        }
    }
}
