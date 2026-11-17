using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;

namespace PressToShape.Core.Documents;

/// <summary>
/// The path operations on a document: the boolean operations, Combine, Break Apart, and converting shapes to paths and reversing paths. Every method checks the shapes first and returns
/// a failure without changing anything if they cannot be used. The result is a new document, the input is never changed.
/// </summary>
public static class PathShapeOperations
{
    /// <summary>
    /// Replaces the shapes with one path that is the result of the operation. The operands are rectangles, ellipses and closed paths that
    /// are visible and not locked (a group, an open path or a hidden or locked shape makes the operation <see cref="PathOperationFailure.Invalid"/>),
    /// at least two. They are combined from the lowest in the document upward (see <see cref="BooleanEngine.Compute"/>). The result sits where the
    /// lowest operand was (same layer and group, same place in the stack) and takes its fill, stroke, opacity, name and flags. It has no rotation
    /// and the even-odd fill rule, so holes show. If nothing is left the failure is <see cref="PathOperationFailure.Empty"/>.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="ids">The ids of the operands.</param>
    /// <param name="type">The operation.</param>
    /// <param name="newId">The id of the result.</param>
    /// <param name="cancellationToken">Checked while the result is computed.</param>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    public static PathOperationResult Boolean(Document document, IReadOnlyList<Guid> ids, PathOperationType type, Guid newId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);

        if (Operands(document, ids, allowOpen: false) is not { } operands)
        {
            return PathOperationResult.Fail(PathOperationFailure.Invalid);
        }

        var outlines = operands.Select(operand => operand.Outline).ToList();
        var subPaths = BooleanEngine.Compute(type, outlines, cancellationToken);
        if (subPaths.Count == 0)
        {
            return PathOperationResult.Fail(PathOperationFailure.Empty);
        }

