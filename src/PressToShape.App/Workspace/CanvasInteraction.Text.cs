using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

internal sealed partial class CanvasInteraction
{
    /// <summary>Whether a text is being edited on the canvas.</summary>
    public bool IsEditingText => _state.TextEdit is not null;

    // A press with the Text tool on a text edits it, on anything else it starts a new text: a click gives point text, a drag gives area text.
    // A locked text is not edited and nothing is created over it.
    private void PressWithTextTool(double x, double y)
    {
        var document = _state.Document;
        if (document.HitTest(x, y).FirstOrDefault() is TextShape hit)
        {
            if (document.FindShape(hit.Id) is { IsEffectivelyLocked: false })
            {
                BeginTextEdit(hit, isNew: false);
            }

            return;
        }

        if (!CanDrawInActiveLayer())
        {
            return;
        }

        BeginSnapSession([]);
        var (startX, startY) = SnappedPoint(x, y);
        BeginDrag(DragType.CreatingText, startX, startY);
    }

    private SelectionBox TextBoxTo(double x, double y)
    {
        var (left, top, width, height) = DragBox(x, y, shift: false, alt: false);
        return new SelectionBox(left, top, width, height);
    }

    // Like the marquee: a box that stays tiny in both directions is a click.
    private void EndTextDrag(double x, double y)
    {
        // The box is made before the drag ends, because ending it drops the snap targets.
        var (left, top, width, height) = DragBox(x, y, shift: false, alt: false);
        EndDrag();
        if (!CanDrawInActiveLayer())
        {
            return;
        }

        var scale = _state.ZoomPercent / 100;
        var isArea = width * scale >= ClickPixels || height * scale >= ClickPixels;
        var appearance = _state.DefaultAppearance with { Stroke = null };
        var text = isArea
            ? _state.TextDefaults.NewText(left, top, width, height, isArea: true, string.Empty, appearance)
            : _state.TextDefaults.NewText(_startX, _startY, 0, 0, isArea: false, string.Empty, appearance);
        BeginTextEdit(text, isNew: true);
    }

    private void BeginTextEdit(TextShape text, bool isNew)
    {
        _state.TextEdit = new TextEditSession(text, isNew);
        if (isNew)
        {
            _state.ClearSelection();
        }
        else
        {
            _state.SelectOnly(text.Id);
        }
    }

    /// <summary>
    /// Ends the editing of the text, if one is edited, and puts the result into the document as one step: a new text is created (and selected),
    /// the content of an existing one is replaced, an emptied text is deleted, an unchanged one stays as it is. A text that stays empty
    /// leaves nothing behind. Does nothing when no text is edited.
    /// </summary>
    public void CommitTextEdit()
    {
        if (_state.TextEdit is not { } session)
        {
            return;
        }

        _state.TextEdit = null;
        var text = session.Text;
        switch (session.Outcome)
        {
            case TextEditOutcome.Create:
                if (CanDrawInActiveLayer())
                {
                    _state.Edit(HistoryStepType.CreateText, document => WithMeasuredBox(TextOperations.Create(document, _state.ActiveLayerId, text), text));
                    _state.Select(text.Id);
                }

                break;
            case TextEditOutcome.Update:
                _state.Edit(HistoryStepType.EditText, document => WithMeasuredBox(TextOperations.SetText(document, text.Id, text.Text), text));
                break;
            case TextEditOutcome.Delete:
                _state.Edit(HistoryStepType.DeleteShape, document => document.RemoveShapes(new HashSet<Guid> { text.Id }));
                _state.ClearSelection();
                break;
        }
    }

    // After a resize the box of a point text follows its font size, which may not be exactly the box that was scaled. The measured size is
    // taken around the same center, so the text does not move.
    private IReadOnlyList<Shape> FitPointTexts(IReadOnlyList<Shape> shapes)
    {
        if (_textMeasurer is null || !SelectionTransforms.HasPointText(shapes))
        {
            return shapes;
        }

        return [.. shapes.Select(shape =>
        {
            if (shape is not TextShape { IsArea: false } text)
            {
                return shape;
            }

            var size = _textMeasurer.Layout(text);
            return text with { X = text.CenterX - size.Width / 2, Y = text.CenterY - size.Height / 2, Width = size.Width, Height = size.Height };
        })];
    }

    // A point text has no box of its own: it is the measured size of its content.
    private Document WithMeasuredBox(Document document, TextShape text) =>
        _textMeasurer is null ? document : TextOperations.ApplyLayout(document, text.Id, _textMeasurer.Layout(text));

    // Choosing another tool ends the editing. It ends without a result when the text it edits is gone, for example after Undo.
    private void TextStateChanged(PropertyChangedEventArgs e)
    {
        if (_state.TextEdit is not { } session)
        {
            return;
        }

        if (e.PropertyName == nameof(EditorState.ActiveTool))
        {
            CommitTextEdit();
        }
        else if (e.PropertyName == nameof(EditorState.Document) && !session.IsNew && _state.Document.FindShape(session.TextId)?.Shape is not TextShape)
        {
            _state.TextEdit = null;
        }
    }
}
