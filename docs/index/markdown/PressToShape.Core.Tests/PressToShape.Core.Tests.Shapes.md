# PressToShape.Core.Tests.Shapes

Project: PressToShape.Core.Tests

## AppearanceTests (class)

`public class AppearanceTests`

Source: `tests/PressToShape.Core.Tests/Shapes/AppearanceTests.cs:7-100`

_(no documentation)_

**Members**

- `public void Default_Always_HasBlueFillDarkStrokeWidthOneAndFullOpacity()` - _(no documentation)_
- `public void With_NullFillAndStroke_MeansNone()` - _(no documentation)_
- `public void With_ChangedFill_LeavesOriginalUnchanged()` - _(no documentation)_
- `public void Constructor_NewInstance_DefaultsToStrokeWidthOneAndFullOpacity()` - _(no documentation)_
- `public void StrokeWidth_NotNegativeAndFinite_IsAccepted(double width)` - _(no documentation)_
- `public void StrokeWidth_Invalid_Throws(double width)` - _(no documentation)_
- `public void Opacity_InRange_IsAccepted(double opacity)` - _(no documentation)_
- `public void Opacity_Invalid_Throws(double opacity)` - _(no documentation)_
- `public void Constructor_InvalidInitializer_Throws()` - _(no documentation)_

## ColorTests (class)

`public class ColorTests`

Source: `tests/PressToShape.Core.Tests/Shapes/ColorTests.cs:6-35`

_(no documentation)_

**Members**

- `public void FromRgb_AnyChannels_SetsAlphaTo255()` - _(no documentation)_
- `public void Constructor_AllChannels_KeepsEachValue()` - _(no documentation)_
- `public void Equals_SameChannels_IsEqual()` - _(no documentation)_
- `public void Equals_DifferentAlpha_IsNotEqual()` - _(no documentation)_

## EllipseShapeTests (class)

`public class EllipseShapeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/EllipseShapeTests.cs:7-82`

_(no documentation)_

**Members**

- `public void Bounds_Ellipse_EqualsPositionAndSize()` - _(no documentation)_
- `public void Contains_Point_IsInsideOrOnTheEllipse(double x, double y, bool expected)` - _(no documentation)_
- `public void Contains_ZeroWidth_ActsAsVerticalSegment(double x, double y, bool expected)` - _(no documentation)_
- `public void Contains_ZeroHeight_ActsAsHorizontalSegment(double x, double y, bool expected)` - _(no documentation)_
- `public void Contains_ZeroWidthAndHeight_ContainsOnlyItsOwnPoint(double x, double y, bool expected)` - _(no documentation)_
- `public void Contains_FarAwayPoint_IsFalse()` - _(no documentation)_

## GroupShapeTests (class)

`public class GroupShapeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/GroupShapeTests.cs:9-160`

_(no documentation)_

**Members**

- `public void Bounds_OneChild_EqualsChildBounds()` - _(no documentation)_
- `public void Bounds_TwoChildren_IsUnion()` - _(no documentation)_
- `public void Bounds_RotatedChild_UsesRotatedBounds()` - _(no documentation)_
- `public void Bounds_NestedGroups_IncludesInnerGroup()` - _(no documentation)_
- `public void Children_AnyOrder_KeepsOrder()` - _(no documentation)_
- `public void Children_Empty_Throws()` - _(no documentation)_
- `public void Children_DefaultArray_Throws()` - _(no documentation)_
- `public void Children_ContainsNull_Throws()` - _(no documentation)_
- `public void With_NewChildren_RecomputesBox()` - _(no documentation)_
- `public void Name_Null_Throws()` - _(no documentation)_
- `public void Flags_NewShape_VisibleUnlockedWithoutName()` - _(no documentation)_
- `public void Contains_PointOnChild_True()` - _(no documentation)_
- `public void Contains_PointOnHiddenChild_False()` - _(no documentation)_
- `public void Constructor_NoId_GetsUniqueIds()` - _(no documentation)_
- `public void MovedCopy_Group_GivesNewIdsAndMovesAllLeaves()` - _(no documentation)_
- `public void Leaves_PlainShapeAndNestedGroups_ReturnTheNonGroupShapes()` - _(no documentation)_

