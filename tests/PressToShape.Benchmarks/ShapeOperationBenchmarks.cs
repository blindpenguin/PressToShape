using PressToShape.Core.Tests;
using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.IO;
using PressToShape.Core.Shapes;

namespace PressToShape.Benchmarks;

/// <summary>
/// How long the shape operations take on a large drawing: a grid of rectangles and ellipses on one layer, all of them selected.
/// Every benchmark leaves the state as it found it (changes are undone and everything is selected again), so runs are comparable.
/// Benchmarks whose description says "reselect" include the cost of selecting all again.
/// The numbers should grow roughly in proportion to the shape count. A jump by far more than 10 times from 1,000 to 10,000 shapes
/// means a step became quadratic.
/// </summary>
[MemoryDiagnoser]
public class ShapeOperationBenchmarks
{
    private EditorState _state = null!;
    private SelectionEditor _editor = null!;
    private CanvasInteraction _interaction = null!;
    private CanvasAreaViewModel _canvas = null!;
    private double _handleX;
    private double _handleY;

    // 50,000 shapes needs a lot of memory and time: add them by setting PTS_BENCH_LARGE=1.
    [ParamsSource(nameof(Counts))]
    public int ShapeCount { get; set; }

    public static int[] Counts { get; } = Environment.GetEnvironmentVariable("PTS_BENCH_LARGE") == "1" ? [1_000, 10_000, 50_000] : [1_000, 10_000];

    [GlobalSetup]
    public void Setup()
    {
        var empty = Document.Create("Layer");
        var shapes = Enumerable.Range(0, ShapeCount)
            .Select(i => i % 2 == 0
                ? (Shape)new RectangleShape { X = i % 100 * 12, Y = i / 100 * 8, Width = 10, Height = 6 }
                : new EllipseShape { X = i % 100 * 12, Y = i / 100 * 8, Width = 10, Height = 6 })
            .ToArray();
        _state = new EditorState(empty with { Layers = [empty.Layers[0] with { Shapes = [.. shapes] }] });
        _canvas = new CanvasAreaViewModel(_state) { ViewportWidth = 1400, ViewportHeight = 800 };
        _editor = new SelectionEditor(_state);
        _interaction = new CanvasInteraction(_state);
        _state.Document.HitTest(0, 0); // first call outside the measurement
        _editor.SelectAll();

        var frame = SelectionGeometry.FrameOf(SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds))!;
        (_handleX, _handleY) = SelectionGeometry.HandlePosition(frame, HandleType.BottomRight, 100);
    }

    [Benchmark(Description = "Select all")]
    public void SelectAll()
    {
        _state.ClearSelection();
        _editor.SelectAll();
    }

    [Benchmark(Description = "Marquee select (reselect)")]
    public void MarqueeSelect()
    {
        _interaction.PointerPressed(-5, -5);
        _interaction.PointerMoved(500, 300);
        _interaction.PointerReleased(500, 300);
        _editor.SelectAll();
    }

    [Benchmark(Description = "Selection overlay build")]
    public int Overlay() => _canvas.Overlay.Outlines.Count;

    [Benchmark(Description = "Click hit test")]
    public int HitTest() => _state.Document.HitTest(600, 300).Count;

    [Benchmark(Description = "Handle drag (press, move, cancel, undo)")]
    public void HandleDrag()
    {
        _interaction.PointerPressed(_handleX, _handleY);
        _interaction.PointerMoved(_handleX + 30, _handleY + 20);
        _interaction.Cancel();
        _state.Undo();
    }

    [Benchmark(Description = "Group move (press, move, cancel, undo)")]
    public void GroupMove()
    {
        _interaction.PointerPressed(5, 3);
        _interaction.PointerMoved(45, 33);
        _interaction.Cancel();
        _state.Undo();
    }

    [Benchmark(Description = "Arrow-key nudge (+ undo)")]
    public void Nudge()
    {
        _editor.Nudge(1, 0);
        _state.Undo();
    }

    [Benchmark(Description = "Duplicate (+ undo, reselect)")]
    public void Duplicate()
    {
        _editor.Duplicate();
        _state.Undo();
        _editor.SelectAll();
    }

    [Benchmark(Description = "Cut (+ undo, reselect)")]
    public void Cut()
    {
        _editor.Cut();
        _state.Undo();
        _editor.SelectAll();
    }

    [Benchmark(Description = "Copy and paste (+ undo, reselect)")]
    public void CopyPaste()
    {
        _editor.Copy();
        _editor.Paste(false);
        _state.Undo();
        _editor.SelectAll();
    }

    [Benchmark(Description = "Properties values of the selection")]
    public bool Values() => SelectionValues.From(SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds)) is not null;

    [Benchmark(Description = "Save project file (JSON)")]
    public byte[] SaveProject() => ProjectFile.Write(_state.Document);

    [Benchmark(Description = "Export SVG")]
    public byte[] ExportSvg() => SvgExport.ToUtf8(_state.Document, new FakeTextMeasurer());
}
