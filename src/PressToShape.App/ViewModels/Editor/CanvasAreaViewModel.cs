using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// Geometry of the canvas area: where the artboard is drawn and where the ruler zero points are.
/// The view only reports the viewport size, everything else is derived here.
/// </summary>
internal sealed partial class CanvasAreaViewModel : ViewModelBase
{
    private readonly EditorState _state;

    public CanvasAreaViewModel(EditorState state)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _state.PropertyChanged += OnStateChanged;
    }

    /// <summary>Width of the area that shows the artboard, in screen pixels. Set by the view.</summary>
    [ObservableProperty] public partial double ViewportWidth { get; set; }

    /// <summary>Height of the area that shows the artboard, in screen pixels. Set by the view.</summary>
    [ObservableProperty] public partial double ViewportHeight { get; set; }

    /// <summary>The document the canvas draws.</summary>
    public Document Document => _state.Document;

    public double ZoomPercent => _state.ZoomPercent;

    public double ArtboardWidth => _state.ArtboardWidth;

    public double ArtboardHeight => _state.ArtboardHeight;

    public double ArtboardScreenWidth => _state.ArtboardWidth * _state.ZoomPercent / 100;

    public double ArtboardScreenHeight => _state.ArtboardHeight * _state.ZoomPercent / 100;

    /// <summary>Horizontal screen position of the artboard's left edge. The horizontal ruler's zero is here.</summary>
    public double OriginX => Math.Max(0, (ViewportWidth - ArtboardScreenWidth) / 2);

    /// <summary>Vertical screen position of the artboard's top edge. The vertical ruler's zero is here.</summary>
    public double OriginY => Math.Max(0, (ViewportHeight - ArtboardScreenHeight) / 2);

    partial void OnViewportWidthChanged(double value) => OnPropertyChanged(nameof(OriginX));

    partial void OnViewportHeightChanged(double value) => OnPropertyChanged(nameof(OriginY));

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.Document):
                OnPropertyChanged(nameof(Document));
                break;
            case nameof(EditorState.ZoomPercent):
                OnPropertyChanged(nameof(ZoomPercent));
                NotifyArtboardGeometryChanged();
                break;
            case nameof(EditorState.ArtboardWidth):
            case nameof(EditorState.ArtboardHeight):
                OnPropertyChanged(nameof(ArtboardWidth));
                OnPropertyChanged(nameof(ArtboardHeight));
                NotifyArtboardGeometryChanged();
                break;
        }
    }

    private void NotifyArtboardGeometryChanged()
    {
        OnPropertyChanged(nameof(ArtboardScreenWidth));
        OnPropertyChanged(nameof(ArtboardScreenHeight));
        OnPropertyChanged(nameof(OriginX));
        OnPropertyChanged(nameof(OriginY));
    }
}
