using System;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

// The Corners section of the Properties tab: the corner radii of the one selected rectangle. The fields show the radii that are drawn
// (the effective ones). A locked or hidden rectangle cannot be selected (the editor state drops it from the selection), so the section
// never shows one. An entry above the limit is limited to half of the shorter side, and the field then shows the limited value.
internal sealed partial class PropertiesViewModel
{
    private RectangleShape? _selectedRectangle;

    /// <summary>The radius of all four corners. Empty when the four differ. An entry sets all four radii.</summary>
    public NumberFieldViewModel LinkedRadius { get; }

    /// <summary>The radius of the top left corner.</summary>
    public NumberFieldViewModel TopLeftRadius { get; }

    /// <summary>The radius of the top right corner.</summary>
    public NumberFieldViewModel TopRightRadius { get; }

    /// <summary>The radius of the bottom right corner.</summary>
    public NumberFieldViewModel BottomRightRadius { get; }

    /// <summary>The radius of the bottom left corner.</summary>
    public NumberFieldViewModel BottomLeftRadius { get; }

    [ObservableProperty] public partial bool IsCornersExpanded { get; set; } = true;

    /// <summary>Whether exactly one rectangle (and nothing else) is selected.</summary>
    public bool HasRectangleSelected => _selectedRectangle is not null;

    /// <summary>Whether the Corners section is shown: the tool has it and one rectangle is selected.</summary>
    public bool ShowCorners => Sections.HasFlag(PropertiesSection.Corners) && HasRectangleSelected;

    /// <summary>
    /// Whether a change of one radius changes all four. Shared with the radius handles (a drag by touch) through the editor state, so
    /// the toggle here is the toggle there.
    /// </summary>
    public bool LinkCorners
    {
        get => _state.LinkCornerRadii;
        set => _state.LinkCornerRadii = value;
    }

    // A null corner is the linked field. With the toggle on, every field sets all four radii. With it off, a corner field sets one.
    private void SetRadius(Corner? corner, double value) =>
        _editor.Apply(
            HistoryStepType.ChangeCornerRadius,
            shape =>
            {
                if (shape is not RectangleShape rectangle)
                {
                    return shape;
                }

                var radius = Math.Clamp(value, 0, Math.Min(rectangle.Width, rectangle.Height) / 2);
                var onlyOne = corner is { } && !_state.LinkCornerRadii;
                return rectangle with { Radii = onlyOne ? rectangle.Radii.With(corner!.Value, radius) : CornerRadii.Uniform(radius) };
            });

    private void RefreshCornerValues()
    {
        _selectedRectangle = SelectedShapes() is [RectangleShape rectangle] ? rectangle : null;

        if (_selectedRectangle is { } shown)
        {
            var radii = shown.EffectiveRadii;
            LinkedRadius.SetValue(radii.TopLeft, mixed: !radii.IsUniform);
            TopLeftRadius.SetValue(radii.TopLeft);
            TopRightRadius.SetValue(radii.TopRight);
            BottomRightRadius.SetValue(radii.BottomRight);
            BottomLeftRadius.SetValue(radii.BottomLeft);
        }
        else
        {
            foreach (var field in new[] { LinkedRadius, TopLeftRadius, TopRightRadius, BottomRightRadius, BottomLeftRadius })
            {
                field.SetValue(null);
            }
        }

        OnPropertyChanged(nameof(HasRectangleSelected));
        OnPropertyChanged(nameof(ShowCorners));
    }
}
