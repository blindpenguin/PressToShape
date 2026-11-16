using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;

namespace PressToShape.App.ViewModels.Editor;

// The Node section of the Properties tab: the position of a selected node, and for the Node tool the node type and segment buttons.
// The edits are made by NodeEditor, which makes one history step of each.
internal sealed partial class PropertiesViewModel
{
    private NodeEditor? _nodeEditor;

    private NodeEditor Nodes => _nodeEditor ??= new NodeEditor(_state);

    /// <summary>Whether the Node section is expanded. Part of the view, not of the document.</summary>
    [ObservableProperty] public partial bool IsNodeExpanded { get; set; } = true;

    /// <summary>The X of the only selected node on the screen, in document units. Disabled without exactly one selected node.</summary>
    public NumberFieldViewModel NodeX { get; }

    /// <summary>The Y of the only selected node on the screen, in document units. Disabled without exactly one selected node.</summary>
    public NumberFieldViewModel NodeY { get; }

    /// <summary>Whether the Node section is shown: for Direct Select and the Node tool.</summary>
    public bool ShowNode => Sections.HasFlag(PropertiesSection.Node);

    /// <summary>Whether the buttons for the node type and the segments are shown: only the Node tool changes them.</summary>
    public bool ShowNodeTypeTools => _state.ActiveTool == Tool.Node;

    /// <summary>Whether at least one node that can be edited is selected.</summary>
    public bool HasNodesSelected => Nodes.HasEditableNodes;

    /// <summary>Whether every selected node is a corner.</summary>
    public bool IsNodeCorner => Nodes.TypeOfSelection == NodeType.Corner;

    /// <summary>Whether every selected node is smooth.</summary>
    public bool IsNodeSmooth => Nodes.TypeOfSelection == NodeType.Smooth;

    /// <summary>Whether every selected node is symmetric.</summary>
    public bool IsNodeSymmetric => Nodes.TypeOfSelection == NodeType.Symmetric;

    /// <summary>Makes the selected nodes corners, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasNodesSelected))]
    private void MakeNodeCorner() => Nodes.SetType(NodeType.Corner);

    /// <summary>Makes the selected nodes smooth, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasNodesSelected))]
    private void MakeNodeSmooth() => Nodes.SetType(NodeType.Smooth);

    /// <summary>Makes the selected nodes symmetric, as one step.</summary>
    [RelayCommand(CanExecute = nameof(HasNodesSelected))]
    private void MakeNodeSymmetric() => Nodes.SetType(NodeType.Symmetric);

    /// <summary>Makes the segment between the two selected neighboring nodes straight, as one step.</summary>
    [RelayCommand(CanExecute = nameof(CanChangeSegment))]
    private void MakeSegmentStraight() => Nodes.StraightenSelectedSegment();

    /// <summary>Makes the straight segment between the two selected neighboring nodes curved, as one step.</summary>
    [RelayCommand(CanExecute = nameof(CanChangeSegment))]
    private void MakeSegmentCurved() => Nodes.CurveSelectedSegment();

    /// <summary>Whether exactly two neighboring nodes of one path are selected.</summary>
    public bool CanChangeSegment => Nodes.CanChangeSegment;

    private void RefreshNodeValues()
    {
        if (Nodes.PositionOfSingleNode is { } position)
        {
            NodeX.SetValue(position.X);
            NodeY.SetValue(position.Y);
        }
        else
        {
            NodeX.SetValue(null);
            NodeY.SetValue(null);
        }

        OnPropertyChanged(nameof(HasNodesSelected));
        OnPropertyChanged(nameof(IsNodeCorner));
        OnPropertyChanged(nameof(IsNodeSmooth));
        OnPropertyChanged(nameof(IsNodeSymmetric));
        OnPropertyChanged(nameof(CanChangeSegment));
        MakeNodeCornerCommand.NotifyCanExecuteChanged();
        MakeNodeSmoothCommand.NotifyCanExecuteChanged();
        MakeNodeSymmetricCommand.NotifyCanExecuteChanged();
        MakeSegmentStraightCommand.NotifyCanExecuteChanged();
        MakeSegmentCurvedCommand.NotifyCanExecuteChanged();
    }
}
