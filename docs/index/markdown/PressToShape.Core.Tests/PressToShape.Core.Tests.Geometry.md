# PressToShape.Core.Tests.Geometry

Project: PressToShape.Core.Tests

## BezierTests (class)

`public class BezierTests`

Source: `tests/PressToShape.Core.Tests/Geometry/BezierTests.cs:8-187`

_(no documentation)_

**Members**

- `public void Point_AtTheEnds_ReturnsTheEndPoints(double t, double x, double y)` - _(no documentation)_
- `public void Point_AtHalf_ReturnsTheKnownMiddle()` - _(no documentation)_
- `public void Split_AnyParameter_PiecesDrawTheOriginalCurve(double t)` - _(no documentation)_
- `public void Split_AnyParameter_PiecesMeetAtTheSplitPoint()` - _(no documentation)_
- `public void ExtremaBounds_CurveThatOvershootsItsEnds_ReachesTheExtremeNotTheHandles()` - _(no documentation)_
- `public void ExtremaBounds_SCurve_FindsBothExtremes()` - _(no documentation)_
- `public void ExtremaBounds_Line_IsTheBoxOfTheEndPoints()` - _(no documentation)_
- `public void ExtremaBounds_AllPointsEqual_HasZeroSize()` - _(no documentation)_
- `public void Flatten_Line_ReturnsOnlyTheEndPoints()` - _(no documentation)_
- `public void Flatten_Curve_StaysWithinTheToleranceOfTheCurve()` - _(no documentation)_
- `public void Flatten_DegenerateCurve_DoesNotProduceNaN()` - _(no documentation)_
- `public void NearestParameter_PointBesideALine_ReturnsTheParameterOfItsFoot()` - _(no documentation)_
- `public void NearestParameter_PointOnTheCurve_ReturnsItsParameter()` - _(no documentation)_
- `public void NearestParameter_PointBeyondTheEnd_ClampsToTheEnd()` - _(no documentation)_

## FlatteningTests (class)

`public class FlatteningTests`

Source: `tests/PressToShape.Core.Tests/Geometry/FlatteningTests.cs:8-116`

_(no documentation)_

**Members**

- `public void Flatten_OpenLine_GivesStartAndEndPoint()` - _(no documentation)_
- `public void Flatten_ClosedSquare_GivesFourPointsWithoutRepeatingTheFirst()` - _(no documentation)_
- `public void Flatten_Curve_ChordsStayWithinTheTolerance()` - _(no documentation)_
- `public void Flatten_Curve_TagsGrowFromZeroToOneAndPointsLieOnTheCurve()` - _(no documentation)_
- `public void Flatten_TighterTolerance_GivesMorePoints()` - _(no documentation)_
- `public void Flatten_SameInputTwice_GivesEqualOutput()` - _(no documentation)_
- `public void Flatten_BadTolerance_Throws(double tolerance)` - _(no documentation)_
- `public void Flatten_SegmentWithANumberThatIsNotFinite_Throws()` - _(no documentation)_
