using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using PressToShape.Core.Documents;

namespace PressToShape.App.Workspace;

// What was selected when a state of the document was left, so that undo and redo can bring the selection back after Group and Ungroup.
internal sealed partial class EditorState
{
    private readonly ConditionalWeakTable<Document, SelectionSnapshot> _selectionOf = new();

    private sealed record SelectionSnapshot(ImmutableArray<Guid> Ids);

    // Called before the document is replaced: the current selection belongs to the document that is being left.
    private void RememberSelection() => _selectionOf.AddOrUpdate(Document, new SelectionSnapshot(SelectedShapeIds));

    private void RestoreSelection(Document document)
    {
        if (_selectionOf.TryGetValue(document, out var snapshot))
        {
            SetSelection(snapshot.Ids);
        }
    }
}
