# PressToShape.Core.Tests.Documents

Project: PressToShape.Core.Tests

## ArrangeOperationsTests (class)

`public class ArrangeOperationsTests`

Source: `tests/PressToShape.Core.Tests/Documents/ArrangeOperationsTests.cs:10-250`

_(no documentation)_

**Members**

- `public void Arrange_BottomShape_MovesUp(ArrangeType type, string expected)` - _(no documentation)_
- `public void Arrange_TopShape_MovesDown(ArrangeType type, string expected)` - _(no documentation)_
- `public void Arrange_SeveralSelected_KeepRelativeOrder(ArrangeType type, string expected)` - _(no documentation)_
- `public void Arrange_Forward_EachSelectedMovesOnePlace()` - _(no documentation)_
- `public void Arrange_Backward_EachSelectedMovesOnePlace()` - _(no documentation)_
- `public void Arrange_AdjacentSelected_MoveTogetherPastOneSibling()` - _(no documentation)_
- `public void Arrange_SelectedAlreadyAtTheEnd_Stays()` - _(no documentation)_
- `public void Arrange_EverythingSelected_ReturnsSameDocument()` - _(no documentation)_
- `public void Arrange_NoIds_ReturnsSameDocument()` - _(no documentation)_
- `public void Arrange_UnknownId_Throws()` - _(no documentation)_
- `public void Arrange_InsideGroup_MovesOnlyAmongTheGroupsChildren()` - _(no documentation)_
- `public void Arrange_ChildAndTopLevelShape_AreArrangedAmongTheirOwnSiblings()` - _(no documentation)_
- `public void Arrange_TwoNestedLevels_BothChange()` - _(no documentation)_
- `public void Arrange_KeepsShapesInTheirLayer()` - _(no documentation)_
- `public void Arrange_Result_LeavesOriginalUnchanged()` - _(no documentation)_
- `public void MoveAbove_ShapeBelowTarget_LandsDirectlyAboveIt()` - _(no documentation)_
- `public void MoveAbove_ShapeAboveTarget_LandsDirectlyAboveIt()` - _(no documentation)_
- `public void MoveAbove_AlreadyThere_ReturnsSameDocument()` - _(no documentation)_
- `public void MoveAbove_InsideGroup_Works()` - _(no documentation)_
- `public void MoveAbove_DifferentParentsOrSameShape_Throws()` - _(no documentation)_

## DocumentBatchShapeTests (class)

`public class DocumentBatchShapeTests`

Source: `tests/PressToShape.Core.Tests/Documents/DocumentBatchShapeTests.cs:10-89`

_(no documentation)_

**Members**

- `public void UpdateShapes_ReplacesSeveralShapes_KeepingTheirPlaces()` - _(no documentation)_
- `public void UpdateShapes_NothingToReplace_ReturnsTheSameDocument_AndSharesUnchangedLayers()` - _(no documentation)_
- `public void UpdateShapes_UnknownIdOrWrongId_Throws()` - _(no documentation)_
- `public void RemoveShapes_RemovesSeveral_AcrossLayers()` - _(no documentation)_
- `public void RemoveShapes_EmptyAndUnknown()` - _(no documentation)_
- `public void AddShapes_AddsOnTopInOrder_AndRejectsDuplicateIdsAndUnknownLayers()` - _(no documentation)_

## DocumentGroupTests (class)

`public class DocumentGroupTests`

Source: `tests/PressToShape.Core.Tests/Documents/DocumentGroupTests.cs:10-239`

_(no documentation)_

**Members**

