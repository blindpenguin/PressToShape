using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using static PressToShape.App.Localization.Localizer;
using Color = PressToShape.Core.Shapes.Color;

namespace PressToShape.App.ViewModels.Tablet;

/// <summary>
/// The small color swatch at the bottom of the tablet tool column: the fill in the middle and the stroke as a ring around it. It shows what the
/// Properties panel shows (the color of the selection, or of the next shape when nothing is selected) by reading the fill and stroke fields of the
/// <see cref="PropertiesViewModel"/>, so there is one source of truth. A tap opens the Properties panel at its Appearance section, where the colors are
/// edited.
/// </summary>
internal sealed partial class TabletColorSwatchViewModel : LocalizedViewModel
{
    private readonly PropertiesViewModel _properties;
    private readonly TabletPanelViewModel _panel;

    public TabletColorSwatchViewModel(PropertiesViewModel properties, TabletPanelViewModel panel, ILocalizer localizer)
        : base(localizer)
    {
        _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        _panel = panel ?? throw new ArgumentNullException(nameof(panel));

        // The swatch lives as long as the shell, so the subscriptions are never removed.
        _properties.Fill.PropertyChanged += OnFillChanged;
        _properties.Stroke.PropertyChanged += OnStrokeChanged;
    }

    /// <summary>The fill color. Only meaningful while <see cref="HasFill"/> is true.</summary>
    public Color FillColor => _properties.Fill.Color;

    /// <summary>Whether the fill has a color. False for no fill and for a selection with different fills.</summary>
    public bool HasFill => _properties.Fill.HasColor;

    /// <summary>The stroke color. Only meaningful while <see cref="HasStroke"/> is true.</summary>
    public Color StrokeColor => _properties.Stroke.Color;

    /// <summary>Whether the stroke has a color. False for no stroke and for a selection with different strokes.</summary>
    public bool HasStroke => _properties.Stroke.HasColor;

    /// <summary>The translated name, used as the tooltip and as the accessible name.</summary>
    public string Name => Localizer.T(N("Fill and stroke color"));

    /// <summary>A tap: opens the Properties panel (or shows it if another tab is open) and expands the Appearance section.</summary>
    [RelayCommand]
    private void Open()
    {
        _properties.IsAppearanceExpanded = true;
        _panel.Open(InspectorTab.Properties);
    }

    private void OnFillChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ColorFieldViewModel.Color):
                OnPropertyChanged(nameof(FillColor));
                break;
            case nameof(ColorFieldViewModel.HasColor):
                OnPropertyChanged(nameof(HasFill));
                break;
        }
    }

    private void OnStrokeChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ColorFieldViewModel.Color):
                OnPropertyChanged(nameof(StrokeColor));
                break;
            case nameof(ColorFieldViewModel.HasColor):
                OnPropertyChanged(nameof(HasStroke));
                break;
        }
    }
}