## NodeTypesTests (class)

`public class NodeTypesTests`

Source: `tests/PressToShape.Core.Tests/Shapes/NodeTypesTests.cs:6-131`

_(no documentation)_

**Members**

- `public void Derive_NoHandles_IsCorner()` - _(no documentation)_
- `public void Derive_OneHandle_IsCorner()` - _(no documentation)_
- `public void Derive_OppositeAndEqual_IsSymmetric()` - _(no documentation)_
- `public void Derive_OppositeAndUnequal_IsSmooth()` - _(no documentation)_
- `public void Derive_SameSide_IsCorner()` - _(no documentation)_
- `public void Derive_AtAnAngle_IsCorner()` - _(no documentation)_
- `public void Derive_ZeroLengthHandle_IsCorner()` - _(no documentation)_
- `public void Derive_FloatingPointNoise_StillCountsAsOnOneLine()` - _(no documentation)_
- `public void Derive_SlightlyOffTheLine_IsCorner()` - _(no documentation)_
- `public void Derive_NodeOfTheStoredKind_IgnoresTheStoredType()` - _(no documentation)_
- `public void Derive_PathNode_GivesTheSameAnswer()` - _(no documentation)_
- `public void Satisfies_NodesWithAtMostOneHandle_IsTrueForEveryType(NodeType type)` - _(no documentation)_
- `public void Satisfies_Corner_IsAlwaysTrue()` - _(no documentation)_
- `public void Satisfies_Smooth_NeedsOppositeHandlesOnOneLine()` - _(no documentation)_
- `public void Satisfies_Symmetric_AlsoNeedsEqualLengths()` - _(no documentation)_
- `public void Satisfies_ZeroLengthHandle_OnlyTheLengthsCanDisagree()` - _(no documentation)_
- `public void Satisfies_PathNode_UsesItsStoredType()` - _(no documentation)_

## PathEditingTests (class)

`public class PathEditingTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathEditingTests.cs:10-313`

_(no documentation)_

**Members**

- `public void InsertNode_OnALine_AddsACornerOnTheLine()` - _(no documentation)_
- `public void InsertNode_OnACurve_KeepsTheCurve(int segment, double t)` - _(no documentation)_
- `public void InsertNode_OnTheClosingSegmentOfAClosedPath_AppendsTheNode()` - _(no documentation)_
- `public void InsertNode_BadArguments_Throw()` - _(no documentation)_
- `public void RemoveNode_MiddleNode_NeighborsConnect()` - _(no documentation)_
- `public void RemoveNode_EndNodeOfAnOpenPath_ShortensThePath()` - _(no documentation)_
- `public void RemoveNode_AtTwoNodes_ReturnsNull()` - _(no documentation)_
- `public void RemoveNode_BadIndex_Throws()` - _(no documentation)_
- `public void Continue_AtTheEnd_ReturnsTheNodesAsTheyAre()` - _(no documentation)_
- `public void Continue_AtTheStart_ReversesTheOrderAndSwapsTheHandles()` - _(no documentation)_
- `public void Continue_ClosedPath_Throws()` - _(no documentation)_
- `public void InsertAndRemove_RotatedPath_KeepEveryOriginalNodeWhereItWas(double rotation)` - _(no documentation)_
- `public void InsertNode_OnACurve_GivesTheNewNodeTheSmoothType()` - _(no documentation)_
- `public void InsertNode_OnALine_GivesTheNewNodeTheCornerType()` - _(no documentation)_
- `public void InsertNode_NewHandlesOnACurve_LieOnTheTangent()` - _(no documentation)_
- `public void InsertNode_KeepsTheTypesOfTheNeighbors()` - _(no documentation)_
- `public void InsertNode_NextToASymmetricNode_TheSymmetricNodeBecomesSmoothBecauseItsHandleIsShorter()` - _(no documentation)_
- `public void Continue_AtTheStart_KeepsTheTypesOfTheReversedNodes()` - _(no documentation)_
- `public void RemoveNode_KeepsTheTypesOfTheOtherNodes()` - _(no documentation)_

