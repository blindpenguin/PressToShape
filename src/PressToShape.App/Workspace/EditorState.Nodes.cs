using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

internal sealed partial class EditorState
{
    /// <summary>
    /// The nodes that are selected with Direct Select or the Node tool. Part of the editor, not of the document: it is not saved and not
    /// undone. It is cleared when the shape selection or the tool changes and when a document is opened or created. Changes of the document
    /// (an undo, for example) drop the references that no longer fit.
    /// </summary>
    [ObservableProperty] public partial ImmutableHashSet<NodeRef> SelectedNodes { get; private set; } = ImmutableHashSet<NodeRef>.Empty;

    /// <summary>Whether at least one node is selected.</summary>
    public bool HasSelectedNodes => !SelectedNodes.IsEmpty;

    /// <summary>Replaces the selected nodes. Repeated nodes count once.</summary>
    public void SetNodeSelection(IEnumerable<NodeRef> nodes)
    {
        System.ArgumentNullException.ThrowIfNull(nodes);

        SetNodes(nodes.ToImmutableHashSet());
    }

    /// <summary>Adds the node to the selected nodes, or removes it if it is selected.</summary>
    public void ToggleNode(NodeRef node) =>
        SetNodes(SelectedNodes.Contains(node) ? SelectedNodes.Remove(node) : SelectedNodes.Add(node));

    /// <summary>Clears the selected nodes.</summary>
    public void ClearNodeSelection() => SetNodes(ImmutableHashSet<NodeRef>.Empty);

    private void SetNodes(ImmutableHashSet<NodeRef> nodes)
    {
        if (SelectedNodes.SetEquals(nodes))
        {
            return;
        }

        SelectedNodes = nodes;
        OnPropertyChanged(nameof(HasSelectedNodes));
    }

    // After a change of the document: a reference to a shape that is gone, or to a node that no longer exists, is dropped.
    private void PruneNodeSelection(Document document)
    {
        if (SelectedNodes.IsEmpty)
        {
            return;
        }

        var kept = SelectedNodes.Where(node => node.Index >= 0 && node.Index < NodeCountOf(document, node.ShapeId)).ToImmutableHashSet();
        SetNodes(kept);
    }

    // A path has its nodes, a rectangle and an ellipse four (their corners are shown as nodes), everything else none.
    private static int NodeCountOf(Document document, System.Guid shapeId) => document.FindShape(shapeId)?.Shape switch
    {
        PathShape path => path.Nodes.Length,
        RectangleShape or EllipseShape => 4,
        _ => 0,
    };
}