- `public void FindShape_InNestedGroup_ReturnsParentAndFlags()` - _(no documentation)_
- `public void FindShape_Group_ReturnsGroupWithoutParent()` - _(no documentation)_
- `public void FindShape_HiddenLayerOrGroup_IsNotEffectivelyVisible()` - _(no documentation)_
- `public void UpdateShapes_Leaf_RebuildsGroupBoxes()` - _(no documentation)_
- `public void UpdateShape_LeafInGroup_Changes()` - _(no documentation)_
- `public void UpdateShapes_GroupWithItsOwnBoxChanged_IsRejected()` - _(no documentation)_
- `public void RemoveShapes_LastChild_RemovesGroup()` - _(no documentation)_
- `public void RemoveShape_OneOfTwoChildren_ShrinksGroup()` - _(no documentation)_
- `public void RemoveShapes_UnknownId_Throws()` - _(no documentation)_
- `public void HitTest_GroupMembers_ReturnsLeavesTopmostFirst()` - _(no documentation)_
- `public void HitTest_HiddenShapeAndHiddenGroup_AreSkipped()` - _(no documentation)_
- `public void HitTest_LockedShape_IsStillReported()` - _(no documentation)_
- `public void AddShapes_ToGroup_AppendsAndGrowsBox()` - _(no documentation)_
- `public void AddShapes_UnknownContainer_Throws()` - _(no documentation)_
- `public void AddShapes_ContainerIsPlainShape_Throws()` - _(no documentation)_
- `public void Layers_DuplicateIdInsideGroup_IsRejected()` - _(no documentation)_
- `public void Layers_GroupWithWrongBox_IsRejected()` - _(no documentation)_
- `public void Layers_GroupsNested65Levels_AreRejected()` - _(no documentation)_
- `public void Layers_GroupsNested64Levels_AreAccepted()` - _(no documentation)_

## DocumentHitTestTests (class)

`public class DocumentHitTestTests`

Source: `tests/PressToShape.Core.Tests/Documents/DocumentHitTestTests.cs:9-115`

_(no documentation)_

**Members**

- `public void HitTest_OverlappingShapesInOneLayer_ReturnsTopmostFirst()` - _(no documentation)_
- `public void HitTest_ShapesInTwoLayers_UpperLayerComesBeforeLowerLayer()` - _(no documentation)_
- `public void HitTest_RectangleAndEllipse_EachUsesItsOwnGeometry()` - _(no documentation)_
- `public void HitTest_HiddenLayer_IsSkipped()` - _(no documentation)_
- `public void HitTest_LockedLayer_IsIncluded()` - _(no documentation)_
- `public void HitTest_Miss_ReturnsEmptyListNotNull()` - _(no documentation)_
- `public void HitTest_PointOnRectangleEdge_Hits()` - _(no documentation)_
- `public void HitTest_EllipseBoundingBoxCorner_Misses()` - _(no documentation)_
- `public void HitTest_StrokeWidth_IsIgnored()` - _(no documentation)_

## DocumentLayerTests (class)

`public class DocumentLayerTests`

Source: `tests/PressToShape.Core.Tests/Documents/DocumentLayerTests.cs:10-205`

_(no documentation)_

**Members**

- `public void Create_FirstLayerName_Has1200By800AndOneVisibleUnlockedLayer()` - _(no documentation)_
- `public void Create_NullName_Throws()` - _(no documentation)_
- `public void WithArtboardSize_InvalidWidth_Throws(double width, double height)` - _(no documentation)_
- `public void WithArtboardSize_InvalidHeight_Throws(double width, double height)` - _(no documentation)_
- `public void WithArtboardSize_Valid_ReturnsNewDocumentAndKeepsOriginal()` - _(no documentation)_
- `public void Layers_EmptyOrDefault_Throws()` - _(no documentation)_
- `public void AddLayer_NewLayer_IsPlacedOnTopAndOriginalStaysUnchanged()` - _(no documentation)_
- `public void AddLayer_NewLayer_SharesExistingLayerInstances()` - _(no documentation)_
- `public void AddLayer_DuplicateId_ThrowsAndLeavesDocumentUnchanged()` - _(no documentation)_
- `public void AddLayer_Null_Throws()` - _(no documentation)_
- `public void RemoveLayer_ExistingLayer_RemovesItAndKeepsTheOrderOfTheOthers()` - _(no documentation)_
- `public void RemoveLayer_LayerWithShapes_RemovesItsShapes()` - _(no documentation)_
- `public void RemoveLayer_LastLayer_Throws()` - _(no documentation)_
- `public void RemoveLayer_UnknownId_Throws()` - _(no documentation)_
- `public void UpdateLayer_NameVisibilityAndLock_ChangesOnlyThatLayer()` - _(no documentation)_
- `public void UpdateLayer_UnknownId_Throws()` - _(no documentation)_
- `public void UpdateLayer_ChangeReturnsOtherId_Throws()` - _(no documentation)_
- `public void UpdateLayer_ChangeReturnsNull_Throws()` - _(no documentation)_
- `public void UpdateLayer_NullChange_Throws()` - _(no documentation)_