## PathNodeEditingMoveTests (class)

`public class PathNodeEditingMoveTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathNodeEditingMoveTests.cs:9-322`

_(no documentation)_

**Members**

- `public void MoveNodes_OneNode_MovesTheAnchorAndBothHandles()` - _(no documentation)_
- `public void MoveNodes_SeveralNodes_MoveByTheSameDistanceAndOthersStay()` - _(no documentation)_
- `public void MoveNodes_NoIndicesOrZeroDistance_KeepsThePicture()` - _(no documentation)_
- `public void MoveNodes_RepeatedIndex_MovesTheNodeOnce()` - _(no documentation)_
- `public void MoveNodes_RotatedPath_NodesThatWereNotMovedStayWhereTheyAreOnTheScreen(double rotation)` - _(no documentation)_
- `public void MoveNodes_BadIndex_Throws()` - _(no documentation)_
- `public void MoveHandle_CornerNode_MovesOnlyThatHandle()` - _(no documentation)_
- `public void MoveHandle_SmoothNode_TheOtherHandleStaysOppositeWithItsOwnLength()` - _(no documentation)_
- `public void MoveHandle_SmoothNode_MovingTheInHandleTurnsTheOutHandle()` - _(no documentation)_
- `public void MoveHandle_SymmetricNode_TheOtherHandleMirrorsIt()` - _(no documentation)_
- `public void MoveHandle_WithBreakLink_MovesOnlyThatHandleAndMakesACorner()` - _(no documentation)_
- `public void MoveHandle_OntoItsAnchor_RemovesTheHandleAndMakesACorner(NodeHandleSide side)` - _(no documentation)_
- `public void MoveHandle_NodeWithOnlyOneHandle_DoesNotCreateTheOther()` - _(no documentation)_
- `public void MoveHandle_NodeWithoutHandles_GetsTheHandleThatWasMoved()` - _(no documentation)_
- `public void MoveHandle_SmoothNodeWhoseOtherHandleHasNoLength_StaysUsable()` - _(no documentation)_
- `public void MoveHandle_RotatedPath_OtherNodesStayWhereTheyAreOnTheScreen(double rotation)` - _(no documentation)_
- `public void MoveHandle_BadInput_Throws()` - _(no documentation)_
- `public void RemoveNodes_Several_NeighborsConnect()` - _(no documentation)_
- `public void RemoveNodes_DownToOneNode_ReturnsNull()` - _(no documentation)_
- `public void RemoveNodes_DownToTwoNodes_KeepsThePath()` - _(no documentation)_
- `public void RemoveNodes_NothingToRemove_ReturnsTheSamePath()` - _(no documentation)_
- `public void RemoveNodes_ClosedPath_KeepsItClosed()` - _(no documentation)_
- `public void RemoveNodes_RotatedPath_KeepsTheOtherNodesOnTheScreen(double rotation)` - _(no documentation)_
- `public void RemoveNodes_BadIndex_Throws()` - _(no documentation)_
- `public void NodePosition_RotatedPath_IsTheTurnedPosition(double rotation)` - _(no documentation)_
- `public void FromLocal_IsTheInverseOfToLocal()` - _(no documentation)_

## PathNodeEditingSegmentTests (class)

`public class PathNodeEditingSegmentTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathNodeEditingSegmentTests.cs:9-321`

_(no documentation)_

**Members**

