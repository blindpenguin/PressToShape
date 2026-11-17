# PressToShape.Core.Tests.PathOperations

Project: PressToShape.Core.Tests

## BooleanEngineTests (class)

`public class BooleanEngineTests`

Source: `tests/PressToShape.Core.Tests/PathOperations/BooleanEngineTests.cs:12-250`

_(no documentation)_

**Members**

- `public void Union_OverlappingSquares_GivesOneStraightOutlineWithTheUnionArea()` - _(no documentation)_
- `public void Union_DisjointSquares_GivesTwoSubPaths()` - _(no documentation)_
- `public void Union_SquaresSharingAnEdge_AreMerged()` - _(no documentation)_
- `public void Difference_SquareInsideSquare_LeavesAHole()` - _(no documentation)_
- `public void Difference_CoveredBottomOperand_IsEmpty()` - _(no documentation)_
- `public void Intersection_OverlappingSquares_GivesTheOverlap()` - _(no documentation)_
- `public void Intersection_SeparateSquares_IsEmpty()` - _(no documentation)_
- `public void Exclusion_OverlappingSquares_RemovesTheOverlap()` - _(no documentation)_
- `public void Difference_ThreeOperands_CutsEveryUpperOneOutOfTheBottomOne()` - _(no documentation)_
- `public void Exclusion_ThreeIdenticalSquares_KeepsTheAreaCoveredAnOddNumberOfTimes()` - _(no documentation)_
- `public void Exclusion_TwoIdenticalSquares_IsEmpty()` - _(no documentation)_
- `public void Union_OperandWithEvenOddHole_KeepsTheHoleWhereNothingFillsIt()` - _(no documentation)_
- `public void Intersection_RotatedSquareWithItsUnrotatedSelf_GivesARegularOctagon()` - _(no documentation)_
- `public void Intersection_CircleWithHalfPlaneSquare_KeepsTheCurvesAsFewNodes()` - _(no documentation)_
- `public void Union_TwoSeparateCircles_KeepsEachAsItsFourOriginalNodes()` - _(no documentation)_
- `public void Difference_CircleInsideCircle_GivesTwoCurvedSubPathsWithTheRingArea()` - _(no documentation)_
- `public void Difference_CrescentOutlineStaysWithinTheToleranceOfTheTwoCircles()` - _(no documentation)_
- `public void Compute_SameInputTwice_GivesEqualResults()` - _(no documentation)_
- `public void Compute_CancelledToken_Throws()` - _(no documentation)_
- `public void Compute_FewerThanTwoOperands_Throws()` - _(no documentation)_
- `public void Compute_UndefinedOperation_Throws()` - _(no documentation)_

## Clipper2BehaviorTests (class)

`public class Clipper2BehaviorTests`

Source: `tests/PressToShape.Core.Tests/PathOperations/Clipper2BehaviorTests.cs:16-112`

Documents how the Clipper2 package behaves for the boolean operations (spec boolean-operations, task 2). Finding about Z: see PressToShape.Core.Tests.PathOperations.Clipper2BehaviorTests.PointD_HasNoZValue_SoSourceTagsAreMatchedByPosition. The NuGet build of 1.5.4 is compiled without USINGZ, so a point has no Z value and there is no Z callback. The engine therefore matches result points to the flattened source points and source edges by position (the fallback of the design).

**Members**

- `public void PointD_HasNoZValue_SoSourceTagsAreMatchedByPosition()` - _(no documentation)_
- `public void Union_OfOverlappingSquares_GivesOneOutlineWithTheUnionArea()` - _(no documentation)_
- `public void Difference_OfSquareInsideSquare_GivesOuterAndHoleWithOppositeWinding()` - _(no documentation)_
- `public void Intersect_OfOverlappingSquares_GivesTheOverlap()` - _(no documentation)_
- `public void Xor_OfOverlappingSquares_RemovesTheOverlap()` - _(no documentation)_
- `public void Intersect_OfDisjointSquares_IsEmpty()` - _(no documentation)_
- `public void Union_OfDisjointSquares_GivesTwoOutlines()` - _(no documentation)_
- `public void Union_OfSquaresSharingAnEdge_MergesThem()` - _(no documentation)_
- `public void Difference_OfCoveredSquare_IsEmpty()` - _(no documentation)_
- `public void Union_RunTwice_GivesTheSameResult()` - _(no documentation)_

## CurveFittingTests (class)

`public class CurveFittingTests`

Source: `tests/PressToShape.Core.Tests/PathOperations/CurveFittingTests.cs:10-92`

_(no documentation)_

**Members**

- `public void Fit_PolygonAlongOneFlattenedSegment_BecomesOneCubic()` - _(no documentation)_
- `public void Fit_PolygonRunningAgainstTheCurve_StillKeepsTheCurve()` - _(no documentation)_
- `public void Fit_ChordAcrossABigArc_IsALine()` - _(no documentation)_
- `public void Fit_PolygonWithoutAnySource_IsLines()` - _(no documentation)_
- `public void Fit_KeptCurveNodes_HaveTheTypeTheirHandlesGive()` - _(no documentation)_