        return Replace(document, operands, subPaths, newId);
    }

    /// <summary>
    /// Replaces the shapes with one path that holds all their sub-paths unchanged, in document coordinates. The operands are rectangles,
    /// ellipses and paths (open ones too) that are visible and not locked, at least two. The result sits where the lowest operand was, takes
    /// its fill, stroke, opacity, name and flags, has no rotation and the even-odd fill rule, so a small circle inside a big one becomes a hole.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="ids">The ids of the operands.</param>
    /// <param name="newId">The id of the result.</param>
    public static PathOperationResult Combine(Document document, IReadOnlyList<Guid> ids, Guid newId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(ids);

        if (Operands(document, ids, allowOpen: true) is not { } operands)
        {
            return PathOperationResult.Fail(PathOperationFailure.Invalid);
        }

        var subPaths = operands.SelectMany(operand => operand.Outline.SubPaths).ToList();
        return subPaths.Count > PathShape.MaxSubPaths ? PathOperationResult.Fail(PathOperationFailure.Invalid) : Replace(document, operands, subPaths, newId);
    }

    /// <summary>
    /// Replaces a path that has several sub-paths with one path for each of them, in the same place, in the original order. Each keeps the
    /// appearance, name, flags, rotation and fill rule of the original and stays where it was on the screen. A hole becomes a separate filled path.
    /// A path with one sub-path, another type of shape, or a hidden or locked path is <see cref="PathOperationFailure.Invalid"/>.
    /// </summary>
    /// <param name="document">The document.</param>
    /// <param name="id">The id of the path.</param>
    /// <param name="newId">Makes the id of each new path.</param>
    public static PathOperationResult BreakApart(Document document, Guid id, Func<Guid> newId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(newId);

        if (ShapeTree.Find(document.Layers, id) is not { } found
            || found.Shape is not PathShape { SubPaths.Length: > 1 } path
            || !IsUsable(document, found))
        {
            return PathOperationResult.Fail(PathOperationFailure.Invalid);
        }

        var pieces = path.ToDocumentSubPaths()
            .Select(subPath => (Shape)(path.Rebuilt([subPath], path.FillRule) with { Id = newId() }))
            .ToList();
        var changed = ReplaceInPlace(document, found, pieces);
        return PathOperationResult.Success(changed, [.. pieces.Select(piece => piece.Id)]);
    }

    /// <summary>
    /// The ids of the shapes that <see cref="ConvertToPaths"/> would change: the rectangles and ellipses among the selected shapes and among the
    /// shapes inside selected groups (at any depth), each once, that are shown and not locked (the layer and the groups around them count).
    /// Ids that are not shapes of the document are ignored.
    /// </summary>
    public static IReadOnlyList<Guid> ConvertibleShapeIds(Document document, IEnumerable<Guid> selectedIds) =>
        [.. AffectedLeaves(document, selectedIds, IsConvertible).Select(shape => shape.Id)];

    /// <summary>
    /// Replaces the rectangles and ellipses found by <see cref="ConvertibleShapeIds"/> with the paths they would become
    /// (<see cref="ShapeConversion.ToPath"/>). They keep id, name, flags, box, rotation, appearance and place in the stack, so they look the same.
    /// Returns the same document if there is nothing to convert.
    /// </summary>
    public static Document ConvertToPaths(Document document, IEnumerable<Guid> selectedIds) =>
        Change(document, selectedIds, IsConvertible, shape => ShapeConversion.ToPath(shape)!);

    /// <summary>
    /// The ids of the shapes that <see cref="ReverseDirections"/> would change: the paths among the selected shapes and among the shapes inside
    /// selected groups (at any depth), each once, that are shown and not locked. Ids that are not shapes of the document are ignored.
    /// </summary>
    public static IReadOnlyList<Guid> ReversibleShapeIds(Document document, IEnumerable<Guid> selectedIds) =>
        [.. AffectedLeaves(document, selectedIds, IsPath).Select(shape => shape.Id)];

    /// <summary>
    /// Reverses the paths found by <see cref="ReversibleShapeIds"/> (see <see cref="PathReversal.Reverse"/>). Returns the same document if there is
    /// nothing to reverse.
    /// </summary>
    public static Document ReverseDirections(Document document, IEnumerable<Guid> selectedIds) =>
        Change(document, selectedIds, IsPath, shape => PathReversal.Reverse((PathShape)shape));

    private static bool IsConvertible(Shape shape) => shape is RectangleShape or EllipseShape;

    private static bool IsPath(Shape shape) => shape is PathShape;

    // The leaves of the selected shapes (a group gives its leaves), each once, that the test accepts and that are shown and not locked.
    private static List<Shape> AffectedLeaves(Document document, IEnumerable<Guid> selectedIds, Func<Shape, bool> accepts)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(selectedIds);

        var seen = new HashSet<Guid>();
        var affected = new List<Shape>();
        foreach (var id in selectedIds)
        {
            if (ShapeTree.Find(document.Layers, id) is not { } found)
            {
                continue;
            }

            foreach (var leaf in found.Shape.Leaves())
            {
                if (seen.Add(leaf.Id) && accepts(leaf) && document.FindShape(leaf.Id) is { IsEffectivelyVisible: true, IsEffectivelyLocked: false })
                {
                    affected.Add(leaf);
                }
            }
        }

        return affected;
    }

    private static Document Change(Document document, IEnumerable<Guid> selectedIds, Func<Shape, bool> accepts, Func<Shape, Shape> change)
    {
        var replacements = AffectedLeaves(document, selectedIds, accepts).ToDictionary(shape => shape.Id, change);
        return replacements.Count == 0 ? document : document.UpdateShapes(replacements);
    }

    private sealed record Operand(ShapeTree.Found Found, Outline Outline);

    // The operands from the lowest to the highest, or null if the shapes cannot be used.
    private static List<Operand>? Operands(Document document, IReadOnlyList<Guid> ids, bool allowOpen)
    {
        if (ids.Count < 2 || ids.Distinct().Count() != ids.Count)
        {
            return null;
        }

        var order = StackingOrder(document);
        var operands = new List<(int Order, Operand Operand)>(ids.Count);
        foreach (var id in ids)
        {
            if (ShapeTree.Find(document.Layers, id) is not { } found || !IsUsable(document, found))
            {
                return null;
            }

            var path = found.Shape as PathShape ?? ShapeConversion.ToPath(found.Shape);
            if (path is null || (!allowOpen && path.SubPaths.Any(subPath => !subPath.IsClosed)))
            {
                return null;
            }

            var onScreen = path.WithRotationApplied();
            operands.Add((order[id], new Operand(found, new Outline(onScreen.ToDocumentSubPaths(), onScreen.FillRule))));
        }

        return [.. operands.OrderBy(entry => entry.Order).Select(entry => entry.Operand)];
    }

    // Visible and not locked, counting the layer and the groups around the shape. A group is never usable.
    private static bool IsUsable(Document document, ShapeTree.Found found)
    {
        var (visible, locked) = ShapeTree.EffectiveState(found);
        var layer = document.Layers[found.LayerIndex];
        return found.Shape is not GroupShape && visible && !locked && layer.IsVisible && !layer.IsLocked;
    }

    // Where every shape is in the stack: layers from the bottom, shapes from the bottom, a group before its children.
    private static Dictionary<Guid, int> StackingOrder(Document document)
    {
        var order = new Dictionary<Guid, int>();
        void Visit(IEnumerable<Shape> shapes)
        {
            foreach (var shape in shapes)
            {
                order[shape.Id] = order.Count;
                if (shape is GroupShape group)
                {
                    Visit(group.Children);
                }
            }
        }

        foreach (var layer in document.Layers)
        {
            Visit(layer.Shapes);
        }

        return order;
    }

    // The new path takes the look of the lowest operand and its place. The other operands go.
    private static PathOperationResult Replace(Document document, List<Operand> operands, IReadOnlyList<DocumentSubPath> subPaths, Guid newId)
    {
        var bottom = operands[0].Found.Shape;
        var result = PathShape.Create(subPaths, FillRule.EvenOdd) with
        {
            Id = newId,
            Appearance = bottom.Appearance,
            Name = bottom.Name,
            IsVisible = bottom.IsVisible,
            IsLocked = bottom.IsLocked,
        };

        var changed = ReplaceInPlace(document, operands[0].Found, [result]);
        var others = operands.Skip(1).Select(operand => operand.Found.Shape.Id).ToHashSet();
        return PathOperationResult.Success(changed.RemoveShapes(others), [result.Id]);
    }

    // The shape is replaced by the shapes, at its place among its siblings.
    private static Document ReplaceInPlace(Document document, ShapeTree.Found found, IReadOnlyList<Shape> replacements)
    {
        var layer = document.Layers[found.LayerIndex];
        var siblings = found.Parent is null ? layer.Shapes : found.Parent.Children;
        var index = siblings.IndexOf(found.Shape);
        var result = ImmutableArray.CreateBuilder<Shape>(siblings.Length - 1 + replacements.Count);
        result.AddRange(siblings.Take(index));
        result.AddRange(replacements);
        result.AddRange(siblings.Skip(index + 1));
        return GroupOperations.WithSiblings(document, found.LayerIndex, found.Parent, result.ToImmutable());
    }
}
