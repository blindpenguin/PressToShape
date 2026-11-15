using Avalonia;
using Avalonia.Controls;

namespace PressToShape.App.Controls;

/// <summary>
/// The sizes for touch and stylus, defined in <c>Styles/Theme.axaml</c> as the dictionary <c>TouchSizes</c>. The tablet layout applies them to its
/// root, so every control below it that looks a size up with <c>DynamicResource</c> gets the touch value, and the desktop keeps its own.
/// The dictionary cannot simply be merged, because Avalonia allows a dictionary only one owner. Its entries are copied instead.
/// </summary>
internal static class TouchSizes
{
    public const string Key = "TouchSizes";

    /// <summary>Puts the touch sizes into the resources of <paramref name="root"/>. Does nothing if the application has no such dictionary.</summary>
    public static void ApplyTo(Control root)
    {
        if (Application.Current?.FindResource(Key) is not ResourceDictionary sizes)
        {
            return;
        }

        foreach (var (key, value) in sizes)
        {
            root.Resources[key] = value;
        }
    }
}
