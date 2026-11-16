using System;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Workspace;

/// <summary>What is under the pointer for the node tools. The default value is a miss.</summary>
/// <param name="Type">What was hit.</param>
/// <param name="ShapeId">The shape of the node, handle or segment.</param>
/// <param name="Index">The index of the node or handle, or of the segment (the node it starts at).</param>
/// <param name="Side">For a handle: which one.</param>
/// <param name="T">For a segment: the parameter of the point of the curve that is nearest to the pointer, strictly between 0 and 1.</param>
internal readonly record struct NodeHit(NodeHitType Type, Guid ShapeId = default, int Index = -1, NodeHandleSide Side = NodeHandleSide.In, double T = 0);