- `public void BendSegment_PointOnTheCurve_MovesByTheDistance(double t)` - _(no documentation)_
- `public void BendSegment_NodesDoNotMoveAndOtherSegmentsStay()` - _(no documentation)_
- `public void BendSegment_StraightSegment_BecomesACurveThroughTheMovedPoint()` - _(no documentation)_
- `public void BendSegment_ClosingSegmentOfAClosedPath_Works()` - _(no documentation)_
- `public void BendSegment_SmoothAndSymmetricNodesAtTheEnds_BecomeCorners()` - _(no documentation)_
- `public void BendSegment_NodeWhoseHandlesStillFitItsType_KeepsTheType()` - _(no documentation)_
- `public void BendSegment_NearAnEnd_StaysFiniteAndGentle()` - _(no documentation)_
- `public void BendSegment_NoDistance_ReturnsTheSamePath()` - _(no documentation)_
- `public void BendSegment_ZeroLengthSegment_GivesNoNaN()` - _(no documentation)_
- `public void BendSegment_RotatedPath_AnchorsStayWhereTheyAreOnTheScreen(double rotation)` - _(no documentation)_
- `public void BendSegment_BadInput_Throws()` - _(no documentation)_
- `public void StraightenSegment_RemovesBothHandlesAndMakesCornersOfTheNodesThatLostOne()` - _(no documentation)_
- `public void StraightenSegment_NodeThatLostNothing_KeepsItsType()` - _(no documentation)_
- `public void StraightenSegment_AlreadyStraight_ReturnsTheSamePath()` - _(no documentation)_
- `public void StraightenSegment_ClosingSegment_RemovesItsHandles()` - _(no documentation)_
- `public void CurveSegment_StraightSegment_GetsHandlesAtThirdsOnTheLine()` - _(no documentation)_
- `public void CurveSegment_StraightSegment_DoesNotChangeThePicture()` - _(no documentation)_
- `public void CurveSegment_AlreadyCurved_ReturnsTheSamePath()` - _(no documentation)_
- `public void CurveSegment_SegmentWithOneHandle_CountsAsCurvedAndStaysAsItIs()` - _(no documentation)_
- `public void CurveSegment_NodeWhoseOtherHandleNoLongerFits_BecomesACorner()` - _(no documentation)_
- `public void CurveSegment_SegmentOfNoLength_ReturnsTheSamePath()` - _(no documentation)_
- `public void StraightenAndCurve_RotatedPath_AnchorsStayWhereTheyAre(double rotation)` - _(no documentation)_
- `public void SegmentMethods_BadIndex_Throw()` - _(no documentation)_

## PathNodeEditingTypeTests (class)

`public class PathNodeEditingTypeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathNodeEditingTypeTests.cs:8-259`

_(no documentation)_

**Members**

