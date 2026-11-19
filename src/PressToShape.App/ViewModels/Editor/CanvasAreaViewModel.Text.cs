using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

// The text that is edited on the canvas: what the editor control above the canvas shows and what it reports back.
internal sealed partial class CanvasAreaViewModel
{
    /// <summary>Whether a text is being edited on the canvas.</summary>
    public bool IsEditingText => _state.TextEdit is not null;

    /// <summary>
    /// The content of the text that is being edited, empty when none is. The editor sets it while the user types. The document does not change
    /// until the editing ends (see <see cref="CommitTextEditCommand"/>), and a point text gets the measured size of its content meanwhile.
    /// </summary>
    public string EditedText
    {
        get => _state.TextEdit?.Text.Text ?? string.Empty;
        set
        {
            if (_state.TextEdit is not { } session || session.Text.Text == value)
            {
                return;
            }

            session.SetContent(value);
            if (TextMeasurer is { } measurer)
            {
                var fitted = TextOperations.Fit(session.Text, measurer.Layout(session.Text));
                session.SetBox(fitted.X, fitted.Y, fitted.Width, fitted.Height);
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(EditedTextShape));
        }
    }

    /// <summary>The text that is being edited, in screen space with its font and its draft content, or <c>null</c> when none is.</summary>
    public ScreenShape? EditedTextShape =>
        _state.TextEdit is { } session ? CanvasScene.BuildShape(session.Text, ZoomPercent, OriginX, OriginY, TextMeasurer) : null;

    /// <summary>
    /// The height of a line of the edited text in screen pixels, with its line spacing. <c>NaN</c> when nothing is edited or there is no
    /// measurer, which leaves the line height to the editor.
    /// </summary>
    public double EditedTextLineHeight =>
        _state.TextEdit is { } session && TextMeasurer is { } measurer
            ? measurer.Layout(session.Text with { Text = "x", IsArea = false }).Height * ZoomPercent / 100
            : double.NaN;

    /// <summary>Ends the editing of the text: the result goes into the document as one step. Does nothing when no text is edited.</summary>
    [RelayCommand]
    private void CommitTextEdit() => _interaction.CommitTextEdit();

    private void NotifyEditedTextChanged()
    {
        OnPropertyChanged(nameof(IsEditingText));
        OnPropertyChanged(nameof(EditedText));
        OnPropertyChanged(nameof(EditedTextShape));
        OnPropertyChanged(nameof(EditedTextLineHeight));
    }
}