## DocumentPaletteTests (class)

`public class DocumentPaletteTests`

Source: `tests/PressToShape.Core.Tests/Documents/DocumentPaletteTests.cs:9-90`

_(no documentation)_

**Members**

- `public void Create_NewDocument_HasEmptyPalette()` - _(no documentation)_
- `public void AddSwatch_NewColors_AppendsInOrder()` - _(no documentation)_
- `public void AddSwatch_ColorAlreadyInPalette_ReturnsSameInstance()` - _(no documentation)_
- `public void AddSwatch_SameRgbWithOtherAlpha_IsAnotherSwatch()` - _(no documentation)_
- `public void RemoveSwatchAt_ValidIndex_RemovesOnlyThatSwatch()` - _(no documentation)_
- `public void RemoveSwatchAt_BadIndex_Throws(int index)` - _(no documentation)_
- `public void AddSwatch_Document_KeepsLayersAndShapes()` - _(no documentation)_
- `public void Palette_DefaultArray_BecomesEmpty()` - _(no documentation)_
- `public void Palette_DuplicateColorsInInit_AreRemoved()` - _(no documentation)_

## DocumentShapeTests (class)

`public class DocumentShapeTests`

Source: `tests/PressToShape.Core.Tests/Documents/DocumentShapeTests.cs:10-168`

_(no documentation)_

**Members**

- `public void AddShape_TwoShapes_AreStackedBottomFirstInTheRightLayer()` - _(no documentation)_
- `public void AddShape_Shape_LeavesOriginalUnchangedAndSharesOtherLayers()` - _(no documentation)_
- `public void AddShape_DuplicateIdInOtherLayer_ThrowsAndLeavesDocumentUnchanged()` - _(no documentation)_
- `public void AddShape_UnknownLayer_Throws()` - _(no documentation)_
- `public void AddShape_Null_Throws()` - _(no documentation)_
- `public void UpdateShape_GeometryAndAppearance_ReplacesShapeAndKeepsItsPlace()` - _(no documentation)_
- `public void UpdateShape_ChangeToOtherShapeType_IsAllowedWithSameId()` - _(no documentation)_
- `public void UpdateShape_UnknownId_Throws()` - _(no documentation)_
- `public void UpdateShape_ChangeReturnsOtherId_Throws()` - _(no documentation)_
- `public void UpdateShape_ChangeReturnsNull_Throws()` - _(no documentation)_
- `public void UpdateShape_NullChange_Throws()` - _(no documentation)_
- `public void RemoveShape_ExistingShape_RemovesOnlyThatShape()` - _(no documentation)_
- `public void RemoveShape_UnknownId_Throws()` - _(no documentation)_
- `public void FindShape_ExistingShape_ReturnsShapeAndItsLayer()` - _(no documentation)_
- `public void FindShape_UnknownId_ReturnsNullWithoutThrowing()` - _(no documentation)_
- `public void FindShape_AfterRemove_ReturnsNull()` - _(no documentation)_

## FlagOperationsTests (class)

`public class FlagOperationsTests`

Source: `tests/PressToShape.Core.Tests/Documents/FlagOperationsTests.cs:10-243`

_(no documentation)_

**Members**