- `public void SetNodeType_ToCorner_OnlyChangesTheType()` - _(no documentation)_
- `public void SetNodeType_ToSmoothWithBothHandles_TheOutDirectionWinsAndTheInHandleKeepsItsLength()` - _(no documentation)_
- `public void SetNodeType_ToSymmetricWithBothHandles_TheInHandleGetsTheOutLength()` - _(no documentation)_
- `public void SetNodeType_NodeWithOnlyAnInHandle_TheOutHandleFollowsIt()` - _(no documentation)_
- `public void SetNodeType_NodeWithOnlyAnOutHandle_TheInHandleIsACreatedThirdOfTheWayToThePreviousNodeForSmooth()` - _(no documentation)_
- `public void SetNodeType_NodeWithoutHandles_GetsHandlesOnTheLineBetweenItsNeighbors(NodeType type)` - _(no documentation)_
- `public void SetNodeType_SymmetricWithNeighborsAtDifferentDistances_BothHandlesGetTheShorterThird()` - _(no documentation)_
- `public void SetNodeType_SmoothWithNeighborsAtDifferentDistances_HandlesKeepTheirOwnThird()` - _(no documentation)_
- `public void SetNodeType_FirstNodeOfAnOpenPath_OnlyGetsTheOutHandle()` - _(no documentation)_
- `public void SetNodeType_LastNodeOfAnOpenPath_OnlyGetsTheInHandle()` - _(no documentation)_
- `public void SetNodeType_FirstNodeOfAClosedPath_UsesTheLastNodeAsItsPreviousNeighbor()` - _(no documentation)_
- `public void SetNodeType_NeighborsOnTheSameSpot_CreatesNoHandleAndNoNaN()` - _(no documentation)_
- `public void SetNodeType_NeighborOnTheSpotOfTheNode_OnlyTheOtherSideGetsAHandle()` - _(no documentation)_
- `public void SetNodeType_SeveralNodes_ChangesAllOfThem()` - _(no documentation)_
- `public void SetNodeType_NoIndices_ReturnsTheSamePath()` - _(no documentation)_
- `public void SetNodeType_BadInput_Throws()` - _(no documentation)_
- `public void SetNodeType_RotatedPath_EveryAnchorStaysWhereItIsOnTheScreen(double rotation)` - _(no documentation)_
- `public void SetNodeType_NodesNotInTheList_AreUntouched()` - _(no documentation)_

## PathNodeTests (class)

`public class PathNodeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathNodeTests.cs:7-116`

_(no documentation)_

**Members**

- `public void Create_AnchorOnly_HasNoHandles()` - _(no documentation)_
- `public void Create_WithBothHandles_KeepsThem()` - _(no documentation)_
- `public void Create_NonFiniteAnchor_Throws(double x, double y)` - _(no documentation)_
- `public void Create_NonFiniteHandle_Throws()` - _(no documentation)_
- `public void Create_HalfAHandle_Throws()` - _(no documentation)_
- `public void Equals_SameValues_AreEqual()` - _(no documentation)_
- `public void DocumentNode_ValidAndInvalidValues_FollowTheSameRules()` - _(no documentation)_
- `public void FillRule_Default_IsNonZero()` - _(no documentation)_
- `public void Type_NotGiven_IsCorner()` - _(no documentation)_
- `public void Type_Given_IsKept(NodeType type)` - _(no documentation)_
- `public void Type_NotDefined_Throws()` - _(no documentation)_
- `public void Equals_NodesThatDifferOnlyInType_AreNotEqual()` - _(no documentation)_

## PathShapeGeometryTests (class)

`public class PathShapeGeometryTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathShapeGeometryTests.cs:9-200`

_(no documentation)_

**Members**

- `public void Rebuilt_MovedNode_EveryNodeStaysWhereItWasOnTheScreen(double rotation)` - _(no documentation)_
- `public void Rebuilt_Edit_KeepsIdAppearanceNameFlagsAndRotation()` - _(no documentation)_
- `public void Rebuilt_FewerThanTwoNodes_Throws()` - _(no documentation)_
- `public void Contains_PentagramCenterWithNonZero_IsInside()` - _(no documentation)_
- `public void Contains_PentagramCenterWithEvenOdd_IsOutside()` - _(no documentation)_
- `public void Contains_PentagramPointOfTheStar_IsInsideWithBothRules(FillRule rule)` - _(no documentation)_
- `public void Contains_OutsideAClosedFilledPath_IsAMiss()` - _(no documentation)_
- `public void Contains_ThinStrokeOfAnUnfilledOpenPath_IsHitNearTheLineOnly()` - _(no documentation)_
- `public void Contains_WideStroke_HitsWithinHalfTheStrokeWidth()` - _(no documentation)_
- `public void Contains_UnfilledOpenCurve_DoesNotHitTheEmptyInside()` - _(no documentation)_
- `public void Contains_FilledOpenPath_IsFilledAcrossTheGapButTheClosingLineIsNotAnOutline()` - _(no documentation)_
- `public void Contains_RotatedPath_UsesTheTurnedPath()` - _(no documentation)_
- `public void Contains_CurvedClosedPath_FollowsTheCurve()` - _(no documentation)_

