using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

internal sealed partial class EditorState
{
    /// <summary>
    /// The formatting of the next text that is created. Editor state, not part of the document: it is not saved and not undone, and a new or
    /// opened document keeps it.
    /// </summary>
    [ObservableProperty] public partial TextDefaults TextDefaults { get; set; } = TextDefaults.Initial;

    /// <summary>
    /// The text that is being edited on the canvas, or <c>null</c>. Not part of the document: the document changes when the editing ends
    /// (see <see cref="TextEditSession"/>).
    /// </summary>
    [ObservableProperty] public partial TextEditSession? TextEdit { get; set; }

    /// <summary>Remembers the values of a format change for the next text. Call it when the user changes the formatting.</summary>
    public void RememberTextFormat(TextFormatChange change)
    {
        System.ArgumentNullException.ThrowIfNull(change);

        TextDefaults = TextDefaults.With(change);
    }
}