- `public void SetHidden_Leaf_HidesOnlyThatLeaf()` - _(no documentation)_
- `public void SetHidden_False_ShowsAgain()` - _(no documentation)_
- `public void SetHidden_Group_ChangesOnlyTheGroupsOwnFlag()` - _(no documentation)_
- `public void SetHidden_GroupAndItsChildTogether_ChangesBoth()` - _(no documentation)_
- `public void SetHidden_KeepsIdsAndBoxOfGroups()` - _(no documentation)_
- `public void SetLocked_AndUnlock_Work()` - _(no documentation)_
- `public void SetLocked_Group_ChildIsEffectivelyLockedButKeepsItsFlag()` - _(no documentation)_
- `public void SetHiddenAndLocked_NoChange_ReturnSameDocument()` - _(no documentation)_
- `public void SetHiddenAndLocked_UnknownId_Throws()` - _(no documentation)_
- `public void ShowAll_NestedHiddenShapes_AreShown()` - _(no documentation)_
- `public void UnlockAll_NestedLockedShapes_AreUnlocked()` - _(no documentation)_
- `public void ShowAllAndUnlockAll_NothingToChange_ReturnSameDocument()` - _(no documentation)_
- `public void ShowAllAndUnlockAll_DoNotTouchLayers()` - _(no documentation)_
- `public void Rename_Shape_SetsTrimmedName()` - _(no documentation)_
- `public void Rename_EmptyOrBlank_ClearsTheName(string name)` - _(no documentation)_
- `public void Rename_SameName_ReturnsSameDocument()` - _(no documentation)_
- `public void Rename_GroupInsideGroup_Works()` - _(no documentation)_
- `public void Rename_UnknownIdOrNullName_Throws()` - _(no documentation)_
- `public void FlagOperations_Result_LeaveOriginalUnchanged()` - _(no documentation)_

## GroupOperationsTests (class)

`public class GroupOperationsTests`

Source: `tests/PressToShape.Core.Tests/Documents/GroupOperationsTests.cs:11-247`

_(no documentation)_

**Members**

- `public void Group_TwoOfThree_ReplacesAtTopmostPositionKeepingOrder()` - _(no documentation)_
- `public void Group_TopMembersFirst_GroupStaysOnTop()` - _(no documentation)_
- `public void Group_GroupAndShape_NestsGroup()` - _(no documentation)_
- `public void Group_MembersOfOneGroup_GroupsInsideThatGroup()` - _(no documentation)_
- `public void Group_DifferentLayers_Throws()` - _(no documentation)_
- `public void Group_DifferentParents_Throws()` - _(no documentation)_
- `public void Group_OneIdOrRepeatedId_Throws()` - _(no documentation)_
- `public void Group_UnknownId_Throws()` - _(no documentation)_
- `public void Group_GroupIdAlreadyUsed_Throws()` - _(no documentation)_
- `public void Group_Result_LeavesOriginalUnchanged()` - _(no documentation)_
- `public void Ungroup_Group_ReplacesByChildrenInPlace()` - _(no documentation)_
- `public void Ungroup_ChildrenKeepTheirOwnFlagsAndGroupFlagsAreDropped()` - _(no documentation)_
- `public void Ungroup_SeveralGroups_UngroupsEach()` - _(no documentation)_
- `public void Ungroup_OuterGroup_KeepsInnerGroup()` - _(no documentation)_
- `public void Ungroup_InnerGroup_StaysInsideOuter()` - _(no documentation)_
- `public void Ungroup_PlainShape_Throws()` - _(no documentation)_
- `public void Ungroup_UnknownId_Throws()` - _(no documentation)_
- `public void Ungroup_NoIds_ReturnsSameDocument()` - _(no documentation)_
- `public void Ungroup_AfterGroup_MembersMoveToTheTopmostPosition()` - _(no documentation)_

## LayerOperationsTests (class)

`public class LayerOperationsTests`

Source: `tests/PressToShape.Core.Tests/Documents/LayerOperationsTests.cs:10-237`

_(no documentation)_

**Members**