## PathShapeNodeTypeTests (class)

`public class PathShapeNodeTypeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathShapeNodeTypeTests.cs:7-125`

_(no documentation)_

**Members**

- `public void Create_NodesWithTypes_StoresTheTypes()` - _(no documentation)_
- `public void ToDocumentNodes_AfterCreate_ReturnsTheTypes()` - _(no documentation)_
- `public void With_ResizedBox_KeepsTheTypes()` - _(no documentation)_
- `public void Rebuilt_WithTypedNodes_KeepsTheTypesAndTheScreenPositions(double rotation)` - _(no documentation)_
- `public void Rebuilt_ChangedNodeTypes_AreTheNewOnes()` - _(no documentation)_
- `public void ToLocal_PositionOnTheScreen_IsTheInverseOfTheRotation(double rotation)` - _(no documentation)_
- `public void ToLocal_WithoutRotation_ChangesNothing()` - _(no documentation)_
- `public void ToLocalDelta_QuarterTurn_TurnsTheDistanceBack()` - _(no documentation)_

## PathShapeTests (class)

`public class PathShapeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/PathShapeTests.cs:10-229`

_(no documentation)_

**Members**

- `public void Create_Line_BoxIsTheBoxOfTheEndPoints()` - _(no documentation)_
- `public void Create_Triangle_StoresNodesAsFractionsOfTheBox()` - _(no documentation)_
- `public void Create_CurveThatOvershootsItsAnchors_BoxReachesTheCurveNotTheHandles()` - _(no documentation)_
- `public void Create_ClosedPathWithACurvedClosingSegment_BoxIncludesTheClosingCurve()` - _(no documentation)_
- `public void Create_FewerThanTwoNodes_Throws()` - _(no documentation)_
- `public void Nodes_FewerThanTwoOrDefault_Throws()` - _(no documentation)_
- `public void Create_HorizontalLine_HasZeroHeightAndKeepsItsPosition()` - _(no documentation)_
- `public void Create_VerticalLine_HasZeroWidthAndKeepsItsPosition()` - _(no documentation)_
- `public void Create_TwoEqualPoints_HasZeroSizeWithoutNaN()` - _(no documentation)_
- `public void ToDocumentNodes_AfterCreate_ReturnsTheGivenNodes()` - _(no documentation)_
- `public void With_DoubledWidth_ScalesThePicture()` - _(no documentation)_
- `public void With_NewPosition_MovesThePicture()` - _(no documentation)_
- `public void With_WidthZeroAndBack_RestoresThePicture()` - _(no documentation)_
- `public void MovedCopy_Path_MovesTheBoxAndGetsANewId()` - _(no documentation)_
- `public void RotatedBounds_RotatedPath_IsTheTurnedBox()` - _(no documentation)_
- `public void Segments_ClosedTriangle_HasThreeLines()` - _(no documentation)_
- `public void Segments_OpenPath_HasOneLessThanNodes()` - _(no documentation)_

## RectangleShapeTests (class)

`public class RectangleShapeTests`

Source: `tests/PressToShape.Core.Tests/Shapes/RectangleShapeTests.cs:8-131`

_(no documentation)_

**Members**

- `public void Bounds_Rectangle_EqualsPositionAndSize()` - _(no documentation)_
- `public void Bounds_WithStroke_DoesNotIncludeStrokeWidth()` - _(no documentation)_
- `public void Constructor_NoId_GetsNonEmptyUniqueIds()` - _(no documentation)_
- `public void Constructor_NoAppearance_UsesDefault()` - _(no documentation)_
- `public void With_ChangedGeometry_KeepsIdAndLeavesOriginalUnchanged()` - _(no documentation)_
- `public void Constructor_ZeroWidthAndHeight_IsAllowed()` - _(no documentation)_
- `public void Width_Invalid_Throws(double width)` - _(no documentation)_
- `public void Height_Invalid_Throws(double height)` - _(no documentation)_
- `public void Position_NotFinite_Throws(double value)` - _(no documentation)_
- `public void Position_Negative_IsAllowed()` - _(no documentation)_
- `public void Id_Empty_Throws()` - _(no documentation)_
- `public void Appearance_Null_Throws()` - _(no documentation)_
- `public void Contains_Point_IncludesEdges(double x, double y, bool expected)` - _(no documentation)_

