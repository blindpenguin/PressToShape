using System;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>What leaving the editing of a text does to the document.</summary>
internal enum TextEditOutcome
{
    /// <summary>Nothing: an existing text is as it was, or a new text stayed empty.</summary>
    None,

    /// <summary>A new text with content is added. The text was not in the document while it was edited.</summary>
    Create,

    /// <summary>The content of an existing text changed.</summary>
    Update,

    /// <summary>An existing text became empty, so it is removed.</summary>
    Delete,
}

/// <summary>
/// The editing of one text. It keeps the text that is edited with its draft content, and decides at the end what happens to the document, so
/// the whole editing is one history step. The document is not touched while typing.
/// </summary>
internal sealed class TextEditSession
{
    private readonly string _original;

    /// <summary>Starts editing.</summary>
    /// <param name="text">The text. For a new text it is not in the document yet.</param>
    /// <param name="isNew">Whether the text was created by this session.</param>
    public TextEditSession(TextShape text, bool isNew)
    {
        ArgumentNullException.ThrowIfNull(text);

        Text = text;
        IsNew = isNew;
        _original = text.Text;
    }

    /// <summary>The text with its draft content.</summary>
    public TextShape Text { get; private set; }

    /// <summary>Whether the text was created by this session and is not in the document yet.</summary>
    public bool IsNew { get; }

    /// <summary>The id of the text.</summary>
    public Guid TextId => Text.Id;

    /// <summary>Replaces the draft content.</summary>
    public void SetContent(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        Text = Text with { Text = content };
    }

    /// <summary>Replaces the draft with a text of another size or box, for example after it was measured. The content is kept.</summary>
    public void SetBox(double x, double y, double width, double height) =>
        Text = Text with { X = x, Y = y, Width = width, Height = height };

    /// <summary>What ending the editing now does to the document.</summary>
    public TextEditOutcome Outcome
    {
        get
        {
            if (Text.Text.Length == 0)
            {
                return IsNew ? TextEditOutcome.None : TextEditOutcome.Delete;
            }

            return IsNew ? TextEditOutcome.Create : Text.Text == _original ? TextEditOutcome.None : TextEditOutcome.Update;
        }
    }
}