- `public void DuplicateLayer_Layer_InsertsCopyAboveWithSameFlagsAndLook()` - _(no documentation)_
- `public void DuplicateLayer_LayerWithGroup_GivesNewIdsToEveryShapeAndGroup()` - _(no documentation)_
- `public void DuplicateLayer_EmptyLayer_InsertsEmptyCopy()` - _(no documentation)_
- `public void DuplicateLayer_UnknownLayer_Throws()` - _(no documentation)_
- `public void MergeLayerDown_TwoLayers_PutsUpperShapesOnTopOfLowerAndKeepsLowerLayer()` - _(no documentation)_
- `public void MergeLayerDown_UpperLayerWithGroup_KeepsGroup()` - _(no documentation)_
- `public void MergeLayerDown_BottomLayer_Throws()` - _(no documentation)_
- `public void MergeLayerDown_UnknownLayer_Throws()` - _(no documentation)_
- `public void MoveShapesToLayer_OneShape_GoesToTopOfTargetLayer()` - _(no documentation)_
- `public void MoveShapesToLayer_ShapesFromTwoLayers_KeepDocumentOrder()` - _(no documentation)_
- `public void MoveShapesToLayer_MemberOfGroup_LeavesGroupAndGroupStays()` - _(no documentation)_
- `public void MoveShapesToLayer_AllMembersOfGroup_RemovesEmptyGroup()` - _(no documentation)_
- `public void MoveShapesToLayer_GroupAndItsMember_MovesGroupOnce()` - _(no documentation)_
- `public void MoveShapesToLayer_IntoOwnLayer_GoesToTop()` - _(no documentation)_
- `public void MoveShapesToLayer_NoIds_ReturnsSameDocument()` - _(no documentation)_
- `public void MoveShapesToLayer_UnknownIds_Throw()` - _(no documentation)_
- `public void LayerOperations_Applied_LeaveOriginalDocumentUnchanged()` - _(no documentation)_

## LayerReorderTests (class)

`public class LayerReorderTests`

Source: `tests/PressToShape.Core.Tests/Documents/LayerReorderTests.cs:10-159`

_(no documentation)_

**Members**

- `public void InsertLayer_AtIndex_PlacesTheLayer(int index, string[] expected)` - _(no documentation)_
- `public void InsertLayer_BadIndex_Throws(int index)` - _(no documentation)_
- `public void InsertLayer_ExistingId_Throws()` - _(no documentation)_
- `public void MoveLayerToIndex_Up_SwapsWithNeighbour()` - _(no documentation)_
- `public void MoveLayerToIndex_ToBottom_PlacesAtBottom()` - _(no documentation)_
- `public void MoveLayerToIndex_SameIndex_ReturnsSameDocument()` - _(no documentation)_
- `public void MoveLayerToIndex_UnknownId_Throws()` - _(no documentation)_
- `public void MoveLayerToIndex_BadIndex_Throws(int index)` - _(no documentation)_
- `public void MoveLayerAbove_LayerBelowTarget_GoesAboveTarget()` - _(no documentation)_
- `public void MoveLayerAbove_LayerAboveTarget_GoesDirectlyAboveTarget()` - _(no documentation)_
- `public void MoveLayerAbove_AlreadyDirectlyAbove_ReturnsSameDocument()` - _(no documentation)_
- `public void MoveLayerAbove_SameLayer_Throws()` - _(no documentation)_
- `public void MoveLayerAbove_UnknownId_Throws()` - _(no documentation)_
- `public void MoveLayerToIndex_Moved_KeepsLayersFlagsAndShapesAndOriginal()` - _(no documentation)_

## LayerTests (class)

`public class LayerTests`

Source: `tests/PressToShape.Core.Tests/Documents/LayerTests.cs:9-98`

_(no documentation)_

**Members**

- `public void Constructor_Defaults_VisibleUnlockedAndEmpty()` - _(no documentation)_
- `public void Constructor_NoId_GetsNonEmptyUniqueIds()` - _(no documentation)_
- `public void Name_EmptyString_IsAllowed()` - _(no documentation)_
- `public void Name_Null_ThrowsOnCreationAndWith()` - _(no documentation)_
- `public void Id_Empty_Throws()` - _(no documentation)_
- `public void Shapes_DefaultArray_BecomesEmpty()` - _(no documentation)_
- `public void With_NameVisibilityAndLock_ChangesCopyAndLeavesOriginalUnchanged()` - _(no documentation)_
- `public void With_Shapes_KeepsGivenShapesInOrder()` - _(no documentation)_
- `public void ShapeLocation_LayerAndShape_AreKept()` - _(no documentation)_

