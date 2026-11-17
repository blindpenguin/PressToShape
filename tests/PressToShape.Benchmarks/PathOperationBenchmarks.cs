using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using PressToShape.Core.PathOperations;
using PressToShape.Core.Shapes;

namespace PressToShape.Benchmarks;

/// <summary>
/// How long a boolean operation takes for two overlapping outlines with the same number of curved nodes each. The 200 node case is the
/// budget of the requirements (under 200 ms); the larger sizes show where the work should leave the UI thread
/// (<see cref="BooleanEngine.BackgroundNodeLimit"/>). Total nodes are twice the parameter.
/// </summary>
[MemoryDiagnoser]
public class PathOperationBenchmarks
{
    private Outline _first = null!;
    private Outline _second = null!;

    [Params(200, 5_000, 20_000, 100_000)]
    public int NodesPerOperand { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _first = Wavy(0, 0);
        _second = Wavy(60, 40);

        // A benchmark of an operation that returns nothing would measure nothing.
        if (BooleanEngine.Compute(PathOperationType.Union, [_first, _second]).Count == 0)
        {
            throw new InvalidOperationException("The union of the two outlines is empty.");
        }
    }

    [Benchmark(Description = "Union")]
    public int Union() => BooleanEngine.Compute(PathOperationType.Union, [_first, _second]).Count;

    [Benchmark(Description = "Difference")]
    public int Difference() => BooleanEngine.Compute(PathOperationType.Difference, [_first, _second]).Count;

    // A closed outline of smooth curved nodes around a wavy circle of radius about 100, so the operands cross many times.
    private Outline Wavy(double centerX, double centerY)
    {
        var points = Enumerable.Range(0, NodesPerOperand).Select(i =>
        {
            var angle = 2 * Math.PI * i / NodesPerOperand;
            var radius = 100 * (1 + 0.15 * Math.Sin(9 * angle));
            return (X: centerX + radius * Math.Cos(angle), Y: centerY + radius * Math.Sin(angle));
        }).ToList();

        var nodes = new List<DocumentNode>(points.Count);
        for (var i = 0; i < points.Count; i++)
        {
            var previous = points[(i - 1 + points.Count) % points.Count];
            var next = points[(i + 1) % points.Count];
            var point = points[i];
            var dx = (next.X - previous.X) / 6;
            var dy = (next.Y - previous.Y) / 6;
            nodes.Add(new DocumentNode(point.X, point.Y, point.X - dx, point.Y - dy, point.X + dx, point.Y + dy, NodeType.Smooth));
        }

        return new Outline([new DocumentSubPath(nodes, isClosed: true)], FillRule.NonZero);
    }
}
