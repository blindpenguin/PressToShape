using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// The swatches saved with the document, shown in the Appearance section. Adding and removing a swatch is one undo step.
/// Applying a swatch sets the fill or the stroke like a color chosen in the picker.
/// </summary>
internal sealed class PaletteViewModel : ViewModelBase
{
    private readonly EditorState _state;
    private readonly ColorFieldViewModel _fill;
    private readonly ColorFieldViewModel _stroke;
    private ImmutableArray<Color> _shown;

    public PaletteViewModel(EditorState state, ColorFieldViewModel fill, ColorFieldViewModel stroke)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _fill = fill ?? throw new ArgumentNullException(nameof(fill));
        _stroke = stroke ?? throw new ArgumentNullException(nameof(stroke));

        _shown = _state.Document.Palette;
        _state.PropertyChanged += OnStateChanged;
    }

    /// <summary>The swatches of the current document, in order.</summary>
    public IReadOnlyList<Color> Swatches => _state.Document.Palette;

    /// <summary>Whether the palette has no swatches, for the hint that replaces the list.</summary>
    public bool IsEmpty => _state.Document.Palette.IsEmpty;

    /// <summary>Adds the color at the end of the palette. Does nothing if the palette has it.</summary>
    public void AddSwatch(Color color) => _state.Edit(HistoryStepType.AddSwatch, document => document.AddSwatch(color));

    /// <summary>Removes the swatch at this index. Does nothing for an index outside the palette.</summary>
    public void RemoveSwatch(int index)
    {
        if (index >= 0 && index < _state.Document.Palette.Length)
        {
            _state.Edit(HistoryStepType.RemoveSwatch, document => document.RemoveSwatchAt(index));
        }
    }

    /// <summary>Removes the swatch with this color. Does nothing if the palette does not have it.</summary>
    public void RemoveSwatch(Color color)
    {
        for (var i = 0; i < _state.Document.Palette.Length; i++)
        {
            if (_state.Document.Palette[i] == color)
            {
                RemoveSwatch(i);
                return;
            }
        }
    }

    /// <summary>Sets the fill (or the stroke) of the selection to the color. Without a selection it sets the default for new shapes.</summary>
    public void Apply(Color color, bool toStroke) => (toStroke ? _stroke : _fill).SetColor(color);

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Most document changes (a drag, a color) leave the palette array as it was, so the swatch list is not rebuilt for them.
        if (e.PropertyName == nameof(EditorState.Document) && _state.Document.Palette != _shown)
        {
            _shown = _state.Document.Palette;
            OnPropertyChanged(nameof(Swatches));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}