## PathShapeOperationsBooleanTests (class)

`public class PathShapeOperationsBooleanTests`

Source: `tests/PressToShape.Core.Tests/Documents/PathShapeOperationsBooleanTests.cs:12-297`

_(no documentation)_

**Members**

- `public void Boolean_TwoRectangles_ReplacesThemWithOnePath()` - _(no documentation)_
- `public void Boolean_TakesTheAppearanceNameAndFlagsOfTheBottomOperand()` - _(no documentation)_
- `public void Boolean_OperandsGivenInAnyOrder_BottomOneIsTheLowestInTheDocument()` - _(no documentation)_
- `public void Boolean_ResultTakesThePlaceOfTheBottomOperandInTheStack()` - _(no documentation)_
- `public void Boolean_OperandsInDifferentLayers_ResultIsInTheLayerOfTheBottomOperand()` - _(no documentation)_
- `public void Boolean_BottomOperandInAGroup_ResultStaysInThatGroup()` - _(no documentation)_
- `public void Boolean_OtherOperandWasTheOnlyChildOfAGroup_TheEmptyGroupGoes()` - _(no documentation)_
- `public void Boolean_RotatedAndRoundedOperands_UseWhatYouSee()` - _(no documentation)_
- `public void Boolean_EllipseAndRectangle_KeepsTheCurve()` - _(no documentation)_
- `public void Boolean_EmptyResult_FailsWithEmptyAndLeavesTheDocument()` - _(no documentation)_
- `public void Boolean_Difference_CutsUpperOperandsFromTheBottomOne()` - _(no documentation)_
- `public void Boolean_Undo_RestoresTheOperandsAndTheirOrder()` - _(no documentation)_
- `public void Boolean_DoesNotChangeTheInputDocument()` - _(no documentation)_
- `public void Boolean_FewerThanTwoOrRepeatedIds_IsInvalid()` - _(no documentation)_
- `public void Boolean_Group_IsInvalid()` - _(no documentation)_
- `public void Boolean_OpenPath_IsInvalid()` - _(no documentation)_
- `public void Boolean_ClosedPathOperand_IsAccepted()` - _(no documentation)_
- `public void Boolean_LockedOrHiddenOperand_IsInvalid()` - _(no documentation)_
- `public void Boolean_OperandOfALockedLayerOrGroup_IsInvalid()` - _(no documentation)_
- `public void Boolean_CancelledToken_Throws()` - _(no documentation)_

## PathShapeOperationsCombineTests (class)

`public class PathShapeOperationsCombineTests`

Source: `tests/PressToShape.Core.Tests/Documents/PathShapeOperationsCombineTests.cs:10-210`

_(no documentation)_

**Members**

- `public void Combine_BigAndSmallCircle_ShowsAHoleInTheMiddle()` - _(no documentation)_
- `public void Combine_KeepsTheNodesInDocumentCoordinatesAndTakesTheBottomAppearance()` - _(no documentation)_
- `public void Combine_RotatedOperand_IsCombinedAsItLooks()` - _(no documentation)_
- `public void Combine_GroupHiddenOrFewerThanTwo_IsInvalid()` - _(no documentation)_
- `public void BreakApart_CompoundPath_GivesOnePathPerSubPathInOrder()` - _(no documentation)_
- `public void BreakApart_KeepsAppearanceNameFillRuleAndRotation()` - _(no documentation)_
- `public void BreakApart_RotatedPath_EveryNodeStaysWhereItWasOnTheScreen()` - _(no documentation)_
- `public void BreakApart_PathInAGroup_PiecesStayInTheGroup()` - _(no documentation)_
- `public void BreakApart_OneSubPathOtherShapeOrLockedOrUnknown_IsInvalid()` - _(no documentation)_
- `public void CombineThenBreakApart_GivesBackTheShapesAsPaths()` - _(no documentation)_
- `public void Operations_NullArguments_Throw()` - _(no documentation)_

## PathShapeOperationsConvertTests (class)

`public class PathShapeOperationsConvertTests`

