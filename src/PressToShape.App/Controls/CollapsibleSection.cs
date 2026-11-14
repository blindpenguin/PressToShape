using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace PressToShape.App.Controls;

/// <summary>
/// A titled section whose whole header row toggles the content. Used by the inspector panels.
/// The look is defined in Styles/Controls.axaml.
/// </summary>
internal sealed class CollapsibleSection : HeaderedContentControl
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<CollapsibleSection, bool>(nameof(IsExpanded), true, defaultBindingMode: BindingMode.TwoWay);

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}
