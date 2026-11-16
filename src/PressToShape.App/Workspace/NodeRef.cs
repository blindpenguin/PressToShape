using System;

namespace PressToShape.App.Workspace;

/// <summary>One node of a shape: the shape and the index of the node in it. Used for the selected nodes of the node tools.</summary>
/// <param name="ShapeId">The id of the path, rectangle or ellipse.</param>
/// <param name="Index">The index of the node. A rectangle and an ellipse have the nodes 0 to 3.</param>
internal readonly record struct NodeRef(Guid ShapeId, int Index);