## ShapeConversionTests (class)

`public class ShapeConversionTests`

Source: `tests/PressToShape.Core.Tests/Shapes/ShapeConversionTests.cs:8-174`

_(no documentation)_

**Members**

- `public void ToPath_Rectangle_KeepsIdNameFlagsBoxRotationAndAppearance()` - _(no documentation)_
- `public void ToPath_Ellipse_KeepsIdNameFlagsBoxRotationAndAppearance()` - _(no documentation)_
- `public void ToPath_Rectangle_HasFourCornerNodesInOrder()` - _(no documentation)_
- `public void ToPath_Ellipse_HasFourSymmetricNodesWithTheCircleHandles()` - _(no documentation)_
- `public void ToPath_Rectangle_ContainsTheSamePointsAsTheRectangle(double rotation)` - _(no documentation)_
- `public void ToPath_Ellipse_ContainsTheSamePointsAsTheEllipseWithinTheApproximation(double rotation)` - _(no documentation)_
- `public void ToPath_ZeroSizeShapes_GiveNoNaN()` - _(no documentation)_
- `public void ToPath_PathGroupAndNull_AreNotConverted()` - _(no documentation)_

## ShapeRotationTests (class)

`public class ShapeRotationTests`

Source: `tests/PressToShape.Core.Tests/Shapes/ShapeRotationTests.cs:8-223`

_(no documentation)_

**Members**

- `public void Rotation_Default_IsZero()` - _(no documentation)_
- `public void Rotation_IsNormalizedToZeroUpTo360(double input, double expected)` - _(no documentation)_
- `public void Rotation_NotFinite_IsRejected(double value)` - _(no documentation)_
- `public void With_Rotation_KeepsIdAndEverythingElse()` - _(no documentation)_
- `public void Center_IsTheMiddleOfTheBox()` - _(no documentation)_
- `public void Contains_TheCenter_AlwaysHits(double rotation)` - _(no documentation)_
- `public void Contains_Rectangle_Unrotated_UsesTheBox()` - _(no documentation)_
- `public void Contains_Rectangle_Rotated90_SwapsWidthAndHeightAroundTheCenter()` - _(no documentation)_
- `public void Contains_Rectangle_Rotated45_CornerOfTheUnturnedBoxIsOutside()` - _(no documentation)_
- `public void Contains_Rectangle_Rotated180_IsTheSameAsUnrotated()` - _(no documentation)_
- `public void Contains_Rectangle_PositiveRotationTurnsClockwise()` - _(no documentation)_
- `public void Contains_Ellipse_Rotated90_SwapsTheRadii()` - _(no documentation)_
- `public void Contains_Ellipse_Rotated45_HitsAlongTheTurnedAxis()` - _(no documentation)_
- `public void Contains_ZeroSizeShapes_StillWorkWhenRotated()` - _(no documentation)_
- `public void RotatedBounds_Unrotated_EqualsBounds()` - _(no documentation)_
- `public void RotatedBounds_Rotated90_SwapsWidthAndHeightAroundTheCenter()` - _(no documentation)_
- `public void RotatedBounds_Rotated45_IsTheDiagonalBox()` - _(no documentation)_
- `public void RotatedBounds_ContainTheTurnedCorners()` - _(no documentation)_
- `public void Document_HitTest_FindsARotatedShapeWhereItIs()` - _(no documentation)_
