using System;
using System.Collections.Immutable;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

// The entered group: the group whose children a click selects, until it is left.
internal sealed partial class EditorState
{
    /// <summary>
    /// The group that was entered, or <c>null</c>. While it is set, a click selects the children of this group, and the rest of the
    /// drawing cannot be selected. The state clears it when the group is gone, hidden or locked. Not part of the document.
    /// </summary>
    [ObservableProperty] public partial Guid? EnteredGroupId { get; private set; }

    /// <summary>
    /// Enters the group. Of the selection only the direct children of the group stay selected. Does nothing if the id is not a group that
    /// can be selected.
    /// </summary>
    /// <returns>Whether the group was entered.</returns>
    public bool EnterGroup(Guid groupId)
    {
        if (Document.FindShape(groupId) is not { Shape: GroupShape group } || !SelectionQueries.IsSelectable(Document, groupId))
        {
            return false;
        }

        EnteredGroupId = groupId;
        var children = group.Children.Select(child => child.Id).ToHashSet();
        SetSelectionCore(SelectedShapeIds.Where(children.Contains).ToImmutableArray());
        return true;
    }

    /// <summary>
    /// Leaves the entered group by one level and selects it. If that group is inside another group, the other group is entered. Does
    /// nothing when no group is entered.
    /// </summary>
    public void LeaveGroup()
    {
        if (EnteredGroupId is not { } leaving)
        {
            return;
        }

        EnteredGroupId = Document.FindShape(leaving)?.Parent?.Id;
        SetSelection([leaving]);
    }

    /// <summary>Leaves every entered group at once, so the top level of the drawing is selectable again. The selection stays.</summary>
    public void LeaveAllGroups() => EnteredGroupId = null;

    private void LeaveEnteredGroupIfGone(Document document)
    {
        if (EnteredGroupId is { } entered
            && !(document.FindShape(entered) is { Shape: GroupShape } && SelectionQueries.IsSelectable(document, entered)))
        {
            EnteredGroupId = null;
        }
    }
}
