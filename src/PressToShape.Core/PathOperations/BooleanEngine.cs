using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Clipper2Lib;
using PressToShape.Core.Geometry;
using PressToShape.Core.Shapes;
using ClipperFillRule = Clipper2Lib.FillRule;
using FillRule = PressToShape.Core.Shapes.FillRule;

namespace PressToShape.Core.PathOperations;

/// <summary>
/// Combines the outlines of shapes with a boolean operation. The curves are flattened, the polygons are clipped with the Clipper2 package,
/// and the result is turned back into nodes (see <see cref="CurveFitting"/>), keeping unchanged pieces of the original curves. This is the only class that uses Clipper2, and its interface is plain geometry, so other
/// features can use it as well. The same input always gives the same result.
/// </summary>
public static class BooleanEngine
{
    /// <summary>
    /// The total node count of the operands above which a caller should run the operation off the UI thread. From the benchmark
    /// <c>PathOperationBenchmarks</c> (spec boolean-operations, task 19): a union of two wavy outlines took 72 ms at 40,000 nodes in total and
    /// 1,030 ms at 200,000, so it reaches about 200 ms, the budget of the requirements, at about 70,000.
    /// </summary>
    public const int BackgroundNodeLimit = 70_000;

    /// <summary>How far the flattened polygons may be from the curves, in document units.</summary>
    public const double Tolerance = 0.1;

    // The number of decimals Clipper2 keeps. Far below the tolerance.
    private const int Precision = 4;

    /// <summary>
    /// Combines the operands from the bottom one up: the result so far is combined with the next operand, so a difference cuts every
    /// upper operand out of the bottom one, and an exclusion keeps the areas that an odd number of operands cover.
    /// </summary>
    /// <param name="type">The operation.</param>
    /// <param name="operands">The outlines, bottom first, at least two.</param>
    /// <param name="cancellationToken">Checked between the operands.</param>
    /// <returns>The sub-paths of the result in document coordinates, all closed. Empty if the result covers no area.</returns>
    /// <exception cref="ArgumentException">There are fewer than two operands, or a number is not finite.</exception>
    /// <exception cref="OperationCanceledException">The token was cancelled.</exception>
    public static IReadOnlyList<DocumentSubPath> Compute(PathOperationType type, IReadOnlyList<Outline> operands, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operands);
        if (operands.Count < 2)
        {
            throw new ArgumentException("A boolean operation needs at least two operands.", nameof(operands));
        }

        var sources = new SourceCurves();
        PathsD? result = null;
        for (var i = 0; i < operands.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var polygons = Normalized(Flatten(operands[i], i, sources), operands[i].FillRule);
            result = result is null ? polygons : Combine(type, result, polygons);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return result!.Where(polygon => polygon.Count >= 3).Select(polygon => CurveFitting.Fit([.. polygon.Select(point => (point.x, point.y))], sources)).ToList();
    }

    // Every sub-path of the operand as a polygon. An open sub-path is closed by the polygon itself.
    private static PathsD Flatten(Outline outline, int operand, SourceCurves sources)
    {
        ArgumentNullException.ThrowIfNull(outline);

        var polygons = new PathsD(outline.SubPaths.Count);
        var firstSegment = 0;
        foreach (var subPath in outline.SubPaths)
        {
            var segments = PathShape.BuildSegments(subPath.Nodes, subPath.IsClosed);
            var points = Flattening.Flatten(segments, subPath.IsClosed, operand, firstSegment, Tolerance);
            polygons.Add(new PathD(points.Select(point => new PointD(point.X, point.Y))));
            sources.Add(operand, firstSegment, segments, points);
            firstSegment += segments.Count;
        }

        return polygons;
    }

    // Each operand is first cleaned with its own fill rule, so the fold can use one rule for everything.
    private static PathsD Normalized(PathsD polygons, FillRule fillRule) =>
        Clipper.Union(polygons, new PathsD(), fillRule == FillRule.EvenOdd ? ClipperFillRule.EvenOdd : ClipperFillRule.NonZero, Precision);

    private static PathsD Combine(PathOperationType type, PathsD accumulated, PathsD next) => type switch
    {
        PathOperationType.Union => Clipper.Union(accumulated, next, ClipperFillRule.NonZero, Precision),
        PathOperationType.Difference => Clipper.Difference(accumulated, next, ClipperFillRule.NonZero, Precision),
        PathOperationType.Intersection => Clipper.Intersect(accumulated, next, ClipperFillRule.NonZero, Precision),
        PathOperationType.Exclusion => Clipper.Xor(accumulated, next, ClipperFillRule.NonZero, Precision),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "The operation is not defined."),
    };
}