Source: `tests/PressToShape.Core.Tests/Documents/PathShapeOperationsConvertTests.cs:9-155`

_(no documentation)_

**Members**

- `public void ConvertToPaths_RectangleAndEllipse_BecomePathsWithTheSameIdsAndPlaces()` - _(no documentation)_
- `public void ConvertToPaths_RoundedRectangle_LooksTheSame()` - _(no documentation)_
- `public void ConvertToPaths_SelectedGroup_ConvertsItsShapesAndKeepsTheGroup()` - _(no documentation)_
- `public void ConvertToPaths_NestedGroups_AreReached()` - _(no documentation)_
- `public void ConvertToPaths_LockedAndHiddenShapes_AreSkipped()` - _(no documentation)_
- `public void ConvertToPaths_ShapesInALockedGroupOrLayer_AreSkipped()` - _(no documentation)_
- `public void ConvertToPaths_PathsGroupsWithoutShapesAndUnknownIds_AreIgnored_AndTheSameDocumentComesBack()` - _(no documentation)_
- `public void ConvertibleShapeIds_ListsEachConvertibleShapeOnce()` - _(no documentation)_
- `public void ConvertToPaths_DoesNotChangeTheInput_AndNullsThrow()` - _(no documentation)_

## PathShapeOperationsReverseTests (class)

`public class PathShapeOperationsReverseTests`

Source: `tests/PressToShape.Core.Tests/Documents/PathShapeOperationsReverseTests.cs:9-103`

_(no documentation)_

**Members**

- `public void ReverseDirections_Path_IsReversedWithTheSameIdAndPlace()` - _(no documentation)_
- `public void ReverseDirections_PathInASelectedGroup_IsReversed()` - _(no documentation)_
- `public void ReverseDirections_RectanglesEllipsesLockedAndHidden_AreIgnored_AndTheSameDocumentComesBack()` - _(no documentation)_
- `public void ReversibleShapeIds_ListsEachPathOnce_AndSelectedGroupsGiveTheirPaths()` - _(no documentation)_
- `public void ReverseDirections_Twice_GivesTheOriginalFirstNode()` - _(no documentation)_
- `public void ReverseDirections_DoesNotChangeTheInput_AndNullsThrow()` - _(no documentation)_

## ShapeTreeTests (class)

`public class ShapeTreeTests`

Source: `tests/PressToShape.Core.Tests/Documents/ShapeTreeTests.cs:12-249`

_(no documentation)_

**Members**

- `public void Leaves_PlainShape_ReturnsItself()` - _(no documentation)_
- `public void Leaves_NestedGroups_ReturnsLeavesBottomFirst()` - _(no documentation)_
- `public void Find_ShapeInNestedGroup_ReturnsParentChain()` - _(no documentation)_
- `public void Find_TopLevelShape_HasNoParent()` - _(no documentation)_
- `public void Find_UnknownId_ReturnsNull()` - _(no documentation)_
- `public void Replace_Leaf_RebuildsAncestorBoxes()` - _(no documentation)_
- `public void Replace_NoMatch_ReturnsSameArray()` - _(no documentation)_
- `public void Remove_OneOfTwoChildren_UpdatesGroupBox()` - _(no documentation)_
- `public void Remove_LastChild_RemovesGroupAndEmptyAncestors()` - _(no documentation)_
- `public void Remove_GroupAndItsChild_CountsBoth()` - _(no documentation)_
- `public void Remove_UnknownId_MatchesNothing()` - _(no documentation)_
- `public void Reidentify_NestedGroup_GivesFreshUniqueIds()` - _(no documentation)_
- `public void Translate_Group_MovesAllLeavesAndBox()` - _(no documentation)_
- `public void EffectiveState_HiddenGroup_HidesChild()` - _(no documentation)_
- `public void EffectiveState_LockedGroup_LocksChildButChildFlagStays()` - _(no documentation)_
- `public void Depth_NestedGroups_CountsLevels()` - _(no documentation)_
- `public void Walks_64Levels_Work()` - _(no documentation)_
- `public void Walks_65Levels_Throw()` - _(no documentation)_
