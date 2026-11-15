# PressToShape.Benchmarks

Project: PressToShape.Benchmarks

## Program (class)

`internal static class Program`

Source: `tests/PressToShape.Benchmarks/Program.cs:6-25`

_(no documentation)_

## ShapeOperationBenchmarks (class)

`public class ShapeOperationBenchmarks`

Source: `tests/PressToShape.Benchmarks/ShapeOperationBenchmarks.cs:19-135`

How long the shape operations take on a large drawing: a grid of rectangles and ellipses on one layer, all of them selected. Every benchmark leaves the state as it found it (changes are undone and everything is selected again), so runs are comparable. Benchmarks whose description says "reselect" include the cost of selecting all again. The numbers should grow roughly in proportion to the shape count. A jump by far more than 10 times from 1,000 to 10,000 shapes means a step became quadratic.

**Members**

- `public int ShapeCount { get; set; }` - _(no documentation)_
- `public int[] Counts { get; }` - _(no documentation)_
- `public void Setup()` - _(no documentation)_
- `public void SelectAll()` - _(no documentation)_
- `public void MarqueeSelect()` - _(no documentation)_
- `public int Overlay()` - _(no documentation)_
- `public int HitTest()` - _(no documentation)_
- `public void HandleDrag()` - _(no documentation)_
- `public void GroupMove()` - _(no documentation)_
- `public void Nudge()` - _(no documentation)_
- `public void Duplicate()` - _(no documentation)_
- `public void Cut()` - _(no documentation)_
- `public void CopyPaste()` - _(no documentation)_
- `public bool Values()` - _(no documentation)_
- `public byte[] SaveProject()` - _(no documentation)_
- `public byte[] ExportSvg()` - _(no documentation)_
