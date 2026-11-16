# PressToShape.App.Tests

Project: PressToShape.App.Tests

## AppArchitectureTests (class)

`public class AppArchitectureTests`

Source: `tests/PressToShape.App.Tests/AppArchitectureTests.cs:11-61`

_(no documentation)_

**Members**

- `public void App_DoesNotReference_PlatformHeads(string head)` - _(no documentation)_
- `public void OnlyEditorState_AssignsTheDocument()` - _(no documentation)_
- `public void DocumentAssignmentPattern_FlagsAssignmentsOnly(string line, bool expected)` - _(no documentation)_

## AppLayoutTests (class)

`public class AppLayoutTests`

Source: `tests/PressToShape.App.Tests/AppLayoutTests.cs:17-145`

_(no documentation)_

**Members**

- `public void AnApplicationWithAWindow_GetsTheDesktopLayout()` - _(no documentation)_
- `public void WithoutALifetime_NothingHappens()` - _(no documentation)_
- `public void TheTabletFactory_BuildsTheTabletLayout_OnlyWhenCalled()` - _(no documentation)_
- `public void EveryCallOfTheFactory_GivesAnIndependentLayout()` - _(no documentation)_
- `public void LeavingTheTabletLayout_CallsTheExitThatTheHeadSetLate()` - _(no documentation)_
- `public void LeavingBeforeTheHeadSetTheExit_DoesNotCrash()` - _(no documentation)_
- `public void TheTabletLayout_ShowsItsDialogsInsideTheLayout_NotInAWindow()` - _(no documentation)_
- `public void TheTabletLayout_UsesTheGivenSettings_ForTheLanguageAndNotTheRealFile()` - _(no documentation)_
- `public void NullArguments_Throw()` - _(no documentation)_

## AvaloniaFileDialogsTests (class)

`public class AvaloniaFileDialogsTests`

Source: `tests/PressToShape.App.Tests/AvaloniaFileDialogsTests.cs:8-80`

_(no documentation)_

**Members**

- `public void ChooseReference_WithALocalPath_ReturnsThePathAndDoesNotRegister()` - _(no documentation)_
- `public void ChooseReference_WithoutAPath_RegistersAndReturnsTheReference()` - _(no documentation)_
- `public void ChooseReference_WithoutAPathAndWithoutRegistry_ReturnsNull()` - _(no documentation)_
- `public void BuildSaveOptions_SuggestedNameWithTheEnding_PassesNoDefaultExtension(string suggested, string extension)` - _(no documentation)_
- `public void BuildSaveOptions_SuggestedNameWithoutTheEnding_KeepsTheDefaultExtension(string suggested, string extension)` - _(no documentation)_
- `public void BuildSaveOptions_SetsTitleOverwritePromptAndTheFileType()` - _(no documentation)_
- `public void Constructor_NullOwnerOrLocalizer_Throws()` - _(no documentation)_

## BackRequestGuardTests (class)

`public class BackRequestGuardTests`

Source: `tests/PressToShape.App.Tests/BackRequestGuardTests.cs:6-53`

_(no documentation)_

**Members**

- `public void ConsumeIfSamePress_WithoutAClose_IsFalse()` - _(no documentation)_
- `public void ConsumeIfSamePress_RightAfterAClose_IsTrueOnce()` - _(no documentation)_
- `public void ConsumeIfSamePress_AtTheEndOfTheWindow_IsTrue()` - _(no documentation)_
- `public void ConsumeIfSamePress_AfterTheWindow_IsFalse()` - _(no documentation)_
- `public void NoteClosed_Again_StartsANewWindow()` - _(no documentation)_

## CanvasAreaViewModelDoubleTapTests (class)

`public class CanvasAreaViewModelDoubleTapTests`

Source: `tests/PressToShape.App.Tests/CanvasAreaViewModelDoubleTapTests.cs:12-160`

_(no documentation)_

**Members**

- `public void DoubleTap_OnAGroup_EntersItAndSelectsTheChildUnderTheFinger()` - _(no documentation)_
- `public void SingleTap_OnAGroup_OnlySelectsIt()` - _(no documentation)_
- `public void TwoTaps_TooFarApartInTime_DoNotEnter()` - _(no documentation)_
- `public void TwoTaps_OnDifferentSpots_DoNotEnter()` - _(no documentation)_
- `public void DoubleTap_OnEmptySpaceInsideAnEnteredGroup_LeavesIt()` - _(no documentation)_
- `public void TapBesideTheEnteredGroup_LeavesItWithoutADoubleTap()` - _(no documentation)_
- `public void TapThenADragOfTheSameShape_StillDragsAndIsNoDoubleTap()` - _(no documentation)_
- `public void DoubleTap_WithAnotherTool_DoesNothing()` - _(no documentation)_
- `public void TwoFingerGesture_BetweenTaps_BreaksTheDoubleTap()` - _(no documentation)_

## CanvasAreaViewModelLongPressTests (class)

`public class CanvasAreaViewModelLongPressTests`

Source: `tests/PressToShape.App.Tests/CanvasAreaViewModelLongPressTests.cs:12-226`

_(no documentation)_

**Members**

- `public void Tick_FingerHeldStill_OpensTheRingAtThePressAfterTheDelay()` - _(no documentation)_
- `public void Tick_BeforeTheDelay_DoesNothing()` - _(no documentation)_
- `public void Tick_FingerMovedPastTheSlop_DoesNotOpen()` - _(no documentation)_
- `public void Tick_FingerReleasedBeforeTheDelay_DoesNotOpen()` - _(no documentation)_
- `public void Tick_FingerTwitchedWithinTheSlop_UndoesTheMoveItStarted()` - _(no documentation)_
- `public void Release_AfterTheLongPress_KeepsTheRingOpenAndIsNoTap()` - _(no documentation)_
- `public void Tick_SecondFingerArrives_CancelsTheLongPress()` - _(no documentation)_
- `public void Tick_WithAnotherTool_DoesNotWatch()` - _(no documentation)_
- `public void Tick_Mouse_NeverLongPresses()` - _(no documentation)_
- `public void Tick_Pen_LongPressesLikeAFinger()` - _(no documentation)_
- `public void Tick_PenMovedPastTheSlop_DoesNotOpen()` - _(no documentation)_
- `public void Tick_NothingWatched_DoesNothing()` - _(no documentation)_
- `public void Tick_OnEmptyCanvas_OpensTheEmptyCanvasRing()` - _(no documentation)_

## CanvasAreaViewModelRingTests (class)

`public class CanvasAreaViewModelRingTests`

Source: `tests/PressToShape.App.Tests/CanvasAreaViewModelRingTests.cs:13-243`

_(no documentation)_

**Members**

- `public void OpenContextMenu_OnASelectedShape_OpensTheSelectionRingAndKeepsTheSelection()` - _(no documentation)_
- `public void OpenContextMenu_OnAnUnselectedShape_SelectsItFirst()` - _(no documentation)_
- `public void OpenContextMenu_OnEmptyCanvas_ClearsTheSelectionAndOpensTheEmptyCanvasRing()` - _(no documentation)_
- `public void OpenContextMenu_WhileOpen_MovesTheRingToTheNewPoint()` - _(no documentation)_
- `public void OpenContextMenu_WithAnotherTool_DoesNothing()` - _(no documentation)_
- `public void OpenContextMenu_DuringADrag_DoesNothing()` - _(no documentation)_
- `public void OpenContextMenu_DuringAPan_DoesNothing()` - _(no documentation)_
- `public void OpenContextMenu_DuringAColorPick_DoesNothing()` - _(no documentation)_
- `public void OpenContextMenu_WithoutARing_DoesNothing()` - _(no documentation)_
- `public void KeyPressed_ContextMenuWithASelection_OpensInTheMiddleOfTheSelectionWithTheFirstSlotFocused()` - _(no documentation)_
- `public void KeyPressed_ContextMenuWithoutASelection_OpensTheEmptyCanvasRingInTheMiddleOfTheViewport()` - _(no documentation)_
- `public void KeyPressed_ContextMenuWithAnotherTool_IsNotHandled()` - _(no documentation)_
- `public void SelectionChange_WhileOpen_ClosesTheRing()` - _(no documentation)_
- `public void DocumentChange_WhileOpen_ClosesTheRing()` - _(no documentation)_
- `public void OpenContextMenu_AddsNoHistoryStep()` - _(no documentation)_

## CanvasAreaViewModelTests (class)

`public class CanvasAreaViewModelTests`

Source: `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs:12-1209`

_(no documentation)_

**Members**

- `public void ArtboardScreenSize_FollowsZoom()` - _(no documentation)_
- `public void Artboard_IsCentered_WhenSmallerThanViewport()` - _(no documentation)_
- `public void Origin_StaysCentered_WhenViewportOrZoomChanges()` - _(no documentation)_
- `public void Origin_IsCenteredWithOverflowOnBothSides_WhenArtboardIsLargerThanViewport()` - _(no documentation)_
- `public void ZeroViewport_GivesFiniteValues_WithoutErrors()` - _(no documentation)_
- `public void ZoomChange_RaisesNotificationsForDerivedValues()` - _(no documentation)_
- `public void ViewportChange_RaisesOriginNotifications()` - _(no documentation)_
- `public void ArtboardSizeChange_UpdatesScreenSize()` - _(no documentation)_
- `public void Document_IsTheDocumentOfTheState()` - _(no documentation)_
- `public void DocumentChange_RaisesDocumentNotification()` - _(no documentation)_
- `public void NewArtboardSize_ChangesArtboardPropertiesAndOrigins()` - _(no documentation)_
- `public void ZoomChange_DoesNotRaiseDocumentNotification()` - _(no documentation)_
- `public void Pointer_DrawAtZoom200WithAnOrigin_CreatesTheShapeAtTheDocumentPosition()` - _(no documentation)_
- `public void Draft_DuringADrag_IsInScreenSpaceAndNullAfterwards()` - _(no documentation)_
- `public void DeleteSelection_SelectedShape_RemovesIt()` - _(no documentation)_
- `public void Cancel_DuringADrag_ClearsTheDraft()` - _(no documentation)_
- `public void Origin_FollowsTheViewOffset()` - _(no documentation)_
- `public void ViewportChanges_ReachTheStateAndLimitTheOffset()` - _(no documentation)_
- `public void Pointer_DrawAtAViewOffsetAndZoom_LandsUnderThePointer()` - _(no documentation)_
- `public void Wheel_WithoutModifier_MovesTheView(double deltaY, double deltaX, double expectedPanX, double expectedPanY)` - _(no documentation)_
- `public void Wheel_WithShift_MovesTheViewHorizontally()` - _(no documentation)_
- `public void Wheel_WithShiftAndAHorizontalDelta_KeepsTheHorizontalDelta()` - _(no documentation)_
- `public void Wheel_WithControl_ZoomsAroundThePointer()` - _(no documentation)_
- `public void Wheel_WithControlDown_ZoomsOut()` - _(no documentation)_
- `public void Wheel_WithControlAtTheZoomLimit_ChangesNothing()` - _(no documentation)_
- `public void Wheel_AtThePanLimit_ReturnsFalse()` - _(no documentation)_
- `public void Wheel_NonFiniteOrZeroDelta_IsIgnored(double deltaX, double deltaY)` - _(no documentation)_
- `public void Wheel_NonFinitePointerPosition_IsIgnored()` - _(no documentation)_
- `public void Wheel_WhileADragIsInProgress_IsIgnored()` - _(no documentation)_
- `public void Wheel_NeverTouchesTheDocumentOrTheHistory()` - _(no documentation)_
- `public void HandTool_Drag_FollowsThePointerOneToOne_AndCreatesNothing()` - _(no documentation)_
- `public void HandTool_PressOnAShape_DoesNotSelectOrMoveIt()` - _(no documentation)_
- `public void HandTool_DragFarAway_IsLimited_AndRecoversWhenTheDragComesBack()` - _(no documentation)_
- `public void Pan_Cancel_EndsItAndFurtherMovesDoNothing()` - _(no documentation)_
- `public void MiddleDrag_PansWithAnyTool_AndTheToolStays()` - _(no documentation)_
- `public void MiddlePress_WhileALeftDragIsInProgress_IsIgnored()` - _(no documentation)_
- `public void Pan_BlocksUndoAndTheHistoryUntilItEnds()` - _(no documentation)_
- `public void ChangingTheToolDuringAPan_EndsIt()` - _(no documentation)_
- `public void ZoomTool_Click_ZoomsInOneStepAroundTheClickedPoint()` - _(no documentation)_
- `public void ZoomTool_AltClick_ZoomsOutOneStep()` - _(no documentation)_
- `public void ZoomTool_AtTheLimits_ChangesNothing()` - _(no documentation)_
- `public void ZoomTool_Click_DoesNotSelectOrDrawAnything()` - _(no documentation)_
- `public void CursorType_FollowsTheActiveTool()` - _(no documentation)_
- `public void CursorType_EyedropperToolOrAPick_IsTheEyedropper_AndRaisesPropertyChanged()` - _(no documentation)_
- `public void CursorType_ZoomToolWithAlt_IsTheZoomOutMagnifier_AndBackWhenAltIsReleased()` - _(no documentation)_
- `public void IsAltHeld_SetToTheSameValue_RaisesNothing()` - _(no documentation)_
- `public void CursorType_OtherToolsWithAlt_IgnoreAlt(string tool, string expected)` - _(no documentation)_
- `public void CursorType_AHandleUnderThePointerWithAlt_IgnoresAlt()` - _(no documentation)_
- `public void CursorType_SelectingTheZoomToolWhileAltIsHeld_ShowsTheMinusAtOnce()` - _(no documentation)_
- `public void Pointer_MovePressAndRelease_TakeTheAltStateFromTheirArgument()` - _(no documentation)_
- `public void Pointer_ReleaseAfterAPanWithAlt_KeepsAltHeld()` - _(no documentation)_
- `public void ZoomTool_AltHeld_StillZoomsOutOnClick()` - _(no documentation)_
- `public void CursorType_DuringAPan_IsAClosedHand_AndBackAfterwards()` - _(no documentation)_
- `public void Drawing_AfterPanningAndZooming_StillLandsUnderThePointer()` - _(no documentation)_
- `public void ScrollRange_IsTheAllowedOffsetRange_AndFollowsZoomViewportAndArtboard()` - _(no documentation)_
- `public void ScrollValue_IsTheNegatedOffset_AndSettingItPansTheOtherWay()` - _(no documentation)_
- `public void ScrollValue_SetOutsideTheRange_IsLimited()` - _(no documentation)_
- `public void ScrollValue_AtTheOffsetLimit_IsAtTheEndOfTheRange()` - _(no documentation)_
- `public void ScrollBars_WithoutACanvasSize_CannotScroll()` - _(no documentation)_
- `public void ScrollProperties_RaiseNotificationsForWheelDragZoomAndResize()` - _(no documentation)_
- `public void ScrollValue_SetToTheCurrentValue_ChangesNothing()` - _(no documentation)_
- `public void ScrollBars_NeverCreateHistoryOrDirtyState()` - _(no documentation)_
- `public void Overlay_OneShape_HasOutlineFrameAndHandles_AndFollowsZoomAndOrigin()` - _(no documentation)_
- `public void Overlay_SeveralOrNoShapesOrOtherTools()` - _(no documentation)_
- `public void Overlay_Notifies_OnSelectionToolSelectionBoxViewportAndDocumentChange()` - _(no documentation)_
- `public void Modifiers_ReachTheInteraction_ShiftAddsToTheSelection_AndShiftKeepsTheRatio()` - _(no documentation)_
- `public void Hover_OverEachHandle_SetsTheMatchingCursor_AndAwayResetsIt()` - _(no documentation)_
- `public void Hover_OverARotatedShapesHandle_TurnsTheResizeCursor()` - _(no documentation)_
- `public void Cursor_DuringAHandleDrag_StaysTheHandlesCursor_EvenAwayFromIt()` - _(no documentation)_
- `public void Cursor_OtherToolsNeverShowHandleCursors()` - _(no documentation)_
- `public void PointerLeft_ClearsTheHover()` - _(no documentation)_
- `public void ArrowKeys_NudgeByOne_ShiftByTen_AndRepeatedKeysMergeIntoOneStep()` - _(no documentation)_
- `public void Escape_ClearsTheSelection()` - _(no documentation)_
- `public void Keys_DoNothingWithoutASelectionOrDuringADrag()` - _(no documentation)_
- `public void DeleteSelection_OfSeveralShapes_RemovesAllOfThem()` - _(no documentation)_
- `public void SharedEditor_IsUsedForKeys()` - _(no documentation)_

## CanvasAreaViewModelTouchTests (class)

`public class CanvasAreaViewModelTouchTests`

Source: `tests/PressToShape.App.Tests/CanvasAreaViewModelTouchTests.cs:12-400`

_(no documentation)_

**Members**

- `public void OneFinger_DrawsARectangle_AsOneHistoryStep()` - _(no documentation)_
- `public void OneFinger_WithTheSelectTool_MovesTheShape()` - _(no documentation)_
- `public void OneFinger_WithTheHandTool_PansTheView()` - _(no documentation)_
- `public void AfterAFingerIsUp_TheNextFingerDrawsAgain_WithAnotherId()` - _(no documentation)_
- `public void SecondFinger_DuringDrawing_LeavesNoShapeAndNoStep()` - _(no documentation)_
- `public void SecondFinger_DuringAMove_PutsTheShapeBackAndRecordsNothing()` - _(no documentation)_
- `public void SecondFinger_DuringAResize_PutsTheShapeBack()` - _(no documentation)_
- `public void SecondFinger_WithTheHandTool_EndsThePanAndStartsTheGesture()` - _(no documentation)_
- `public void Pinch_ZoomsAroundTheMidpointOfTheFingers()` - _(no documentation)_
- `public void Pinch_MovingTogether_ZoomsOut()` - _(no documentation)_
- `public void TwoFingersMovingTogether_PanTheView()` - _(no documentation)_
- `public void Pinch_IsKeptInsideTheZoomRange()` - _(no documentation)_
- `public void WhileTwoFingersMoveTheView_UndoIsBlocked_AndTheDocumentIsUntouched()` - _(no documentation)_
- `public void AfterTheGesture_TheLiftingFingerDoesNotDraw_ButTheNextTouchDoes()` - _(no documentation)_
- `public void AThirdFinger_DoesNotDisturbTheGesture()` - _(no documentation)_
- `public void TouchCancelled_DuringAMove_PutsTheShapeBackAndForgetsTheFinger()` - _(no documentation)_
- `public void TouchCancelled_AfterANormalRelease_ChangesNothing()` - _(no documentation)_
- `public void TouchCancelled_DuringAGesture_EndsItAndFreesTheHistory()` - _(no documentation)_
- `public void Finger_FifteenPixelsFromAHandle_GrabsIt_ButTheMouseDoesNot()` - _(no documentation)_
- `public void TouchGrabSize_CanBeChanged()` - _(no documentation)_
- `public void Mouse_StillDrawsWithTheExistingMethods()` - _(no documentation)_

## CanvasInteractionContextMenuTests (class)

`public class CanvasInteractionContextMenuTests`

Source: `tests/PressToShape.App.Tests/CanvasInteractionContextMenuTests.cs:9-129`

_(no documentation)_

**Members**

- `public void SelectForContextMenu_OnASelectedObject_KeepsTheWholeSelection()` - _(no documentation)_
- `public void SelectForContextMenu_OnAnotherObject_SelectsOnlyIt()` - _(no documentation)_
- `public void SelectForContextMenu_OnAMemberOfAGroup_SelectsTheGroup()` - _(no documentation)_
- `public void SelectForContextMenu_InAnEnteredGroup_SelectsTheMember()` - _(no documentation)_
- `public void SelectForContextMenu_OnEmptyCanvas_ClearsTheSelection()` - _(no documentation)_
- `public void SelectForContextMenu_OnAHiddenOrLockedObject_CountsAsEmptyCanvas(double x)` - _(no documentation)_
- `public void SelectForContextMenu_OnAHandleOfTheSelection_KeepsTheSelection()` - _(no documentation)_
- `public void SelectForContextMenu_AddsNoHistoryStep()` - _(no documentation)_

## CanvasInteractionGroupTests (class)

`public class CanvasInteractionGroupTests`

Source: `tests/PressToShape.App.Tests/CanvasInteractionGroupTests.cs:13-327`

_(no documentation)_

**Members**

- `public void Click_OnAMember_SelectsTheOutermostGroup()` - _(no documentation)_
- `public void Click_InTheGapOfAGroup_SelectsNothing()` - _(no documentation)_
- `public void ShiftClick_OnAMemberAndAPlainShape_TogglesTheGroupAndTheShape()` - _(no documentation)_
- `public void DoubleClick_OnAGroup_EntersItAndSelectsTheChildUnderThePointer()` - _(no documentation)_
- `public void Click_AfterEntering_SelectsChildrenAndIgnoresShapesOutsideTheGroup()` - _(no documentation)_
- `public void DoubleClick_OnAPlainShape_DoesNothing()` - _(no documentation)_
- `public void DoubleClick_OnANestedGroupInsideTheEnteredGroup_EntersTheInnerGroup()` - _(no documentation)_
- `public void DoubleClick_OnEmptySpaceInsideAnEnteredGroup_LeavesIt()` - _(no documentation)_
- `public void Click_OnEmptySpaceBesideTheEnteredGroup_LeavesIt()` - _(no documentation)_
- `public void Click_OnEmptySpaceInsideTheBoxOfTheEnteredGroup_OnlyClearsTheSelection()` - _(no documentation)_
- `public void DoubleClick_WithAnotherTool_DoesNothing()` - _(no documentation)_
- `public void Move_Group_IsOneStepAndMovesAllLeaves()` - _(no documentation)_
- `public void Move_Group_UndoRestoresTheDocument()` - _(no documentation)_
- `public void Resize_Group_IsOneStepAndKeepsTheLayout()` - _(no documentation)_
- `public void Rotate_Group_IsOneStepAndTheGroupStaysUpright()` - _(no documentation)_
- `public void Rubberband_AtTopLevel_SelectsGroupsNotMembers()` - _(no documentation)_
- `public void Rubberband_InsideAnEnteredGroup_SelectsItsChildren()` - _(no documentation)_
- `public void Click_OnALockedOrHiddenShape_SelectsWhatIsBelow()` - _(no documentation)_
- `public void Click_OnAMemberOfALockedGroup_SelectsNothing()` - _(no documentation)_
- `public void DeleteSelection_Group_RemovesTheWholeGroupAsOneStep()` - _(no documentation)_
- `public void Escape_InAnEnteredGroup_LeavesIt()` - _(no documentation)_

## CanvasInteractionTests (class)

`public class CanvasInteractionTests`

Source: `tests/PressToShape.App.Tests/CanvasInteractionTests.cs:11-1586`

_(no documentation)_

**Members**

- `public void Draw_Rectangle_AddsARectangleOnTheActiveLayer()` - _(no documentation)_
- `public void Draw_Ellipse_AddsAnEllipse()` - _(no documentation)_
- `public void Draw_AnyDragDirection_GivesTheNormalizedRectangle(double x1, double y1, double x2, double y2)` - _(no documentation)_
- `public void Draw_UsesTheCoreDefaultAppearanceAndANewId()` - _(no documentation)_
- `public void Draw_Twice_PutsTheNewShapeOnTop()` - _(no documentation)_
- `public void Draw_Finished_SelectsTheNewShapeAndKeepsTheTool()` - _(no documentation)_
- `public void Draw_InTheMiddleOfTheDrag_ShowsTheDraftOutsideTheDocument()` - _(no documentation)_
- `public void Draw_SmallerThanTwoScreenPixels_AddsNothing(double zoom, double width, double height)` - _(no documentation)_
- `public void Draw_ExactlyTwoScreenPixels_AddsTheShape(double zoom, double width, double height)` - _(no documentation)_
- `public void Draw_HiddenActiveLayer_CreatesNothing()` - _(no documentation)_
- `public void Draw_LockedActiveLayer_CreatesNothing()` - _(no documentation)_
- `public void Draw_LayerLockedDuringTheDrag_CreatesNothing()` - _(no documentation)_
- `public void Draw_OnTheOtherLayerWhenItIsActive_UsesThatLayer()` - _(no documentation)_
- `public void Draw_OutsideTheArtboard_IsCreatedAtTheGivenPosition()` - _(no documentation)_
- `public void Draw_ToolChangedDuringTheDrag_DropsTheDraftAndAddsNothing()` - _(no documentation)_
- `public void Cancel_DuringDrawing_DropsTheDraft()` - _(no documentation)_
- `public void OtherTools_Drag_DoNothing()` - _(no documentation)_
- `public void MoveAndRelease_WithoutPress_DoNothing()` - _(no documentation)_
- `public void Constructor_NullState_Throws()` - _(no documentation)_
- `public void Select_ClickOnOverlap_SelectsTheTopmostShape()` - _(no documentation)_
- `public void Select_ClickOnTheBackShapeOnly_SelectsIt()` - _(no documentation)_
- `public void Select_ClickInTheCornerOfAnEllipse_Misses()` - _(no documentation)_
- `public void Select_ClickOnEmptySpace_ClearsTheSelection()` - _(no documentation)_
- `public void Select_TopShapeInLockedLayer_SelectsTheUnlockedShapeBelow()` - _(no documentation)_
- `public void Select_OnlyLockedShapeUnderThePointer_ClearsTheSelection()` - _(no documentation)_
- `public void Select_ShapeInHiddenLayer_IsSkipped()` - _(no documentation)_
- `public void Move_DragSelectedShape_FollowsThePointerByTheSameDocumentDistance(double zoom)` - _(no documentation)_
- `public void Move_ManySmallMoves_DoNotDrift()` - _(no documentation)_
- `public void Move_OnlyThePositionChanges()` - _(no documentation)_
- `public void Move_EllipseKeepsItsType()` - _(no documentation)_
- `public void Move_PointerFarOutsideTheCanvas_StillFollows()` - _(no documentation)_
- `public void Move_PressOnEmptySpaceAndDrag_MovesNothing()` - _(no documentation)_
- `public void Move_ShapeInLockedLayer_IsNotMoved()` - _(no documentation)_
- `public void Move_Cancel_KeepsTheReachedPositionAndEndsTheDrag()` - _(no documentation)_
- `public void DeleteSelection_SelectedShape_RemovesItAndClearsTheSelection()` - _(no documentation)_
- `public void DeleteSelection_NothingSelected_DoesNothing()` - _(no documentation)_
- `public void Draw_Rectangle_IsOneStepOfTheRightType()` - _(no documentation)_
- `public void Draw_Ellipse_IsOneStepOfTheRightType()` - _(no documentation)_
- `public void Draw_TooSmallOrBlockedOrCancelled_IsNoStep()` - _(no documentation)_
- `public void Move_ManyPointerMoves_AreOneMoveStep_AndUndoGoesBackToTheStart()` - _(no documentation)_
- `public void Move_ClickWithoutMovement_IsNoStep_AndKeepsTheDocumentInstance()` - _(no documentation)_
- `public void Move_PressOnEmptySpaceAndDrag_IsNoStep()` - _(no documentation)_
- `public void Move_CancelAfterMovement_IsOneStepWithTheReachedPosition()` - _(no documentation)_
- `public void Move_ToolChangeAfterMovement_IsOneStep()` - _(no documentation)_
- `public void Move_CancelWithoutMovement_IsNoStep()` - _(no documentation)_
- `public void Delete_IsOneStep_AndUndoBringsTheShapeBack()` - _(no documentation)_
- `public void Delete_WithoutSelection_IsNoStep()` - _(no documentation)_
- `public void DragFlag_IsSetDuringEveryDragAndClearedAtTheEnd()` - _(no documentation)_
- `public void Undo_DuringADrag_IsRefused()` - _(no documentation)_
- `public void ShiftClick_AddsAndRemovesShapes_AndMovesNothing()` - _(no documentation)_
- `public void ShiftPress_OnASelectedShape_RemovesItAndADragAfterwardsMovesNothing()` - _(no documentation)_
- `public void ClickOnEmptySpace_ClearsTheSelection_ButShiftClickKeepsIt()` - _(no documentation)_
- `public void ClickOnAnUnselectedShape_SelectsOnlyIt()` - _(no documentation)_
- `public void PressOnASelectedShape_KeepsTheSelection_AndMovesAllOfThemAsOneStep()` - _(no documentation)_
- `public void ClickOnASelectedShape_WithoutMovement_MakesNoStep_AndKeepsTheOthers()` - _(no documentation)_
- `public void MovingBackToTheStart_RestoresThePositionsWithoutAStep()` - _(no documentation)_
- `public void MovingSeveralShapes_UsesDocumentUnits_AtAnyZoom(double zoom)` - _(no documentation)_
- `public void SelectionBox_SelectsTouchedShapes_AndShowsTheBoxWhileDragging()` - _(no documentation)_
- `public void SelectionBox_DraggedBackwards_Works()` - _(no documentation)_
- `public void SelectionBox_WithShift_AddsToTheSelection()` - _(no documentation)_
- `public void SelectionBox_WithoutShift_ReplacesTheSelection()` - _(no documentation)_
- `public void TinyBox_IsAClick_ThatClearsTheSelection()` - _(no documentation)_
- `public void TinyBox_IsMeasuredInScreenPixels()` - _(no documentation)_
- `public void SelectionBox_SkipsHiddenAndLockedLayers()` - _(no documentation)_
- `public void ClickOnAShapeOfALockedLayer_ActsLikeEmptySpace()` - _(no documentation)_
- `public void CancelAndToolChange_DuringABox_DropTheBoxAndKeepTheSelection()` - _(no documentation)_
- `public void CancelDuringAMoveOfSeveralShapes_KeepsTheReachedPositionAsOneStep()` - _(no documentation)_
- `public void SelectionChanges_CreateNoHistorySteps()` - _(no documentation)_
- `public void ResizeDrag_BottomRight_ChangesTheSizeAsOneStep_AndUndoRestores()` - _(no documentation)_
- `public void Press_ByTouch_FifteenPixelsFromAHandle_StartsAResize()` - _(no documentation)_
- `public void Press_ByMouseOrPen_FifteenPixelsFromAHandle_DoesNotGrabIt(int deviceType)` - _(no documentation)_
- `public void Press_ByTouch_UsesTheConfiguredGrabSize()` - _(no documentation)_
- `public void Press_ByTouch_InTheMiddleOfAShape_MovesItInsteadOfResizing()` - _(no documentation)_
- `public void Abort_DuringDrawing_DropsTheDraftAndRecordsNothing()` - _(no documentation)_
- `public void Abort_DuringAMove_PutsTheDocumentBackAndRecordsNothing()` - _(no documentation)_
- `public void Abort_DuringAResize_PutsTheDocumentBackAndRecordsNothing()` - _(no documentation)_
- `public void Abort_DuringARotate_PutsTheDocumentBackAndRecordsNothing()` - _(no documentation)_
- `public void Abort_DuringAMarquee_DropsTheBoxAndKeepsTheSelection()` - _(no documentation)_
- `public void Abort_WithoutADrag_DoesNothing()` - _(no documentation)_
- `public void Abort_ThenANewDrag_WorksAsUsual()` - _(no documentation)_
- `public void Cancel_AfterAMove_StillKeepsTheReachedPosition()` - _(no documentation)_
- `public void ResizeDrag_AnEdgeHandle_ChangesOnlyOneSide()` - _(no documentation)_
- `public void ResizeDrag_PressedAFewPixelsOffTheHandle_DoesNotJump()` - _(no documentation)_
- `public void ResizeDrag_ShiftKeepsTheRatio_AltResizesAroundTheCenter()` - _(no documentation)_
- `public void ResizeDrag_PastTheOppositeEdge_Flips()` - _(no documentation)_
- `public void ResizeDrag_OfARotatedShape_FollowsTheTurnedFrame()` - _(no documentation)_
- `public void ResizeDrag_OfSeveralShapes_ScalesThemTogether()` - _(no documentation)_
- `public void RotateDrag_TheHandleFollowsThePointer_AsOneStep()` - _(no documentation)_
- `public void RotateDrag_ShiftSnapsToFifteenDegrees_AndAStartAngleIsRespected()` - _(no documentation)_
- `public void RotateDrag_OfSeveralShapes_TurnsThemAroundTheFrameCenter()` - _(no documentation)_
- `public void ClickOnAHandle_WithoutMovement_MakesNoStep()` - _(no documentation)_
- `public void HandleHit_WinsOverAShapeBelow()` - _(no documentation)_
- `public void HandleSize_IsConstantOnTheScreen_SoItIsGrabbableAtLowZoom()` - _(no documentation)_
- `public void CancelOrToolChange_DuringAResize_KeepTheReachedStateAsOneStep()` - _(no documentation)_
- `public void HoverHandle_OnlyWithoutADragAndWithTheSelectTool()` - _(no documentation)_
- `public void DeleteSelection_OfSeveralShapes_IsOneStep_AndClearsTheSelection()` - _(no documentation)_
- `public void DeleteSelection_WithoutSelection_DoesNothing()` - _(no documentation)_
- `public void Draw_WithShift_MakesASquare_InEveryDragDirection()` - _(no documentation)_
- `public void Draw_WithShift_MakesACircle()` - _(no documentation)_
- `public void Draw_WithAlt_UsesThePressPointAsTheCenter()` - _(no documentation)_
- `public void Draw_WithShiftAndAlt_MakesASquareAroundTheCenter()` - _(no documentation)_
- `public void Draw_ModifiersCanChangeDuringTheDrag_AndTheReleaseDecides()` - _(no documentation)_

## CanvasMappingTests (class)

`public class CanvasMappingTests`

Source: `tests/PressToShape.App.Tests/CanvasMappingTests.cs:6-45`

_(no documentation)_

**Members**

- `public void ToDocument_KnownValues_ReturnsDocumentPosition(double screen, double origin, double zoom, double expected)` - _(no documentation)_
- `public void ToScreen_KnownValues_ReturnsScreenPosition(double value, double origin, double zoom, double expected)` - _(no documentation)_
- `public void ToScreenThenToDocument_AnyZoomAndOrigin_ReturnsTheStart(double zoom, double origin)` - _(no documentation)_

## CanvasSceneGroupTests (class)

`public class CanvasSceneGroupTests`

Source: `tests/PressToShape.App.Tests/CanvasSceneGroupTests.cs:10-115`

_(no documentation)_

**Members**

- `public void Build_Groups_AreFlattenedInDrawingOrder()` - _(no documentation)_
- `public void Build_HiddenShapeHiddenGroupAndItsChildren_AreLeftOut()` - _(no documentation)_
- `public void Build_HiddenGroupFlagDoesNotChangeTheChildrensOwnFlag_ButTheyAreStillLeftOut()` - _(no documentation)_
- `public void Build_LockedShapesAndGroups_AreDrawn()` - _(no documentation)_
- `public void Build_NoEnteredGroup_DimsNothing()` - _(no documentation)_
- `public void Build_EnteredGroup_DimsEverythingOutsideIt()` - _(no documentation)_
- `public void Build_EnteredNestedGroup_DimsSiblingsOfItsParentsToo()` - _(no documentation)_
- `public void Build_EnteredOuterGroup_DimsNothingInsideIt()` - _(no documentation)_
- `public void Build_RotatedLeafInGroup_KeepsItsRotationAndPosition()` - _(no documentation)_

## CanvasSceneTests (class)

`public class CanvasSceneTests`

Source: `tests/PressToShape.App.Tests/CanvasSceneTests.cs:10-246`

_(no documentation)_

**Members**

- `public void Build_NoShapes_ReturnsEmptyList()` - _(no documentation)_
- `public void Build_NullDocument_Throws()` - _(no documentation)_
- `public void Build_ShapesInTwoLayers_AreOrderedBottomLayerFirstThenBottomShapeFirst()` - _(no documentation)_
- `public void Build_HiddenLayer_IsSkippedAndComesBackWhenShownAgain()` - _(no documentation)_
- `public void Build_LockedLayer_IsDrawn()` - _(no documentation)_
- `public void Build_EmptyLayerBetweenFilledLayers_DrawsOnlyTheShapes()` - _(no documentation)_
- `public void Build_Zoom_ScalesPositionAndSizeFromTheOrigin(double zoom, double originX, double originY, double x, double y, double expectedWidth, double expectedHeight)` - _(no documentation)_
- `public void Build_StrokeWidth_IsScaledWithTheZoom()` - _(no documentation)_
- `public void Build_NoFill_StaysNull()` - _(no documentation)_
- `public void Build_NoStroke_GivesNullStrokeAndZeroWidth()` - _(no documentation)_
- `public void Build_StrokeWidthZero_GivesNullStroke()` - _(no documentation)_
- `public void Build_OpacityAndAlpha_ArePassedThrough()` - _(no documentation)_
- `public void Build_RectangleAndEllipse_GetTheirTypes()` - _(no documentation)_
- `public void Build_ZeroSizeShape_IsKept()` - _(no documentation)_
- `public void Build_ShapeOutsideTheArtboard_IsKept()` - _(no documentation)_
- `public void Build_SameDocumentTwice_DoesNotChangeTheDocument()` - _(no documentation)_
- `public void BuildShape_ShapeInDocument_EqualsTheMatchingBuildEntry()` - _(no documentation)_
- `public void BuildShape_NullShape_Throws()` - _(no documentation)_
- `public void BuildShape_PassesTheRotationThrough_AndUprightShapesHaveNone()` - _(no documentation)_
- `public void Build_PassesTheRotationOfEveryShape_AndKeepsTheBoxUnturned()` - _(no documentation)_

## ColorFieldViewModelTests (class)

`public class ColorFieldViewModelTests`

Source: `tests/PressToShape.App.Tests/ColorFieldViewModelTests.cs:9-287`

_(no documentation)_

**Members**

- `public void BeginUpdatesEnd_PreviewEachChange_AndCommitOnce()` - _(no documentation)_
- `public void BeginAndEndWithoutChange_CommitNothing()` - _(no documentation)_
- `public void EndEdit_OnlyOnce_AndASecondSessionCommitsAgain()` - _(no documentation)_
- `public void Update_WithoutBeginEdit_IsIgnored()` - _(no documentation)_
- `public void SetNone_AndSetColor_ApplyAtOnce()` - _(no documentation)_
- `public void ToggleNone_RemovesTheColor_AndGivesTheShownColorBack()` - _(no documentation)_
- `public void ToggleNone_OnAMixedField_RemovesTheColors()` - _(no documentation)_
- `public void NoneFromTheShapes_ShowsNoColor_ButKeepsAStartValueForThePicker()` - _(no documentation)_
- `public void Mixed_ShowsNoColor_AndTheFirstUpdateMakesItAColor()` - _(no documentation)_
- `public void Update_WithTheShownStartColorOnANoneField_IsIgnored_ButANewColorMakesItAColor()` - _(no documentation)_
- `public void IsNone_IsFalseWhileDisabled()` - _(no documentation)_
- `public void IsNone_IsTrueOnlyWithoutColorAndNotMixed()` - _(no documentation)_
- `public void Update_OnANoneField_MakesItAColor()` - _(no documentation)_
- `public void Values_RefreshWhenNotEditing_AndAreKeptWhileEditing()` - _(no documentation)_
- `public void Disabled_IgnoresEditsAndSettings()` - _(no documentation)_
- `public void AlphaIsPartOfTheColor()` - _(no documentation)_
- `public void NotifiesPropertyChanges()` - _(no documentation)_
- `public void Constructor_NullDelegates_Throw()` - _(no documentation)_

## ColorHexInputTests (class)

`public class ColorHexInputTests`

Source: `tests/PressToShape.App.Tests/ColorHexInputTests.cs:11-41`

_(no documentation)_

**Members**

- `public void Convert_ColorWithAlpha_WritesRrggbbaaWithoutTheHash()` - _(no documentation)_
- `public void ConvertBack_EightDigits_ReadsTheAlphaLast()` - _(no documentation)_
- `public void ConvertBack_SixDigits_IsOpaque()` - _(no documentation)_
- `public void ConvertBack_InvalidText_IsNotAColor(string text)` - _(no documentation)_

## CoreColorConverterTests (class)

`public class CoreColorConverterTests`

Source: `tests/PressToShape.App.Tests/CoreColorConverterTests.cs:11-67`

_(no documentation)_

**Members**

- `public void Convert_KeepsAllChannelsIncludingAlpha()` - _(no documentation)_
- `public void ConvertBack_KeepsAllChannelsIncludingAlpha()` - _(no documentation)_
- `public void RoundTrip_EveryAlphaAndSampleColors_IsLossless()` - _(no documentation)_
- `public void WrongInput_GivesNull()` - _(no documentation)_
- `public void BrushConverter_MakesASolidBrushWithTheColor()` - _(no documentation)_
- `public void BrushConverter_DoesNotConvertBack()` - _(no documentation)_

## CursorImageFilesTests (class)

`public class CursorImageFilesTests`

Source: `tests/PressToShape.App.Tests/CursorImageFilesTests.cs:10-58`

_(no documentation)_

**Members**

- `public void EveryCursorTypeButArrow_HasAnImageOfTheRightSize_AtEverySize()` - _(no documentation)_
- `public void CursorsFolder_HoldsNoOtherFiles()` - _(no documentation)_

## CursorImagesTests (class)

`public class CursorImagesTests`

Source: `tests/PressToShape.App.Tests/CursorImagesTests.cs:9-142`

_(no documentation)_

**Members**

- `public void SizeFor_Scale_ReturnsNearestSize(double scale, int expected)` - _(no documentation)_
- `public void SizeFor_ScaleNotUsable_ReturnsSmallSize(double scale)` - _(no documentation)_
- `public void Sizes_AreTheTwoImageSizes()` - _(no documentation)_
- `public void FileName_EveryTypeButArrow_HasAName()` - _(no documentation)_
- `public void FileName_Arrow_HasNone()` - _(no documentation)_
- `public void FileName_AllImages_AreUnique()` - _(no documentation)_
- `public void FileName_Type_MatchesTheToolOutput(string typeName, int size, string expected)` - _(no documentation)_
- `public void Hotspot_At64_IsDoubleTheHotspotAt32()` - _(no documentation)_
- `public void Hotspot_Magnifier_IsTheLensCenter()` - _(no documentation)_
- `public void Hotspot_Eyedropper_IsTheTipOfThePipette()` - _(no documentation)_
- `public void Hotspot_OtherTypes_AreTheImageCenter()` - _(no documentation)_
- `public void SizeDependentMembers_UnknownSize_Throw(int size)` - _(no documentation)_
- `public void Hotspot_Arrow_Throws()` - _(no documentation)_

## DefaultAppearanceTests (class)

`public class DefaultAppearanceTests`

Source: `tests/PressToShape.App.Tests/DefaultAppearanceTests.cs:9-54`

_(no documentation)_

**Members**

- `public void DefaultAppearance_NewState_IsTheModelDefault()` - _(no documentation)_
- `public void Draw_ChangedDefault_ShapeUsesIt(bool ellipse)` - _(no documentation)_
- `public void Draw_UnchangedDefault_ShapeUsesModelDefault()` - _(no documentation)_
- `public void UndoRedoAndReplaceDocument_DoNotChangeTheDefault()` - _(no documentation)_

## DelegateAppShutdownTests (class)

`public class DelegateAppShutdownTests`

Source: `tests/PressToShape.App.Tests/DelegateAppShutdownTests.cs:7-40`

_(no documentation)_

**Members**

- `public void Shutdown_CallsTheDelegate_EachTime()` - _(no documentation)_
- `public void Shutdown_ReadsTheTargetWhenCalled_SoALateAssignmentWorks()` - _(no documentation)_
- `public void Constructor_NullDelegate_Throws()` - _(no documentation)_

## DesktopShellViewModelTests (class)

`public class DesktopShellViewModelTests`

Source: `tests/PressToShape.App.Tests/DesktopShellViewModelTests.cs:13-964`

_(no documentation)_

**Members**

- `public void Shell_CreatesAllRegions()` - _(no documentation)_
- `public void ToolSelectedInToolbox_IsReflectedInToolbar()` - _(no documentation)_
- `public void ToolToggledInToolbar_IsReflectedInToolbox()` - _(no documentation)_
- `public void ZoomFromEveryRoute_ShowsTheSameValue()` - _(no documentation)_
- `public void FitToWindow_UsesTheCanvasViewport_FromMenuAndStatusBar()` - _(no documentation)_
- `public void FitToWindow_WithoutViewport_LeavesZoomUnchanged()` - _(no documentation)_
- `public void WindowMenu_SwitchesTheInspectorTab()` - _(no documentation)_
- `public void Quit_CallsTheInjectedShutdown()` - _(no documentation)_
- `public void SelectionState_FlowsToAllRegions()` - _(no documentation)_
- `public void ShortcutCommands_DoWhatTheMenuItemsDo()` - _(no documentation)_
- `public void FirstViewportSize_FitsTheArtboardOnce()` - _(no documentation)_
- `public void LanguageSwitch_ChangesTextInEveryRegion()` - _(no documentation)_
- `public void Shell_Starts_WithOneEmptyLayerNamedInTheCurrentLanguage()` - _(no documentation)_
- `public void Shell_Starts_WithTheSameDocumentInCanvasAndLayersPanel()` - _(no documentation)_
- `public void HidingALayerInThePanel_ChangesTheCanvasDocument()` - _(no documentation)_
- `public void DrawingARectangle_ShowsInDocumentLayersPanelAndSelection()` - _(no documentation)_
- `public void WindowTitle_Untitled_ShowsUntitledAndTheProductName()` - _(no documentation)_
- `public void WindowTitle_AfterAnEdit_HasAnAsterisk()` - _(no documentation)_
- `public Task WindowTitle_AfterSaveAs_ShowsTheFileNameWithoutAsterisk()` - _(no documentation)_
- `public Task WindowTitle_AfterOpen_ShowsTheOpenedFile()` - _(no documentation)_
- `public void WindowTitle_FollowsTheLanguage()` - _(no documentation)_
- `public Task Commands_CallTheService()` - _(no documentation)_
- `public Task OpenRecentMenuEntry_OpensTheFile()` - _(no documentation)_
- `public Task ConfirmCloseAsync_Clean_IsTrueWithoutAQuestion()` - _(no documentation)_
- `public Task ConfirmCloseAsync_Dirty_FollowsTheAnswer()` - _(no documentation)_
- `public Task ConfirmCloseAsync_SaveAnswer_SavesFirst()` - _(no documentation)_
- `public Task OpeningAFile_FitsTheArtboardToTheWindow()` - _(no documentation)_
- `public Task RecentFiles_ArePersistedInTheSettings()` - _(no documentation)_
- `public void RecentFiles_FromTheSettings_AppearInTheMenuAtStart()` - _(no documentation)_
- `public void UndoAndRedo_AtStart_AreDisabledInMenuToolbarAndAsCommands()` - _(no documentation)_
- `public void UndoAndRedo_EnableAndDisableWithTheHistory()` - _(no documentation)_
- `public void UndoAndRedo_AreDisabledWhileADragIsInProgress()` - _(no documentation)_
- `public void UndoRedoAndJump_EndToEnd_ThroughCanvasLayersHistoryAndTitle()` - _(no documentation)_
- `public void Undo_AfterAMoveDrag_RestoresThePositionInOneStep()` - _(no documentation)_
- `public void LayerToggle_IsUndoableFromTheLayersPanel()` - _(no documentation)_
- `public Task NewAndOpen_ClearTheHistoryAndSaveKeepsIt()` - _(no documentation)_
- `public void ShortcutCommands_ExistForUndoAndRedo()` - _(no documentation)_
- `public Task NewAndOpen_CenterTheArtboardAgain()` - _(no documentation)_
- `public void FitArtboardMenuItem_ResetsTheOffset()` - _(no documentation)_
- `public void EveryZoomRoute_KeepsTheCenterOfTheCanvasFixed()` - _(no documentation)_
- `public void FitRoutes_ResetTheOffset_ActualSizeDoesNot()` - _(no documentation)_
- `public void WheelZoomAndPan_ThenDrawing_LandsUnderThePointerInDocumentCoordinates()` - _(no documentation)_
- `public void PanAndZoom_DoNotCreateHistoryOrDirtyState_AndUndoIsBlockedDuringAPan()` - _(no documentation)_
- `public void HandAndZoomTools_DoNotEditTheDocument()` - _(no documentation)_
- `public void ResizingTheCanvas_LimitsTheOffsetAgain()` - _(no documentation)_
- `public void ScrollBarValues_FollowTheStatusBarZoomAndTheWheel()` - _(no documentation)_
- `public void TheShell_SharesOneSelectionEditor_WithTheCanvasAndTheInspector()` - _(no documentation)_
- `public void EditCommands_AreDisabledWithoutSelectionOrClipboard()` - _(no documentation)_
- `public void EditCommands_FollowTheSelection_AndNotify()` - _(no documentation)_
- `public void CopyAndPaste_CommandsWork_AndPasteFollowsTheClipboardAndTheActiveLayer()` - _(no documentation)_
- `public void CutDuplicateDeleteAndSelectCommands_DoTheirWork()` - _(no documentation)_
- `public void TheEditMenuItems_UseTheShellCommands()` - _(no documentation)_

## DocumentFileServiceTests (class)

`public sealed class DocumentFileServiceTests : IDisposable`

Source: `tests/PressToShape.App.Tests/DocumentFileServiceTests.cs:18-803`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public Task Save_WithoutAFile_AsksForOneAndWritesTheProject()` - _(no documentation)_
- `public Task Save_WithAFile_WritesWithoutADialog()` - _(no documentation)_
- `public Task SaveAs_Cancelled_WritesNothingAndChangesNothing()` - _(no documentation)_
- `public Task SaveAs_WithAReferenceFromTheSystemPicker_KeepsTheReferenceAsItIs(string reference)` - _(no documentation)_
- `public Task Export_WithAReferenceFromTheSystemPicker_KeepsTheReferenceAsItIs()` - _(no documentation)_
- `public Task SaveAs_AddsTheEndingOnlyWhenMissing(string chosen, string expected)` - _(no documentation)_
- `public Task SaveAs_ChangesTheTargetOfLaterSaves_AndSuggestsTheFileName()` - _(no documentation)_
- `public Task Save_WriteFailure_KeepsTheOldBytesTheDirtyStateAndShowsTheFileName()` - _(no documentation)_
- `public Task SaveAs_WriteFailure_DoesNotChangeTheSessionOrTheRecentList()` - _(no documentation)_
- `public Task Save_DocumentEditedWhileWriting_StaysDirty()` - _(no documentation)_
- `public Task Save_DialogThrows_ShowsAnErrorAndStops()` - _(no documentation)_
- `public Task Save_ErrorTexts_AreTranslated()` - _(no documentation)_
- `public Task Export_WritesAValidSvgAndLeavesSessionAndRecentAlone()` - _(no documentation)_
- `public Task Export_OfASavedDocument_SuggestsItsNameAndKeepsItClean()` - _(no documentation)_
- `public Task Export_Cancelled_WritesNothing()` - _(no documentation)_
- `public Task Export_WriteFailure_ShowsAnErrorWithTheFileName()` - _(no documentation)_
- `public Task Export_DialogThrows_ShowsAnError()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_
- `public Task Save_SavesWhatTheDocumentHoldsRightNow_NotAnOlderState()` - _(no documentation)_
- `public Task ConfirmDiscard_Clean_IsTrueWithoutAQuestion()` - _(no documentation)_
- `public Task ConfirmDiscard_DirtyAndDontSave_IsTrueWithoutWriting()` - _(no documentation)_
- `public Task ConfirmDiscard_DirtyAndCancel_IsFalseWithoutWriting()` - _(no documentation)_
- `public Task ConfirmDiscard_SaveAnswerWithAFile_SavesAndContinues()` - _(no documentation)_
- `public Task ConfirmDiscard_SaveAnswerButTheFileDialogIsCancelled_Stops()` - _(no documentation)_
- `public Task ConfirmDiscard_SaveAnswerButTheWriteFails_StopsAndShowsTheError()` - _(no documentation)_
- `public Task New_Clean_ReplacesTheDocumentWithoutAQuestion()` - _(no documentation)_
- `public Task New_NamesTheLayerInTheCurrentLanguage()` - _(no documentation)_
- `public Task New_Dirty_ClearsTheSelection()` - _(no documentation)_
- `public Task New_Dirty_CancelKeepsEverything()` - _(no documentation)_
- `public Task New_Dirty_SaveAnswerSavesThenReplaces()` - _(no documentation)_
- `public Task Open_ValidFile_ReplacesTheDocumentAndResetsTheSession()` - _(no documentation)_
- `public Task Open_Cancelled_ChangesNothing()` - _(no documentation)_
- `public Task Open_DirtyAndCancelledQuestion_DoesNotEvenShowThePicker()` - _(no documentation)_
- `public Task Open_PickerThrows_ShowsAnErrorAndChangesNothing()` - _(no documentation)_
- `public Task Open_BadFile_ShowsTheReasonAndChangesNothing(string content, string reason)` - _(no documentation)_
- `public Task Open_FileTooLarge_ShowsTheReason()` - _(no documentation)_
- `public Task Open_ReadFailure_ShowsTheReason()` - _(no documentation)_
- `public Task Open_ErrorTexts_AreTranslated()` - _(no documentation)_
- `public Task OpenRecent_ExistingFile_OpensItAndMovesItToTheTop()` - _(no documentation)_
- `public Task OpenRecent_Dirty_AsksFirst()` - _(no documentation)_
- `public Task OpenRecent_MissingFile_ShowsAnErrorAndRemovesTheEntry()` - _(no documentation)_
- `public Task OpenRecent_DamagedFile_ShowsAnErrorButKeepsTheEntry()` - _(no documentation)_
- `public Task OpenRecent_BlankPath_DoesNothing()` - _(no documentation)_
- `public Task SaveThenOpen_WithTheRealFileStoreInATempFolder_RoundTrips()` - _(no documentation)_
- `public Task New_ResetsTheHistory()` - _(no documentation)_
- `public Task Open_ResetsTheHistory()` - _(no documentation)_
- `public Task Open_FailedOrCancelled_KeepsTheHistory()` - _(no documentation)_
- `public Task SaveAndExport_KeepTheHistory()` - _(no documentation)_

## DocumentSessionTests (class)

`public class DocumentSessionTests`

Source: `tests/PressToShape.App.Tests/DocumentSessionTests.cs:11-217`

_(no documentation)_

**Members**

- `public void New_StartDocument_IsCleanWithoutAFile()` - _(no documentation)_
- `public void IsDirty_AfterAnEdit_IsTrueAndNotifies()` - _(no documentation)_
- `public void IsDirty_AfterALayerToggle_IsTrue()` - _(no documentation)_
- `public void IsDirty_EditThenChangedBackToAnEqualDocument_StaysDirty()` - _(no documentation)_
- `public void IsDirty_ReplacingTheDocumentWithTheSavedInstance_IsClean()` - _(no documentation)_
- `public void MarkSaved_WithTheWrittenInstance_MakesItCleanAndSetsThePath()` - _(no documentation)_
- `public void MarkSaved_WithAnOlderInstanceWhileTheDocumentChanged_StaysDirty()` - _(no documentation)_
- `public void Reset_SetsThePathAndClearsDirty()` - _(no documentation)_
- `public void Reset_WithoutAPath_HasNoFile()` - _(no documentation)_
- `public void DisplayName_IsTheFileNameWithoutTheFolder(string path, string expected)` - _(no documentation)_
- `public void Arguments_Invalid_Throw()` - _(no documentation)_
- `public void ZoomOrToolChanges_DoNotMakeTheDocumentDirty()` - _(no documentation)_
- `public void Undo_BackToTheSavedInstance_IsClean_AndRedoIsDirtyAgain()` - _(no documentation)_
- `public void Save_ThenUndo_IsDirty_AndRedoIsCleanAgain()` - _(no documentation)_
- `public void Save_DoesNotTouchTheHistory()` - _(no documentation)_

## DoubleTapDetectorTests (class)

`public class DoubleTapDetectorTests`

Source: `tests/PressToShape.App.Tests/DoubleTapDetectorTests.cs:6-73`

_(no documentation)_

**Members**

- `public void Tap_FirstTap_IsNotADoubleTap()` - _(no documentation)_
- `public void Tap_SecondTapSoonAndNear_IsADoubleTap()` - _(no documentation)_
- `public void Tap_SecondTapTooLate_StartsANewCount()` - _(no documentation)_
- `public void Tap_SecondTapTooFar_StartsANewCount()` - _(no documentation)_
- `public void Tap_ThirdTapRightAfterADoubleTap_IsNotAnotherDoubleTap()` - _(no documentation)_
- `public void Reset_ForgetsTheLastTap()` - _(no documentation)_
- `public void Tap_TimeGoingBackwards_IsNotADoubleTap()` - _(no documentation)_

## EditorShellViewModelRingTests (class)

`public class EditorShellViewModelRingTests`

Source: `tests/PressToShape.App.Tests/EditorShellViewModelRingTests.cs:16-207`

_(no documentation)_

**Members**

- `public void Slot_RunsTheSameCommandAsTheMenu_WithTheSameHistoryEntry(string key, string? submenu)` - _(no documentation)_
- `public void Group_ThroughTheRing_CanBeUndoneInOneStep()` - _(no documentation)_
- `public void Group_WithOneObjectSelected_DoesNothingAndAddsNoHistoryEntry()` - _(no documentation)_
- `public void EmptyCanvas_PasteRunsWhenTheClipboardHasShapes()` - _(no documentation)_
- `public void EmptyCanvas_PasteWithNothingCopied_IsUnavailable()` - _(no documentation)_
- `public void EmptyCanvas_SelectAllSelectsEveryShape()` - _(no documentation)_
- `public void DesktopShell_HasTheRingWithItsContent()` - _(no documentation)_
- `public void TabletShell_HasTheRingWithItsContent()` - _(no documentation)_

## EditorShellViewModelTests (class)

`public class EditorShellViewModelTests`

Source: `tests/PressToShape.App.Tests/EditorShellViewModelTests.cs:14-157`

_(no documentation)_

**Members**

- `public void NewShell_IsUntitledAndClean()` - _(no documentation)_
- `public void AnEdit_MakesTheShellDirty_AndRaisesTheNameNotifications()` - _(no documentation)_
- `public Task SaveAs_GivesTheFileNameAndClearsTheDirtyMark()` - _(no documentation)_
- `public void DocumentName_FollowsTheLanguage()` - _(no documentation)_
- `public void SharedRegions_UseTheSameEditorState()` - _(no documentation)_
- `public void UndoAndRedo_FollowTheHistory()` - _(no documentation)_
- `public Task ConfirmClose_WithoutChanges_AllowsClosingWithoutAQuestion()` - _(no documentation)_
- `public Task ConfirmClose_WithChanges_AsksAndFollowsTheAnswer()` - _(no documentation)_
- `public void FitToWindow_UsesTheCanvasViewport()` - _(no documentation)_

## EditorStateGroupTests (class)

`public class EditorStateGroupTests`

Source: `tests/PressToShape.App.Tests/EditorStateGroupTests.cs:12-274`

_(no documentation)_

**Members**

- `public void EnterGroup_Group_SetsEnteredGroup()` - _(no documentation)_
- `public void EnterGroup_PlainShapeUnknownIdHiddenOrLockedGroup_DoesNothing()` - _(no documentation)_
- `public void EnterGroup_SelectedGroup_ClearsTheSelectionOfTheGroupItself()` - _(no documentation)_
- `public void EnterGroup_SelectedChildren_StaySelected()` - _(no documentation)_
- `public void LeaveGroup_EnteredGroup_LeavesAndSelectsIt()` - _(no documentation)_
- `public void LeaveGroup_NestedGroup_EntersTheOuterGroup()` - _(no documentation)_
- `public void LeaveGroup_NothingEntered_DoesNothing()` - _(no documentation)_
- `public void EnteredGroup_Undo_ClearedWhenGroupIsGone()` - _(no documentation)_
- `public void EnteredGroup_Delete_Clears()` - _(no documentation)_
- `public void EnteredGroup_Ungroup_Clears()` - _(no documentation)_
- `public void EnteredGroup_HiddenOrLocked_Clears()` - _(no documentation)_
- `public void EnteredGroup_ChangeInsideTheGroup_StaysEntered()` - _(no documentation)_
- `public void ReplaceDocument_NewDocument_ClearsTheEnteredGroup()` - _(no documentation)_
- `public void SetSelection_IdsWithDifferentParents_KeepsTheFirstParentsIds()` - _(no documentation)_
- `public void SetSelection_GroupAndPlainShapeOnTopLevel_BothSelected()` - _(no documentation)_
- `public void SetSelection_ChildOfHiddenGroup_IsLeftOut()` - _(no documentation)_
- `public void Selection_GroupGetsHidden_LeavesTheSelection()` - _(no documentation)_
- `public void Selection_ChildSelectedThenGroupLocked_LeavesTheSelection()` - _(no documentation)_
- `public void Selection_AllMembersRemoved_GroupDisappearsFromTheSelection()` - _(no documentation)_

## EditorStateTests (class)

`public class EditorStateTests`

Source: `tests/PressToShape.App.Tests/EditorStateTests.cs:12-1046`

_(no documentation)_

**Members**

- `public void NewState_HasExpectedDefaults()` - _(no documentation)_
- `public void SetZoom_OutsideRange_IsClamped()` - _(no documentation)_
- `public void ZoomIn_AndZoomOut_StepThroughPresets()` - _(no documentation)_
- `public void ZoomIn_AtMaximum_StaysAtMaximum()` - _(no documentation)_
- `public void ZoomToActualSize_Sets100()` - _(no documentation)_
- `public void ZoomToFit_UsesArtboardAndViewport()` - _(no documentation)_
- `public void ZoomToFit_WithoutViewportSize_DoesNothing()` - _(no documentation)_
- `public void Changes_RaisePropertyChanged()` - _(no documentation)_
- `public void SetZoom_ToSameValue_DoesNotRaisePropertyChanged()` - _(no documentation)_
- `public void NewState_WithoutDocument_HasAnEmptyDefaultDocument()` - _(no documentation)_
- `public void NewState_WithDocument_KeepsThatDocument()` - _(no documentation)_
- `public void Document_Replaced_RaisesDocumentAndArtboardNotifications()` - _(no documentation)_
- `public void ArtboardSize_FollowsTheDocument()` - _(no documentation)_
- `public void Zoom_Changed_LeavesTheDocumentUntouched()` - _(no documentation)_
- `public void UpdateLayer_ChangesTheLayerAndKeepsTheOldDocumentUnchanged()` - _(no documentation)_
- `public void UpdateLayer_UnknownLayer_ThrowsAndKeepsTheDocument()` - _(no documentation)_
- `public void ZoomToFit_UsesTheArtboardOfTheDocument()` - _(no documentation)_
- `public void NewState_ActiveLayerIsTheTopmost()` - _(no documentation)_
- `public void NewState_HasNoSelectionAndNoDraft()` - _(no documentation)_
- `public void Document_ActiveLayerRemoved_FallsBackToTheTopmostLayer()` - _(no documentation)_
- `public void Document_OtherLayerRemoved_KeepsTheActiveLayer()` - _(no documentation)_
- `public void Select_Shape_SetsSelectionAndHasSelection()` - _(no documentation)_
- `public void Select_Null_ClearsSelection()` - _(no documentation)_
- `public void Document_UnrelatedEdit_KeepsTheSelection()` - _(no documentation)_
- `public void Document_SelectedShapeRemoved_ClearsTheSelection()` - _(no documentation)_
- `public void Document_LayerOfSelectedShapeRemoved_ClearsTheSelection()` - _(no documentation)_
- `public void Draft_Set_RaisesPropertyChanged()` - _(no documentation)_
- `public void Edit_RecordsAStepAndSetsTheDocument()` - _(no documentation)_
- `public void Edit_ReturningTheSameInstance_RecordsNothing()` - _(no documentation)_
- `public void Edit_WhenTheChangeThrows_ChangesNothing()` - _(no documentation)_
- `public void PreviewThenCommit_RecordsOneStep()` - _(no documentation)_
- `public void Commit_WithoutAChange_RecordsNothing()` - _(no documentation)_
- `public void UndoRedoAndJump_SetTheDocumentAndRaiseTheNotifications()` - _(no documentation)_
- `public void UndoRedoAndJump_WithNothingToDo_DoNothing()` - _(no documentation)_
- `public void Undo_KeepsTheSelectionWhileTheShapeExists_AndClearsItOtherwise()` - _(no documentation)_
- `public void Undo_OfADeletedShape_BringsItBackWithoutSelection()` - _(no documentation)_
- `public void CanUndoAndCanRedo_FollowTheDragFlag()` - _(no documentation)_
- `public void ReplaceDocument_ResetsTheHistoryAndShowsTheDocument()` - _(no documentation)_
- `public void UndoBeyondTheLimit_IsNotPossible()` - _(no documentation)_
- `public void Arguments_Invalid_Throw()` - _(no documentation)_
- `public void NewState_HasNoOffsetAndNoViewport()` - _(no documentation)_
- `public void SetPan_StoresTheOffset_AndNotifies()` - _(no documentation)_
- `public void SetPan_BeyondTheLimit_IsLimited()` - _(no documentation)_
- `public void SetPan_NonFiniteValues_LeaveThatAxisUnchanged()` - _(no documentation)_
- `public void PanBy_AddsTheDistance_AndIgnoresNonFiniteDistances()` - _(no documentation)_
- `public void SetViewport_SmallerThanTheOffsetAllows_LimitsTheOffsetAgain()` - _(no documentation)_
- `public void SetViewport_NonFiniteOrNegative_BecomesUnknown()` - _(no documentation)_
- `public void ZoomAround_KeepsTheDocumentPointUnderTheAnchor()` - _(no documentation)_
- `public void CenterAnchoredRoutes_KeepTheCenterOfTheCanvasFixed(double target)` - _(no documentation)_
- `public void ZoomInZoomOutAndActualSize_KeepTheCenter()` - _(no documentation)_
- `public void ZoomAround_OutsideTheRange_StopsAtTheLimit(double target, double expected)` - _(no documentation)_
- `public void ZoomAround_AtTheLimitAlready_ChangesNothingAndRaisesNothing()` - _(no documentation)_
- `public void ZoomAround_NaN_IsTreatedAsTheDefaultZoomByTheClamp_AndStaysFinite()` - _(no documentation)_
- `public void ZoomAround_AnchorAtTheCanvasEdge_StillLimitsTheOffset()` - _(no documentation)_
- `public void ZoomToFit_ResetsTheOffset_AndFits()` - _(no documentation)_
- `public void ZoomToFit_WithoutAViewport_KeepsTheZoomButResetsTheOffset()` - _(no documentation)_
- `public void ViewChanges_NeverTouchTheDocumentTheHistoryOrTheDragFlag()` - _(no documentation)_
- `public void ArtboardSizeChange_LimitsTheOffsetAgain()` - _(no documentation)_
- `public void SetSelection_KeepsTheGivenOrder_AndLeavesOutRepeatsAndUnknownIds()` - _(no documentation)_
- `public void SelectOnly_ReplacesTheSelection_AndSelectedShapeIdIsTheSingleId()` - _(no documentation)_
- `public void ToggleSelected_AddsAndRemoves()` - _(no documentation)_
- `public void Selection_IgnoresShapesOfHiddenAndLockedLayers()` - _(no documentation)_
- `public void LockingOrHidingALayer_RemovesItsShapesFromTheSelection()` - _(no documentation)_
- `public void Selection_AfterEditingASelectedShapeOrAnotherLayer_Stays()` - _(no documentation)_
- `public void Selection_AfterDeleteUndoAndRedo_FollowsWhatExists()` - _(no documentation)_
- `public void ReplaceDocument_ClearsTheSelection()` - _(no documentation)_
- `public void ClearSelection_WhenAlreadyEmpty_RaisesNothing()` - _(no documentation)_
- `public void SelectionChanges_RaiseTheDerivedNotifications_AndCreateNoHistory()` - _(no documentation)_
- `public void Edit_WithExtend_MergesIntoTheLastStepOfTheSameType()` - _(no documentation)_
- `public void SelectionRepair_OnALargeDocument_KeepsAllSelectedShapes()` - _(no documentation)_

## EyedropperTests (class)

`public class EyedropperTests`

Source: `tests/PressToShape.App.Tests/EyedropperTests.cs:11-155`

_(no documentation)_

**Members**

- `public void Click_OnAShapeWithFill_ReportsTheFillWithAlphaAndEndsThePick()` - _(no documentation)_
- `public void Click_OnAShapeWithoutFill_ReportsTheStroke()` - _(no documentation)_
- `public void Click_OnAShapeWithNoFillAndNoStroke_ReportsNothingAndEndsThePick()` - _(no documentation)_
- `public void Click_OverlappingShapes_TakesTheTopmost()` - _(no documentation)_
- `public void Click_OnEmptyCanvas_ChangesNothingAndEndsThePick()` - _(no documentation)_
- `public void Click_DuringAPickWithTheSelectTool_DoesNotSelectOrDrag()` - _(no documentation)_
- `public void Click_AfterThePickEnded_WorksAsBefore()` - _(no documentation)_
- `public void Escape_DuringAPick_EndsItWithoutAColorAndWithoutClearingTheSelection()` - _(no documentation)_
- `public void CanvasClick_DuringAPick_IsTakenByThePickWithAnyTool(string toolName)` - _(no documentation)_

## EyedropperToolTests (class)

`public class EyedropperToolTests`

Source: `tests/PressToShape.App.Tests/EyedropperToolTests.cs:11-126`

_(no documentation)_

**Members**

- `public void Click_OnAShape_SetsTheFillOfTheSelectionAsOneStep()` - _(no documentation)_
- `public void ShiftClick_OnAShape_SetsTheStrokeToTheShapesFillColor()` - _(no documentation)_
- `public void Click_WithNothingSelected_ChangesTheDefaultsAndNoStep()` - _(no documentation)_
- `public void Click_OnEmptyCanvas_ChangesNothing()` - _(no documentation)_
- `public void Click_StaysOnTheEyedropperToolAndKeepsTheSelection()` - _(no documentation)_
- `public void FlyoutPick_StillWorksWithAnotherToolActive()` - _(no documentation)_

## FakeCatalog (class)

`internal sealed class FakeCatalog : ITranslationCatalog`

Source: `tests/PressToShape.App.Tests/FakeLocalizer.cs:7-17`

A dictionary-based translation catalog for tests.

**Members**

- `public FakeCatalog(Dictionary<string, string> entries)` - _(no documentation)_
- `public string? Translate(string text)` - Keys are the message id, or "context|id" for texts with a context.
- `public string? Translate(string context, string text)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Localization.ITranslationCatalog`

## FakeCatalogSource (class)

`internal sealed class FakeCatalogSource : ICatalogSource`

Source: `tests/PressToShape.App.Tests/FakeLocalizer.cs:20-37`

A catalog source for tests: one dictionary per language code. Counts the loads.

**Members**

- `public int LoadCount { get; private set; }` - _(no documentation)_
- `public FakeCatalogSource Add(string code, Dictionary<string, string> entries)` - _(no documentation)_
- `public ITranslationCatalog? Load(string languageCode)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Localization.ICatalogSource`

## FakeFileDialogs (class)

`internal sealed class FakeFileDialogs : IFileDialogs`

Source: `tests/PressToShape.App.Tests/FileServiceFakes.cs:51-87`

Returns queued answers instead of showing pickers, and records what it was asked.

**Members**

- `public Queue<string?> OpenAnswers { get; }` - _(no documentation)_
- `public Queue<string?> SaveAnswers { get; }` - _(no documentation)_
- `public Queue<string?> ExportAnswers { get; }` - _(no documentation)_
- `public List<string> SuggestedSaveNames { get; }` - _(no documentation)_
- `public List<string> SuggestedExportNames { get; }` - _(no documentation)_
- `public int OpenCount { get; private set; }` - _(no documentation)_
- `public Exception? Failure { get; set; }` - _(no documentation)_
- `public Task<string?> PickProjectToOpenAsync()` - _(no documentation)_
- `public Task<string?> PickProjectToSaveAsync(string suggestedFileName)` - _(no documentation)_
- `public Task<string?> PickSvgToExportAsync(string suggestedFileName)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IFileDialogs`
- Injected into `PressToShape.App.Tests.TestShell`

## FakeFileStore (class)

`internal sealed class FakeFileStore : IFileStore`

Source: `tests/PressToShape.App.Tests/FileServiceFakes.cs:12-48`

An in-memory file system. Can be told to fail on write or read.

**Members**

- `public Dictionary<string, byte[]> Files { get; }` - _(no documentation)_
- `public Exception? WriteFailure { get; set; }` - _(no documentation)_
- `public Exception? ReadFailure { get; set; }` - _(no documentation)_
- `public int WriteCount { get; private set; }` - _(no documentation)_
- `public Task<byte[]> ReadAllBytesAsync(string path, long maxBytes, CancellationToken cancellationToken = default(CancellationToken))` - _(no documentation)_
- `public Task WriteAllBytesAtomicAsync(string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IFileStore`
- Injected into `PressToShape.App.Tests.TestShell`

## FakeLocalizer (class)

`internal static class FakeLocalizer`

Source: `tests/PressToShape.App.Tests/FakeLocalizer.cs:40-47`

Creates real PressToShape.App.Localization.Localizer instances with fake German translations.

**Members**

- `public Localizer Create(string initialLanguage = "en", Dictionary<string, string>? german = null)` - _(no documentation)_

## FakeMessageDialogs (class)

`internal sealed class FakeMessageDialogs : IMessageDialogs`

Source: `tests/PressToShape.App.Tests/FileServiceFakes.cs:90-109`

Answers the unsaved-changes question with a fixed or queued choice, and records questions and errors.

**Members**

- `public UnsavedChoice Answer { get; set; }` - _(no documentation)_
- `public List<string> Questions { get; }` - _(no documentation)_
- `public List<string> Errors { get; }` - _(no documentation)_
- `public Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName)` - _(no documentation)_
- `public Task ShowErrorAsync(string message)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IMessageDialogs`
- Injected into `PressToShape.App.Tests.SelectionAndPropertiesIntegrationTests.Session`
- Injected into `PressToShape.App.Tests.TestShell`

## FakePickedFile (class)

`internal sealed class FakePickedFile : IPickedFile`

Source: `tests/PressToShape.App.Tests/FakePickedFile.cs:12-101`

A picked file kept in memory. Like the Android one, opening it for writing empties it at once. It can be told to fail after a number of written bytes, to simulate a full disk, and to fail when it is opened.

**Members**

- `public FakePickedFile(string name, byte[]? content = null)` - _(no documentation)_
- `public byte[] Content { get; set; }` - _(no documentation)_
- `public int? FailWritingAfterBytes { get; set; }` - Writing fails after this many bytes of one write stream. null means it never fails.
- `public Exception? OpenReadFailure { get; set; }` - _(no documentation)_
- `public int WriteOpenCount { get; private set; }` - _(no documentation)_
- `public string Name { get; }` - _(no documentation)_
- `public Task<Stream> OpenReadAsync()` - _(no documentation)_
- `public Task<Stream> OpenWriteAsync()` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IPickedFile`
- Injected into `PressToShape.App.Tests.FakePickedFile.WriteStream`

## GettextCatalogTests (class)

`public class GettextCatalogTests`

Source: `tests/PressToShape.App.Tests/GettextCatalogTests.cs:8-77`

_(no documentation)_

**Members**

- `public void GermanCatalog_IsEmbedded_AndLoads()` - _(no documentation)_
- `public void KnownTexts_AreTranslated()` - _(no documentation)_
- `public void UnknownTexts_ReturnNull()` - _(no documentation)_
- `public void English_HasNoCatalog()` - _(no documentation)_
- `public void MissingOrInvalidLanguage_ReturnsNull(string code)` - _(no documentation)_
- `public void CorruptStream_Throws_ButTheSourceNeverDoes()` - _(no documentation)_
- `public void LocalizerWithTheEmbeddedSource_TranslatesGerman()` - _(no documentation)_

## HeadProjectReferenceTests (class)

`public class HeadProjectReferenceTests`

Source: `tests/PressToShape.App.Tests/HeadProjectReferenceTests.cs:9-49`

_(no documentation)_

**Members**

- `public void App_ProjectReferences_OnlyCore()` - _(no documentation)_
- `public void Head_ProjectReferences_OnlyApp(string csprojPath)` - _(no documentation)_
- `public void Core_ProjectReferences_Nothing()` - _(no documentation)_

## HeadlessUi (class)

`internal static class HeadlessUi`

Source: `tests/PressToShape.App.Tests/HeadlessUi.cs:40-78`

Runs code on the one thread that owns the headless Avalonia platform. Avalonia must be used from a single thread, xUnit v2 uses many. The test classes that use it belong to the collection PressToShape.App.Tests.HeadlessUi.CollectionName, so they run one after the other: a test that lets the dispatcher run its jobs (to lay out a window) would otherwise also run the jobs of a test of another class, in the middle of its own steps.

**Members**

- `public string CollectionName` - The name of the xUnit collection of every test class that uses PressToShape.App.Tests.HeadlessUi.Run``1.
- `public T Run<T>(Func<T> action)` - _(no documentation)_
- `public void Run(Action action)` - _(no documentation)_

## HeadlessUiApp (class)

`internal sealed class HeadlessUiApp : Application`

Source: `tests/PressToShape.App.Tests/HeadlessUi.cs:19-33`

The application of the headless UI tests: the real theme and control styles, but not the real startup, which would read and write the user's settings file. Avalonia's xUnit integration needs xunit v3, so the tests use PressToShape.App.Tests.HeadlessUi.Run``1 instead.

**Members**

- `public void Initialize()` - _(no documentation)_

## HistoryViewModelTests (class)

`public class HistoryViewModelTests`

Source: `tests/PressToShape.App.Tests/HistoryViewModelTests.cs:13-238`

_(no documentation)_

**Members**

- `public void Initially_OnlyTheStartRowExistsAndItIsCurrent()` - _(no documentation)_
- `public void Rows_AfterEdits_ListTheStepsOldestFirstWithNames()` - _(no documentation)_
- `public void EveryStepType_HasAnEnglishName()` - _(no documentation)_
- `public void Undo_MarksTheCurrentRowAndTheRedoableRowsInPlace()` - _(no documentation)_
- `public void Selecting_ARow_JumpsToItsState()` - _(no documentation)_
- `public void Selecting_TheCurrentRow_ChangesNothing()` - _(no documentation)_
- `public void Selecting_Null_IsIgnored()` - _(no documentation)_
- `public void Selecting_WhileADragIsInProgress_IsRefusedAndTheSelectionGoesBack()` - _(no documentation)_
- `public void AnEditAfterUndo_RebuildsWithoutTheDroppedSteps()` - _(no documentation)_
- `public void ReplaceDocument_NewOrOpen_LeavesOnlyTheStartRow()` - _(no documentation)_
- `public void MoreThanTheLimit_KeepsAtMostOneHundredOneRows()` - _(no documentation)_
- `public void Names_AreTranslatedAndFollowALanguageSwitch()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## InspectorViewModelTests (class)

`public class InspectorViewModelTests`

Source: `tests/PressToShape.App.Tests/InspectorViewModelTests.cs:9-140`

_(no documentation)_

**Members**

- `public void DefaultTab_IsProperties()` - _(no documentation)_
- `public void ShowTab_ChangesSelectedTab_AndIndex()` - _(no documentation)_
- `public void SettingTheIndex_SelectsTheTab()` - _(no documentation)_
- `public void OutOfRangeIndex_IsIgnored(int index)` - _(no documentation)_
- `public void TabChange_RaisesIndexNotification()` - _(no documentation)_
- `public void Assets_ShowAnEmptyStateMessage_AndHistoryListsTheStartState()` - _(no documentation)_
- `public void History_FollowsTheEditorState()` - _(no documentation)_
- `public void Properties_Sections_HaveMockupDefaults()` - _(no documentation)_
- `public void Properties_SectionsCanBeToggled()` - _(no documentation)_
- `public void Properties_FollowSelectionState()` - _(no documentation)_
- `public void PlaceholderPanels_AreTranslated_AndRefreshOnLanguageChange()` - _(no documentation)_

## KeyboardInsetTests (class)

`public class KeyboardInsetTests`

Source: `tests/PressToShape.App.Tests/KeyboardInsetTests.cs:6-48`

_(no documentation)_

**Members**

- `public void ClosedKeyboard_LeavesNoGap()` - _(no documentation)_
- `public void OpenKeyboard_LeavesTheCoveredPartOfTheView()` - _(no documentation)_
- `public void AKeyboardBelowTheView_LeavesNoGap()` - _(no documentation)_
- `public void AKeyboardThatStartsExactlyAtTheEndOfTheView_LeavesNoGap()` - _(no documentation)_
- `public void NonFiniteNumbers_LeaveNoGap(double viewBottom, double coveredTop)` - _(no documentation)_
- `public void AKeyboardAboveTheViewTop_CoversTheWholeView()` - _(no documentation)_

## LanguagePreferenceTests (class)

`public class LanguagePreferenceTests`

Source: `tests/PressToShape.App.Tests/LanguagePreferenceTests.cs:9-113`

_(no documentation)_

**Members**

- `public void ChooseStartLanguage_UsesTheSavedLanguage()` - _(no documentation)_
- `public void ChooseStartLanguage_WithoutSavedLanguage_UsesTheSystemLanguage()` - _(no documentation)_
- `public void ChooseStartLanguage_IgnoresAnUnsupportedSavedLanguage()` - _(no documentation)_
- `public void Attach_SavesTheLanguageWhenItChanges()` - _(no documentation)_
- `public void Attach_DoesNotSaveWhenNothingChanges()` - _(no documentation)_
- `public void Attach_KeepsUnknownSettings()` - _(no documentation)_
- `public void LocalizationHost_DefaultsToAnEnglishLocalizer_AndCanBeReplaced()` - _(no documentation)_

## LanguageSelectorTests (class)

`public class LanguageSelectorTests`

Source: `tests/PressToShape.App.Tests/LanguageSelectorTests.cs:7-53`

_(no documentation)_

**Members**

- `public void SavedChoice_WinsOverSystemLanguage(string saved, string system, string expected)` - _(no documentation)_
- `public void NoSavedChoice_GermanSystem_StartsInGerman(string system)` - _(no documentation)_
- `public void NoSavedChoice_OtherSystemLanguage_StartsInEnglish(string system)` - _(no documentation)_
- `public void UnsupportedSavedChoice_IsIgnored(string saved, string system, string expected)` - _(no documentation)_
- `public void InvariantCulture_FallsBackToEnglish()` - _(no documentation)_

## LayersViewModelTests (class)

`public class LayersViewModelTests`

Source: `tests/PressToShape.App.Tests/LayersViewModelTests.cs:14-482`

_(no documentation)_

**Members**

- `public void Rows_LayerThenObjectsTopmostFirst()` - _(no documentation)_
- `public void Group_AfterGrouping_IsShownClosed()` - _(no documentation)_
- `public void Rows_Group_IsCollapsedAtFirstAndOpensToShowItsChildren()` - _(no documentation)_
- `public void Rows_CollapsingTheLayer_HidesEverythingBelow()` - _(no documentation)_
- `public void Rows_NoName_ShowTheDefaultNameOfTheType()` - _(no documentation)_
- `public void Rows_DefaultNames_AreTranslated_AndFollowALanguageChange()` - _(no documentation)_
- `public void Update_FlagsAndNames_KeepTheRowInstances()` - _(no documentation)_
- `public void Update_StructureChange_ReusesTheRowsOfTheSurvivingObjects()` - _(no documentation)_
- `public void ChildOfAHiddenOrLockedGroup_IsMarkedEffectivelyHiddenOrLocked_KeepingItsOwnFlags()` - _(no documentation)_
- `public void ExpandedState_SurvivesUndoAndRedoAsFarAsTheGroupExists()` - _(no documentation)_
- `public void Selection_OnTheCanvas_MarksRowsAndOpensTheGroupsAroundThem()` - _(no documentation)_
- `public void Select_RowOnTheTopLevel_SelectsTheObjectAndLeavesGroups()` - _(no documentation)_
- `public void Select_RowInsideAGroup_EntersTheGroup()` - _(no documentation)_
- `public void Select_WithExtend_AddsAndRemovesRowsOfTheSameParent_AndReplacesOtherwise()` - _(no documentation)_
- `public void Select_LockedRow_OnlyMarksItAsCurrentAndKeepsTheCanvasSelection()` - _(no documentation)_
- `public void Select_ThenTheCanvasSelectionChanges_ClearsTheCurrentMark()` - _(no documentation)_
- `public void Toggles_OnALockedHiddenRow_StillWork()` - _(no documentation)_
- `public void Toggles_VisibleRow_HidesAndLocksIt()` - _(no documentation)_
- `public void Rename_CommitsAsOneStep_EmptyNameFallsBackToTheDefault()` - _(no documentation)_
- `public void Rename_Cancelled_ChangesNothing()` - _(no documentation)_
- `public void Rename_LayerRow_IsNotPossible()` - _(no documentation)_
- `public void Reorder_WithinAParent_MovesTheObjectAboveTheTarget()` - _(no documentation)_
- `public void Reorder_AnotherParentOrALayerRow_IsIgnored()` - _(no documentation)_
- `public void Select_LayerRow_MakesTheLayerActive()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_
- `public void LayerRow_Toggles_HideAndLockTheLayerAsOneStepEach()` - _(no documentation)_
- `public void LayerToggle_ThenUndo_RestoresTheLayerAndTheRow()` - _(no documentation)_
- `public void ActiveLayer_IsMarked_AndFollowsTheState()` - _(no documentation)_
- `public void AddLayerCommand_CannotExecuteYet()` - _(no documentation)_

## LayersViewTests (class)

`public class LayersViewTests`

Source: `tests/PressToShape.App.Tests/LayersViewTests.cs:24-172`

_(no documentation)_

**Members**

- `public void View_ShowsTheLayerAndTheObjectsTopmostFirst()` - _(no documentation)_
- `public void View_HideToggle_HidesTheObjectAndTheRowFollows()` - _(no documentation)_
- `public void View_RowsFollowTheDocument_AGroupShowsAfterGroupingAndOpens()` - _(no documentation)_
- `public void View_SelectingOnTheCanvas_MarksTheRowSelected()` - _(no documentation)_
- `public void View_WithTheTouchSizes_EveryButtonIsAtLeast48()` - _(no documentation)_
- `public void Inspector_ShowsTheLayersTabThroughTheViewLocator()` - _(no documentation)_
- `public void TabletPanel_ShowsTheLayersListForTheLayersTab()` - _(no documentation)_

## LocalFileStoreTests (class)

`public sealed class LocalFileStoreTests : IDisposable`

Source: `tests/PressToShape.App.Tests/LocalFileStoreTests.cs:10-206`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public Task Write_NewFileInNewFolder_CreatesBoth()` - _(no documentation)_
- `public Task Write_ExistingFile_IsReplaced()` - _(no documentation)_
- `public Task Write_Success_LeavesNoTemporaryFile()` - _(no documentation)_
- `public Task Write_TargetIsAnExistingDirectory_ThrowsAndLeavesNoTemporaryFile()` - _(no documentation)_
- `public Task Write_FolderIsAFile_Throws()` - _(no documentation)_
- `public Task Write_FolderIsReadOnly_ThrowsAndKeepsTheOldFile()` - _(no documentation)_
- `public Task Read_ExistingFile_ReturnsTheBytes()` - _(no documentation)_
- `public Task Read_EmptyFile_ReturnsNoBytes()` - _(no documentation)_
- `public Task Read_FileOverTheLimit_ThrowsInvalidDataException()` - _(no documentation)_
- `public Task Read_FileExactlyAtTheLimit_IsRead()` - _(no documentation)_
- `public Task Read_MissingFile_ThrowsFileNotFoundException()` - _(no documentation)_
- `public Task Read_MissingFolder_ThrowsDirectoryNotFoundException()` - _(no documentation)_
- `public Task CancelledToken_ThrowsBeforeTouchingTheFile()` - _(no documentation)_
- `public Task Arguments_EmptyPathOrNullBytes_Throw()` - _(no documentation)_

## LocalizerTests (class)

`public class LocalizerTests`

Source: `tests/PressToShape.App.Tests/LocalizerTests.cs:8-178`

_(no documentation)_

**Members**

- `public void English_ReturnsTheSourceText()` - _(no documentation)_
- `public void English_DoesNotLoadACatalog()` - _(no documentation)_
- `public void German_TranslatesKnownTexts()` - _(no documentation)_
- `public void German_MissingTranslation_FallsBackToEnglish()` - _(no documentation)_
- `public void German_UsesTheContext()` - _(no documentation)_
- `public void CatalogWithoutEntries_FallsBackToEnglish()` - _(no documentation)_
- `public void SetLanguage_SwitchesAndRaisesTheEventOnce()` - _(no documentation)_
- `public void SetLanguage_BackToEnglish_ReturnsSourceText()` - _(no documentation)_
- `public void SetLanguage_ToTheCurrentLanguage_DoesNothing()` - _(no documentation)_
- `public void SetLanguage_UnknownCode_IsIgnored(string code)` - _(no documentation)_
- `public void UnknownInitialLanguage_StartsInEnglish()` - _(no documentation)_
- `public void LanguageCodes_AreCaseInsensitive()` - _(no documentation)_
- `public void Languages_ListEnglishAndGerman_WithOwnNames()` - _(no documentation)_
- `public void N_ReturnsItsArgument()` - _(no documentation)_
- `public void CultureUsedForFormatting_FollowsTheLanguage()` - _(no documentation)_

## LongPressTrackerTests (class)

`public class LongPressTrackerTests`

Source: `tests/PressToShape.App.Tests/LongPressTrackerTests.cs:6-122`

_(no documentation)_

**Members**

- `public void Tick_BeforeTheDelay_DoesNotFire()` - _(no documentation)_
- `public void Tick_AtTheDelay_FiresOnce()` - _(no documentation)_
- `public void Tick_AfterMovingBeyondTheSlop_DoesNotFire()` - _(no documentation)_
- `public void Tick_AfterMovingWithinTheSlop_StillFires()` - _(no documentation)_
- `public void Tick_AfterRelease_DoesNotFire()` - _(no documentation)_
- `public void Tick_AfterCancel_DoesNotFire()` - _(no documentation)_
- `public void Tick_WithoutPress_DoesNotFire()` - _(no documentation)_
- `public void ConsumeFired_AfterAFiredPress_IsTrueOnce()` - _(no documentation)_
- `public void ConsumeFired_AfterAShortPress_IsFalse()` - _(no documentation)_
- `public void Press_AfterAFiredPress_ResetsTheFiredFlag()` - _(no documentation)_
- `public void IsWatching_IsTrueOnlyWhileWaiting()` - _(no documentation)_

## ManualTimeProvider (class)

`internal sealed class ManualTimeProvider : TimeProvider`

Source: `tests/PressToShape.App.Tests/ManualTimeProvider.cs:6-15`

A clock that only moves when a test says so. One timestamp tick is one millisecond.

**Members**

- `public long TimestampFrequency { get; }` - _(no documentation)_
- `public long GetTimestamp()` - _(no documentation)_
- `public void Advance(long milliseconds)` - _(no documentation)_

**Relationships**

- Injected into `PressToShape.App.Tests.CanvasAreaViewModelDoubleTapTests.Fixture`

## MemorySettingsStore (class)

`internal sealed class MemorySettingsStore : ISettingsStore`

Source: `tests/PressToShape.App.Tests/FileServiceFakes.cs:112-119`

Settings kept in memory.

**Members**

- `public AppSettings Current { get; set; }` - _(no documentation)_
- `public AppSettings Load()` - _(no documentation)_
- `public void Save(AppSettings settings)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Settings.ISettingsStore`

## MenuBarViewModelTests (class)

`public class MenuBarViewModelTests`

Source: `tests/PressToShape.App.Tests/MenuBarViewModelTests.cs:13-584`

_(no documentation)_

**Members**

- `public void TopLevelMenus_AreInMockupOrder()` - _(no documentation)_
- `public void OnlyTheImplementedItems_AreEnabledLeafItems()` - _(no documentation)_
- `public void EnabledItems_HaveACommandOrAreSubmenus()` - _(no documentation)_
- `public void ItemsWithoutCommand_AreDisabled()` - _(no documentation)_
- `public void Separators_AreMarked_AndDisabled()` - _(no documentation)_
- `public void Submenus_AreEnabled_SoTheyCanBeOpened()` - _(no documentation)_
- `public void Quit_CallsShutdown()` - _(no documentation)_
- `public void ZoomItems_ChangeEditorState()` - _(no documentation)_
- `public void FitItem_InvokesFitCallback()` - _(no documentation)_
- `public void WindowItems_ShowTheMatchingInspectorTab()` - _(no documentation)_
- `public void ViewMenu_ZoomItems_ShowGestures()` - _(no documentation)_
- `public void Headers_ChangeWithTheLanguage()` - _(no documentation)_
- `public void UntranslatedHeaders_FallBackToEnglish()` - _(no documentation)_
- `public void LanguageChange_NotifiesAllProperties_SoBindingsRefresh()` - _(no documentation)_
- `public void Separators_StayUntranslated()` - _(no documentation)_
- `public void LanguageSubmenu_IsInTheViewMenu_WithBothLanguages()` - _(no documentation)_
- `public void LanguageItems_AreNeverTranslated()` - _(no documentation)_
- `public void CurrentLanguage_IsChecked()` - _(no documentation)_
- `public void SelectingGerman_SwitchesTheLanguage_AndMovesTheCheckMark()` - _(no documentation)_
- `public void SelectingTheActiveLanguage_ChangesNothing()` - _(no documentation)_
- `public void SettingIsChecked_DoesNotChangeTheState()` - _(no documentation)_
- `public void OtherItems_AreNotCheckable()` - _(no documentation)_
- `public void FileItems_ExecuteTheirCommands(string header, string expected)` - _(no documentation)_
- `public void FileItems_KeepTheirShortcutHints()` - _(no documentation)_
- `public void OpenRecent_WithoutFiles_ShowsOneDisabledTranslatedItem()` - _(no documentation)_
- `public void OpenRecent_AfterFilesWereUsed_ListsThemMostRecentFirstWithTheFolderAsToolTip()` - _(no documentation)_
- `public void OpenRecent_ChoosingAnEntry_RunsTheCommandWithTheFullPath()` - _(no documentation)_
- `public void OpenRecent_AfterTheLastEntryIsRemoved_ShowsTheEmptyItemAgain()` - _(no documentation)_
- `public void OpenRecent_AtMostTenEntries()` - _(no documentation)_
- `public void OpenRecent_LanguageSwitch_KeepsTheEntryNamesAndTranslatesTheEmptyItem()` - _(no documentation)_
- `public void MenuItem_FollowsTheCommandsCanExecute()` - _(no documentation)_
- `public void MenuItem_WithAnAlwaysExecutableCommand_StaysEnabled_AndSubmenusAndSeparatorsKeepTheirRules()` - _(no documentation)_
- `public void SubmenuWithACommandThatCannotExecute_StaysEnabled()` - _(no documentation)_
- `public void UndoAndRedoItems_FollowTheirCommands_AndKeepTheirShortcutHints()` - _(no documentation)_
- `public void EditItems_AreDisabledUntilTheirCommandCanRun_AndRunTheirCommand()` - _(no documentation)_
- `public void EditItems_KeepTheirGestureHints()` - _(no documentation)_

## MenuItemViewModelTests (class)

`public class MenuItemViewModelTests`

Source: `tests/PressToShape.App.Tests/MenuItemViewModelTests.cs:7-74`

_(no documentation)_

**Members**

- `public void RemoveMnemonic_DropsTheMarker_AndKeepsDoubleUnderscoresAsOne(string text, string expected)` - _(no documentation)_
- `public void PlainHeader_OfATranslatedItem_HasNoMarker()` - _(no documentation)_
- `public void PlainHeader_FollowsTheLanguage_AndDropsTheMarkerOfTheTranslation()` - _(no documentation)_
- `public void PlainHeader_OfAFixedText_IsLeftAsItIs()` - _(no documentation)_
- `public void PlainHeader_OfASubmenu_HasNoMarker()` - _(no documentation)_
- `public void PlainHeader_OfASeparator_IsTheSeparatorHeader()` - _(no documentation)_

## MessageDialogHostTests (class)

`public class MessageDialogHostTests`

Source: `tests/PressToShape.App.Tests/MessageDialogHostTests.cs:12-170`

_(no documentation)_

**Members**

- `public void NewHost_ShowsNothing()` - _(no documentation)_
- `public Task ShowAsync_ShowsTheDialog_AndCompletesWithTheChosenResult()` - _(no documentation)_
- `public Task ShowAsync_AfterTheResult_ClosesTheDialogAndNotifies()` - _(no documentation)_
- `public Task SecondRequest_WaitsUntilTheFirstIsClosed()` - _(no documentation)_
- `public Task Dismiss_ClosesTheDialogWithoutAChoice()` - _(no documentation)_
- `public void Dismiss_WithoutADialog_DoesNothing()` - _(no documentation)_
- `public Task ADismissedDialog_CannotCompleteTheNextOne()` - _(no documentation)_
- `public Task AfterADialogIsClosed_TheNextRequestShowsImmediately()` - _(no documentation)_
- `public Task ShowAsync_NullDialog_Throws()` - _(no documentation)_

## MessageDialogViewModelTests (class)

`public class MessageDialogViewModelTests`

Source: `tests/PressToShape.App.Tests/MessageDialogViewModelTests.cs:10-112`

_(no documentation)_

**Members**

- `public void ForUnsavedChanges_HasSaveDontSaveAndCancelInThatOrder()` - _(no documentation)_
- `public void ForUnsavedChanges_SaveIsTheDefaultAndCancelIsTheEscapeButton()` - _(no documentation)_
- `public void ForUnsavedChanges_MessageNamesTheDocument()` - _(no documentation)_
- `public void ForUnsavedChanges_UsesTheCurrentLanguage()` - _(no documentation)_
- `public void ChoosingAButton_RaisesCloseRequestedWithItsResult(int index)` - _(no documentation)_
- `public void ForError_HasASingleDefaultAndCancelOkButton()` - _(no documentation)_
- `public void ForError_ChoosingOk_RaisesCloseRequested()` - _(no documentation)_
- `public void Factories_NullArguments_Throw()` - _(no documentation)_

## NumberFieldViewModelTests (class)

`public class NumberFieldViewModelTests`

Source: `tests/PressToShape.App.Tests/NumberFieldViewModelTests.cs:9-243`

_(no documentation)_

**Members**

- `public void Commit_ValidText_IsApplied(string language, string text, double expected)` - _(no documentation)_
- `public void Commit_InvalidTextInEnglish_IsRefusedAndRevertsTheText(string text)` - _(no documentation)_
- `public void Commit_InvalidTextInGerman_IsRefused(string text)` - _(no documentation)_
- `public void Commit_OutsideTheRange_IsRefused_AndTheLimitsThemselvesAreAccepted()` - _(no documentation)_
- `public void Commit_EqualValue_DoesNotCallApply_AndNormalizesTheText()` - _(no documentation)_
- `public void Commit_AfterApply_ShowsWhatTheDocumentNowHas()` - _(no documentation)_
- `public void Commit_WhenTheDocumentIgnoresTheValue_ShowsTheUnchangedValueAgain()` - _(no documentation)_
- `public void Revert_RestoresTheShownValue()` - _(no documentation)_
- `public void SetValue_FormatsWithAtMostThreeDecimalsAndNoTrailingZeros(double value, string expected)` - _(no documentation)_
- `public void Format_UsesTheCultureOfTheLanguage_AndAChangeRefreshesTheText()` - _(no documentation)_
- `public void Mixed_ShowsAnEmptyText_AndAnyValidInputIsApplied()` - _(no documentation)_
- `public void Mixed_EmptyTextStaysEmptyAndNothingIsApplied()` - _(no documentation)_
- `public void NullValue_DisablesTheField_AndCommitDoesNothing()` - _(no documentation)_
- `public void WhileEditing_NewValuesDoNotReplaceTheTypedText_UntilRevertedOrCommitted()` - _(no documentation)_
- `public void Constructor_RejectsBadArguments()` - _(no documentation)_

## ObjectCommandsTests (class)

`public class ObjectCommandsTests`

Source: `tests/PressToShape.App.Tests/ObjectCommandsTests.cs:13-232`

_(no documentation)_

**Members**

- `public void Commands_NothingSelected_AreDisabled()` - _(no documentation)_
- `public void Group_TwoSelectedShapes_GroupsThemAsOneStepAndUngroupBringsThemBack()` - _(no documentation)_
- `public void Group_OneSelectedShape_IsDisabled()` - _(no documentation)_
- `public void Arrange_BottomShape_BringToFrontAndForwardAreEnabledSendBackwardIsNot()` - _(no documentation)_
- `public void Arrange_SelectionChange_RaisesCanExecuteChanged()` - _(no documentation)_
- `public void Lock_SelectedShape_LocksItAndEnablesUnlockAll()` - _(no documentation)_
- `public void Hide_SelectedShape_HidesItAndEnablesShowAll()` - _(no documentation)_
- `public void ObjectMenu_Item_IsBoundToItsCommandAndShowsTheShortcut(string header, string gesture)` - _(no documentation)_
- `public void ObjectMenu_ItemsFollowTheCommands()` - _(no documentation)_
- `public void UndoOfGroup_ReenablesGroupAndDisablesUngroup()` - _(no documentation)_
- `public void QuickActions_GroupAndUngroupButtons_FollowTheSelection()` - _(no documentation)_

## ObjectEditorTests (class)

`public class ObjectEditorTests`

Source: `tests/PressToShape.App.Tests/ObjectEditorTests.cs:11-389`

_(no documentation)_

**Members**

- `public void Group_TwoShapes_IsOneStepAndSelectsTheGroup()` - _(no documentation)_
- `public void Group_UndoAndRedo_RestoreDocumentAndSelection()` - _(no documentation)_
- `public void CanGroup_OneShapeNothingOrDifferentParents_IsFalse()` - _(no documentation)_
- `public void CanGroup_ShapesOfDifferentLayers_IsFalse()` - _(no documentation)_
- `public void Ungroup_Group_IsOneStepAndSelectsTheMembers()` - _(no documentation)_
- `public void Ungroup_GroupAndPlainShape_KeepsThePlainShapeSelected()` - _(no documentation)_
- `public void Ungroup_UndoRestoresTheGroupSelection()` - _(no documentation)_
- `public void CanUngroup_NoGroupSelected_IsFalseAndNothingHappens()` - _(no documentation)_
- `public void Arrange_BottomShape_IsOneStepWithTheRightTypeAndKeepsTheSelection(ArrangeType type, HistoryStepType step, string expected)` - _(no documentation)_
- `public void Arrange_TopShape_IsOneStepWithTheRightType(ArrangeType type, HistoryStepType step, string expected)` - _(no documentation)_
- `public void Arrange_NothingWouldMove_IsDisabledAndRecordsNoStep()` - _(no documentation)_
- `public void CanArrange_NothingSelected_IsFalse()` - _(no documentation)_
- `public void Arrange_InsideAnEnteredGroup_StaysInTheGroup()` - _(no documentation)_
- `public void LockSelection_LocksAndRemovesTheShapeFromTheSelection()` - _(no documentation)_
- `public void HideSelection_HidesAndRemovesTheShapeFromTheSelection()` - _(no documentation)_
- `public void LockAndHideSelection_NothingSelected_AreDisabled()` - _(no documentation)_
- `public void SetLockedAndSetHidden_ById_UnlockAndShowShapesThatCannotBeSelected()` - _(no documentation)_
- `public void SetLocked_UnknownIdOrNoChange_RecordsNothing()` - _(no documentation)_
- `public void ShowAllAndUnlockAll_ReachNestedObjects()` - _(no documentation)_
- `public void ShowAllAndUnlockAll_NothingToChange_RecordNoStep()` - _(no documentation)_
- `public void Rename_Shape_IsOneStep_EmptyNameClears()` - _(no documentation)_
- `public void Rename_UnknownIdOrSameName_RecordsNothing()` - _(no documentation)_
- `public void MoveAbove_SameParent_IsOneReorderStep()` - _(no documentation)_
- `public void MoveAbove_DifferentParentsOrSameShape_DoesNothing()` - _(no documentation)_
- `public void Constructor_NullState_Throws()` - _(no documentation)_

## OverflowMenuViewModelTests (class)

`public class OverflowMenuViewModelTests`

Source: `tests/PressToShape.App.Tests/OverflowMenuViewModelTests.cs:11-201`

_(no documentation)_

**Members**

- `public void Items_AreTheFileActions_TheEditActions_TheObjectActions_AndTheLanguage_InOrder()` - _(no documentation)_
- `public void TheArrangeSubmenu_HasTheFourArrangeCommands()` - _(no documentation)_
- `public void Items_HaveNoQuitAndNoRecentFiles()` - _(no documentation)_
- `public void Items_ShowNoShortcutTexts()` - _(no documentation)_
- `public void Items_RunTheMatchingCommand()` - _(no documentation)_
- `public void AnItemWhoseCommandCannotRun_IsDisabled_AndFollowsTheCommand()` - _(no documentation)_
- `public void TheLanguageSubmenu_IsAlwaysEnabled_AndListsBothLanguages()` - _(no documentation)_
- `public void ChoosingALanguage_SwitchesIt_MovesTheCheckMark_AndTranslatesTheHeaders()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## OverlayMessageDialogsTests (class)

`public class OverlayMessageDialogsTests`

Source: `tests/PressToShape.App.Tests/OverlayMessageDialogsTests.cs:10-124`

_(no documentation)_

**Members**

- `public OverlayMessageDialogsTests()` - _(no documentation)_
- `public Task AskUnsavedChanges_GivesTheChoiceOfTheButton(int button, int expected)` - _(no documentation)_
- `public Task TheDialogsInsideTheLayout_AreNotCancelledByEscape_BecauseTheBackRequestDoesThat()` - _(no documentation)_
- `public void TheWindowDialogs_StillCancelOnEscape()` - _(no documentation)_
- `public Task AskUnsavedChanges_ShowsTheSameTextAsTheWindowDialog()` - _(no documentation)_
- `public Task AskUnsavedChanges_DismissedWithoutAChoice_CountsAsCancel()` - _(no documentation)_
- `public Task ShowError_ShowsTheMessage_AndCompletesWhenOkIsChosen()` - _(no documentation)_
- `public Task AQuestionAfterAnError_WaitsForTheError()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## PaletteViewModelTests (class)

`public class PaletteViewModelTests`

Source: `tests/PressToShape.App.Tests/PaletteViewModelTests.cs:12-222`

_(no documentation)_

**Members**

- `public void Swatches_NewDocument_IsEmpty()` - _(no documentation)_
- `public void AddSwatch_NewColor_AddsItAsOneAddSwatchStep()` - _(no documentation)_
- `public void AddSwatch_ColorAlreadyThere_RecordsNoStep()` - _(no documentation)_
- `public void RemoveSwatch_ValidIndex_RemovesItAsOneRemoveSwatchStep()` - _(no documentation)_
- `public void RemoveSwatch_IndexOutsideThePalette_DoesNothing(int index)` - _(no documentation)_
- `public void RemoveSwatch_ByColor_RemovesThatSwatchAndIgnoresUnknownColors()` - _(no documentation)_
- `public void AddSwatch_WhilePickerIsOpen_StaysWhenTheColorChangesAgain()` - _(no documentation)_
- `public void IsEmpty_FollowsThePalette()` - _(no documentation)_
- `public void Swatches_FollowUndoRedoAndANewDocument()` - _(no documentation)_
- `public void Swatches_RaisePropertyChangedOnlyWhenThePaletteChanges()` - _(no documentation)_
- `public void Apply_ToFill_ChangesTheFillOfTheSelectionAsOneStep()` - _(no documentation)_
- `public void Apply_ToStroke_ChangesTheStrokeOfTheSelection()` - _(no documentation)_
- `public void Apply_WithNothingSelected_ChangesTheDefaultsOnly()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## PoEntry (record)

`internal sealed record PoEntry : IEquatable<PoEntry>`

Source: `tests/PressToShape.App.Tests/PoFile.cs:8-8`

One translation entry of a .po file.

**Members**

- `public PoEntry(string? Context, string Id, string Translation, bool IsFuzzy)` - One translation entry of a .po file.
- `public string? Context { get; init; }` - _(no documentation)_
- `public string Id { get; init; }` - _(no documentation)_
- `public bool IsFuzzy { get; init; }` - _(no documentation)_
- `public string Translation { get; init; }` - _(no documentation)_

## PoFile (class)

`internal static class PoFile`

Source: `tests/PressToShape.App.Tests/PoFile.cs:14-145`

A small reader for gettext .po files, so tests can check translations without the msgfmt tool. It supports message contexts, multi-line strings, escapes and the fuzzy flag. The header entry (empty id) is skipped.

**Members**

- `public IReadOnlyList<PoEntry> Read(string path)` - _(no documentation)_
- `public IReadOnlyList<PoEntry> Parse(string text)` - _(no documentation)_

## PoFileTests (class)

`public class PoFileTests`

Source: `tests/PressToShape.App.Tests/PoFileTests.cs:6-62`

_(no documentation)_

**Members**

- `public void Parse_ReadsEntries_WithContext_AndSkipsTheHeader()` - _(no documentation)_
- `public void Parse_JoinsMultiLineStrings_AndResolvesEscapes()` - _(no documentation)_
- `public void Parse_MarksFuzzyEntries()` - _(no documentation)_
- `public void Parse_IgnoresObsoleteEntries()` - _(no documentation)_
- `public void Parse_EntryWithoutTranslation_HasAnEmptyTranslation()` - _(no documentation)_

## PropertiesOpacitySliderTests (class)

`public class PropertiesOpacitySliderTests`

Source: `tests/PressToShape.App.Tests/PropertiesOpacitySliderTests.cs:11-95`

_(no documentation)_

**Members**

- `public void SliderValue_FollowsTheShapes()` - _(no documentation)_
- `public void ADragWithManyChanges_IsOneChangeOpacityStep_AndPreviewsLive()` - _(no documentation)_
- `public void ChangesWithoutADragStart_AreIgnored_AndAnUnchangedDragMakesNoStep()` - _(no documentation)_
- `public void ValuesAreClamped_AndNonFiniteValuesIgnored()` - _(no documentation)_
- `public void NothingSelected_NoDragStarts()` - _(no documentation)_

## PropertiesSectionsTests (class)

`public class PropertiesSectionsTests`

Source: `tests/PressToShape.App.Tests/PropertiesSectionsTests.cs:8-64`

_(no documentation)_

**Members**

- `public void For_SelectionTool_ShowsTransformAppearanceAlignAndQuickActions()` - _(no documentation)_
- `public void For_ShapeOrTextTool_ShowsAppearanceOnly()` - _(no documentation)_
- `public void For_PathTool_ShowsAppearanceAndPath()` - _(no documentation)_
- `public void For_ToolWithoutProperties_ShowsNothing()` - _(no documentation)_
- `public void For_EveryTool_IsDefinedAndShowsPathOnlyForPenAndNode()` - _(no documentation)_
- `public void For_UndefinedTool_Throws()` - _(no documentation)_

## PropertiesViewColorTests (class)

`public class PropertiesViewColorTests`

Source: `tests/PressToShape.App.Tests/PropertiesViewColorTests.cs:20-101`

_(no documentation)_

**Members**

- `public void NothingSelected_ColorControlsAreEnabled_StrokeWidthAndOpacityAreNot()` - _(no documentation)_
- `public void ShiftClickOnASwatch_AppliesItToTheStroke_AndAPlainClickToTheFill()` - _(no documentation)_
- `public void TheApp_IncludesTheColorPickerTheme()` - _(no documentation)_
- `public void TheColorPicker_WithTheAppsStyles_HasASize()` - _(no documentation)_

## PropertiesViewModelTests (class)

`public class PropertiesViewModelTests`

Source: `tests/PressToShape.App.Tests/PropertiesViewModelTests.cs:14-807`

_(no documentation)_

**Members**

- `public void CommitPendingEdits_AppliesOnlyTheFieldsBeingEdited_AsOneStepEach()` - _(no documentation)_
- `public void NothingSelected_DisablesEverything_AndShowsTheHeader()` - _(no documentation)_
- `public void NothingSelected_ColorFieldsStayEnabled_AndShowTheDefaults()` - _(no documentation)_
- `public void NothingSelected_ColorEdit_ChangesTheDefaultsAndNotTheDocumentOrHistory()` - _(no documentation)_
- `public void NothingSelected_NoneAndSetColor_ChangeTheDefaults()` - _(no documentation)_
- `public void SelectedShapes_ColorEdit_LeavesTheDefaultsAlone()` - _(no documentation)_
- `public void ClearingTheSelection_ColorFieldsShowTheDefaultsAgain()` - _(no documentation)_
- `public void ColorEdit_ThatChangedTheColor_AddsItToTheRecentColorsOnce()` - _(no documentation)_
- `public void ColorEdit_WithoutChange_AndNone_AddNoRecentColor()` - _(no documentation)_
- `public void SetColor_AddsItToTheRecentColors_AndRaisesPropertyChanged()` - _(no documentation)_
- `public void OneShape_ShowsItsValues()` - _(no documentation)_
- `public void SeveralShapes_ShowTheirBox_RotationIsDisabled_AndDifferentValuesAreMixed()` - _(no documentation)_
- `public void MixedStrokeWidth_ShowsAnEmptyText()` - _(no documentation)_
- `public void Header_FollowsTheLanguage()` - _(no documentation)_
- `public void EditingPosition_MovesTheShape_AsOneChangePositionStep()` - _(no documentation)_
- `public void EditingPosition_OfSeveralShapes_MovesTheGroupBox()` - _(no documentation)_
- `public void EditingSize_ChangesTheShape_AsOneChangeSizeStep()` - _(no documentation)_
- `public void KeepProportions_ChangesTheOtherSideInTheSameRatio()` - _(no documentation)_
- `public void EditingRotation_SetsTheAngle_Normalized()` - _(no documentation)_
- `public void EditingStrokeWidthAndOpacity_AreOneStepEach_WithTheRightType()` - _(no documentation)_
- `public void EditingAMixedField_AppliesToAllShapes()` - _(no documentation)_
- `public void EqualValuesAndInvalidText_CreateNoStep_AndRevert()` - _(no documentation)_
- `public void FieldEdits_UndoAndRedo_RefreshTheFields()` - _(no documentation)_
- `public void GermanCulture_FormatsAndParsesWithAComma()` - _(no documentation)_
- `public void LanguageSwitch_ReformatsTheNumbers()` - _(no documentation)_
- `public void ADragPreview_RefreshesTheFields_InPlace()` - _(no documentation)_
- `public void AFieldBeingEdited_IsNotOverwrittenByARefresh()` - _(no documentation)_
- `public void ColorEdit_PreviewsLive_AndIsOneChangeFillStepWhenThePickerCloses()` - _(no documentation)_
- `public void Pick_WithASelection_SetsTheFieldAsOneStep_AndAddsARecentColor()` - _(no documentation)_
- `public void Pick_WithoutASelection_ChangesTheDefaultsOnly()` - _(no documentation)_
- `public void Pick_WithoutAColorOrCancelled_ChangesNothing()` - _(no documentation)_
- `public void Pick_OfTheColorAlreadySet_RecordsNoStep()` - _(no documentation)_
- `public void ColorPicker_ClosedWithoutAChange_RecordsNoStep()` - _(no documentation)_
- `public void MixedFill_OpeningAndClosingThePickerWithoutAPick_LeavesTheShapesAlone()` - _(no documentation)_
- `public void MixedFill_PickingAColor_SetsAllShapesAsOneStep()` - _(no documentation)_
- `public void MixedStroke_None_RemovesTheStrokeOfAllShapesAsOneStep()` - _(no documentation)_
- `public void StrokeColorEdit_IsAChangeStrokeStep_ForAllSelectedShapes()` - _(no documentation)_
- `public void ClosingThePickerWithoutAChange_MakesNoStep()` - _(no documentation)_
- `public void NoneAndColorOutsideThePicker_AreImmediateSteps()` - _(no documentation)_
- `public void ColorField_WithMixedFills_BecomesAColorOnTheFirstUpdate()` - _(no documentation)_
- `public void ResetTransform_IsOneStep_ClearsTheRotation_AndDisablesItself()` - _(no documentation)_
- `public void SelectionChanges_CreateNoSteps_AndSectionFlagsKeepWorking()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_
- `public void ActiveTool_Changes_RaisesPropertyChangedForEverySectionFlag()` - _(no documentation)_
- `public void Sections_SelectTool_ShowsAllButPath()` - _(no documentation)_
- `public void Sections_PenTool_ShowsAppearanceAndPathOnly()` - _(no documentation)_
- `public void Sections_ToolWithoutProperties_HidesEverythingAndShowsTheHint()` - _(no documentation)_
- `public void ActiveTool_Switched_KeepsExpansionAndFieldValues()` - _(no documentation)_

## RecentColorsTests (class)

`public class RecentColorsTests`

Source: `tests/PressToShape.App.Tests/RecentColorsTests.cs:9-66`

_(no documentation)_

**Members**

- `public void Add_NewColors_PutsMostRecentFirst()` - _(no documentation)_
- `public void Add_ColorAlreadyListed_MovesItToFrontWithoutDuplicate()` - _(no documentation)_
- `public void Add_MoreThanMaxCount_DropsTheOldest()` - _(no documentation)_
- `public void Add_SameRgbWithOtherAlpha_IsAnotherColor()` - _(no documentation)_
- `public void MessageId_SwatchSteps_HaveNames(HistoryStepType type, string expected)` - _(no documentation)_

## RecentFilesTests (class)

`public sealed class RecentFilesTests : IDisposable`

Source: `tests/PressToShape.App.Tests/RecentFilesTests.cs:12-281`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public void WithoutPersisting_AddAndRemove_DoNothing()` - _(no documentation)_
- `public void WithoutPersisting_AStoredList_IsIgnoredAndKept()` - _(no documentation)_
- `public void WithPersisting_IsTheDefault()` - _(no documentation)_
- `public void New_WithoutStoredList_IsEmpty()` - _(no documentation)_
- `public void Add_NewPaths_GoToTheTop()` - _(no documentation)_
- `public void Add_ExistingPath_MovesToTheTopWithoutDuplicate()` - _(no documentation)_
- `public void Add_PathsThatDifferOnlyInCase_AreDifferentEntries()` - _(no documentation)_
- `public void Add_MoreThanTheLimit_DropsTheOldest()` - _(no documentation)_
- `public void Add_BlankPath_IsIgnored(string path)` - _(no documentation)_
- `public void Remove_ExistingPath_RemovesIt()` - _(no documentation)_
- `public void Changed_IsRaisedOnlyForRealChanges()` - _(no documentation)_
- `public void Changes_ArePersistedAndReadByANewInstance()` - _(no documentation)_
- `public void Save_KeepsTheLanguageAndUnknownEntries()` - _(no documentation)_
- `public void New_DamagedStoredValue_GivesAnEmptyList(string json)` - _(no documentation)_
- `public void New_MixedStoredList_KeepsOnlyTheGoodPathsOnceAndAtMostTen()` - _(no documentation)_
- `public void RealSettingsFile_DamagedListDoesNotLoseTheLanguage_AndTheListSurvivesRestarts()` - _(no documentation)_
- `public void New_NullStore_Throws()` - _(no documentation)_
- `public void Paths_AreAReadOnlyViewOfTheCurrentList()` - _(no documentation)_

## RepoPaths (class)

`internal static class RepoPaths`

Source: `tests/PressToShape.App.Tests/RepoPaths.cs:7-28`

Finds files in the repository, relative to the solution root, without hardcoded absolute paths.

**Members**

- `public string Root { get; }` - _(no documentation)_
- `public string Combine(string relativePath)` - _(no documentation)_

## RingGeometryTests (class)

`public class RingGeometryTests`

Source: `tests/PressToShape.App.Tests/RingGeometryTests.cs:7-148`

_(no documentation)_

**Members**

- `public void WedgeAngles_EvenCounts_AreCenteredAtTheTopAndClockwise(int count, int index, double start, double sweep)` - _(no documentation)_
- `public void WedgeAngles_SevenSlots_AllWedgesTogetherFillTheCircle()` - _(no documentation)_
- `public void WedgeAngles_NoSlots_IsEmpty()` - _(no documentation)_
- `public void HitTest_FourSlots_PointInsideAWedge_HitsIt(double dx, double dy, int expected)` - _(no documentation)_
- `public void HitTest_SevenSlots_EveryWedgeCenterHitsItsOwnWedge()` - _(no documentation)_
- `public void HitTest_OnTheLineBetweenTwoWedges_TakesTheNextOneClockwise()` - _(no documentation)_
- `public void HitTest_JustLeftOfTheTop_StillHitsTheFirstWedge()` - _(no documentation)_
- `public void HitTest_CloserThanTheInnerRadius_HitsTheCenter()` - _(no documentation)_
- `public void HitTest_ExactlyOnTheRadii_BelongsToTheWedge()` - _(no documentation)_
- `public void HitTest_FartherThanTheOuterRadius_IsOutside()` - _(no documentation)_
- `public void HitTest_NoSlots_HitsNothingExceptTheCenter()` - _(no documentation)_
- `public void HitTest_NotANumber_IsOutside()` - _(no documentation)_
- `public void SlotCenter_FirstSlot_IsAboveTheCenterAtTheMiddleRadius()` - _(no documentation)_
- `public void SlotCenter_SecondOfFour_IsRightOfTheCenter()` - _(no documentation)_
- `public void ClampCenter_InsideTheArea_IsUnchanged()` - _(no documentation)_
- `public void ClampCenter_NearAnEdge_MovesTheRingInside(double x, double y, double expectedX, double expectedY)` - _(no documentation)_
- `public void ClampCenter_AreaSmallerThanTheRing_CentersTheRing()` - _(no documentation)_

## RingMenuContentTests (class)

`public class RingMenuContentTests`

Source: `tests/PressToShape.App.Tests/RingMenuContentTests.cs:12-112`

_(no documentation)_

**Members**

- `public void Selection_HasTheSevenSlotsInOrder()` - _(no documentation)_
- `public void Selection_Slots_RunTheCommandsOfTheMenus()` - _(no documentation)_
- `public void Selection_ArrangeSlot_IsASubmenuWithTheFourArrangeCommands()` - _(no documentation)_
- `public void EmptyCanvas_HasPasteAndSelectAll()` - _(no documentation)_
- `public void Slots_AreTheSameOnEveryCall()` - _(no documentation)_
- `public void EveryLevel_HasUniqueNonEmptyNamesAndDefinedIcons()` - _(no documentation)_
- `public void Create_WithoutCommands_Throws()` - _(no documentation)_

## RingMenuControlTests (class)

`public class RingMenuControlTests`

Source: `tests/PressToShape.App.Tests/RingMenuControlTests.cs:25-343`

_(no documentation)_

**Members**

- `public void RightClickOnAShape_ShowsTheSevenWedgesOfASelection()` - _(no documentation)_
- `public void RightClickOnEmptyCanvas_ShowsTheTwoWedgesOfTheEmptyCanvas()` - _(no documentation)_
- `public void ClickOnAWedge_RunsItsCommandAndClosesTheRing()` - _(no documentation)_
- `public void ClickOnAnUnavailableWedge_DoesNothingAndKeepsTheRingOpen()` - _(no documentation)_
- `public void ArrangeWedge_SwapsToFourWedges_AndTheCenterGoesBack()` - _(no documentation)_
- `public void Escape_InASubmenuGoesBack_ThenCloses()` - _(no documentation)_
- `public void PressOutsideTheRing_ClosesItWithoutChangingTheDocument()` - _(no documentation)_
- `public void MenuKey_OpensTheRingAndTheArrowKeysAndEnterUseASlot()` - _(no documentation)_
- `public void Wedges_CarryTheirTranslatedNameAndTheirDisabledState()` - _(no documentation)_
- `public void LanguageChange_WhileOpen_RefreshesTheCenterText()` - _(no documentation)_
- `public void RingOpenedNearTheCorner_StaysInsideTheCanvasArea()` - _(no documentation)_
- `public void RingRadii_AreLargerInTheTabletLayout()` - _(no documentation)_
- `public void CanvasBelowTheRing_DoesNotReactToThePointerWhileItIsOpen()` - _(no documentation)_

## RingMenuViewModelTests (class)

`public class RingMenuViewModelTests`

Source: `tests/PressToShape.App.Tests/RingMenuViewModelTests.cs:12-384`

_(no documentation)_

**Members**

- `public RingMenuViewModelTests()` - _(no documentation)_
- `public void Open_WithContent_ShowsTheSlotsAtThePoint()` - _(no documentation)_
- `public void Open_EmptyCanvasMode_ShowsTheOtherSlots()` - _(no documentation)_
- `public void Open_WithoutContent_DoesNothing()` - _(no documentation)_
- `public void Open_ContentWithNoSlots_DoesNothing()` - _(no documentation)_
- `public void Close_ClearsTheSlots_AndTheNextOpeningStartsAtTheTop()` - _(no documentation)_
- `public void Close_WhenClosed_DoesNothing()` - _(no documentation)_
- `public void Activate_CommandSlot_ClosesFirstAndRunsOnce()` - _(no documentation)_
- `public void Activate_UnavailableSlot_DoesNothingAndStaysOpen()` - _(no documentation)_
- `public void Activate_SlotThatBecameUnavailable_DoesNothingAndStaysOpen()` - _(no documentation)_
- `public void Activate_SlotThatBecameUnavailableWithoutNotice_DoesNotRunTheCommand()` - _(no documentation)_
- `public void Activate_IndexOutsideTheSlots_DoesNothing()` - _(no documentation)_
- `public void Activate_WhenClosed_DoesNothing()` - _(no documentation)_
- `public void Activate_SubmenuSlot_ShowsTheChildrenAndCanGoBack()` - _(no documentation)_
- `public void Activate_SlotOfASubmenu_RunsItAndCloses()` - _(no documentation)_
- `public void ActivateCenter_InASubmenu_GoesBackOneLevel()` - _(no documentation)_
- `public void Back_AtTheTop_Closes()` - _(no documentation)_
- `public void Back_TwiceFromASubmenu_GoesBackThenCloses()` - _(no documentation)_
- `public void MoveFocus_FirstMove_LandsOnTheFirstOrTheLastSlot()` - _(no documentation)_
- `public void MoveFocus_AtTheEnds_Wraps()` - _(no documentation)_
- `public void MoveFocus_WhenClosed_DoesNothing()` - _(no documentation)_
- `public void ShowLevel_WithKeyboardFocus_FocusesTheFirstSlotOfTheSubmenu()` - _(no documentation)_
- `public void CenterText_HoverWinsOverFocus_AndIsEmptyOtherwise()` - _(no documentation)_
- `public void Hover_IndexOutsideTheSlots_CountsAsNone()` - _(no documentation)_
- `public void LanguageChange_WhileOpen_RefreshesTheNames()` - _(no documentation)_
- `public void Open_WhileOpen_ReplacesTheRing()` - _(no documentation)_
- `public void CenterActionName_IsCloseAtTheTopAndBackInASubmenu_AndFollowsTheLanguage()` - _(no documentation)_

## RingSlotViewModelTests (class)

`public class RingSlotViewModelTests`

Source: `tests/PressToShape.App.Tests/RingSlotViewModelTests.cs:11-153`

_(no documentation)_

**Members**

- `public void Name_AfterALanguageChangeAndRefresh_IsTranslated()` - _(no documentation)_
- `public void IsAvailable_FollowsCanExecuteChanged()` - _(no documentation)_
- `public void IsAvailable_OfASubmenu_IsTrueWhenOneChildCanRun()` - _(no documentation)_
- `public void Execute_AvailableCommand_RunsItOnce()` - _(no documentation)_
- `public void Execute_UnavailableCommand_DoesNotRun()` - _(no documentation)_
- `public void Execute_CommandThatBecameUnavailableWithoutNotice_DoesNotRun()` - _(no documentation)_
- `public void Execute_SubmenuSlot_RunsNothing()` - _(no documentation)_
- `public void Dispose_StopsListeningToTheCommand()` - _(no documentation)_
- `public void WithCommand_InvalidInput_IsRejected()` - _(no documentation)_
- `public void WithChildren_InvalidInput_IsRejected()` - _(no documentation)_
- `public void Commands_OfNestedSubmenus_ListsEveryCommand()` - _(no documentation)_

## RulerTicksTests (class)

`public class RulerTicksTests`

Source: `tests/PressToShape.App.Tests/RulerTicksTests.cs:7-111`

_(no documentation)_

**Members**

- `public void At100Percent_MajorTicksEvery100_MinorEvery10()` - _(no documentation)_
- `public void MajorTicks_AreAtLeast60PixelsApart(double zoom)` - _(no documentation)_
- `public void MajorStep_FollowsZoom(double zoom, double expectedStep)` - _(no documentation)_
- `public void Origin_ShiftsTicks_AndZeroIsLabeledAtOrigin()` - _(no documentation)_
- `public void Origin_InsideRuler_ShowsNegativeLabelsBeforeZero()` - _(no documentation)_
- `public void TickPositions_StayInsideTheRuler()` - _(no documentation)_
- `public void InvalidInput_ReturnsNoTicks(double zoom, double origin, double length)` - _(no documentation)_
- `public void HugeLength_IsCapped()` - _(no documentation)_
- `public void Output_IsDeterministic()` - _(no documentation)_

## SelectionAndPropertiesIntegrationTests (class)

`public class SelectionAndPropertiesIntegrationTests`

Source: `tests/PressToShape.App.Tests/SelectionAndPropertiesIntegrationTests.cs:20-332`

Selection, handles, properties, clipboard and history working together through the shell, the way a user drives them.

**Members**

- `public void TheWholeFlow_IsOneUndoStepPerAction_AndEveryLayerOfTheAppFollows()` - _(no documentation)_
- `public void RedoAfterTheUndos_BringsEverythingBack()` - _(no documentation)_
- `public void LockingALayer_DeselectsItsShapes_AndTheOverlayAndFieldsFollow()` - _(no documentation)_
- `public Task New_ClearsTheSelection_ButTheClipboardStays()` - _(no documentation)_
- `public void RotationSurvivesAProjectFileRoundTrip_AndSvgExportHasTheTransform()` - _(no documentation)_
- `public void ShiftClick_AndSelectAll_AndEscape_WorkOnTheCanvas()` - _(no documentation)_
- `public void ThePropertiesFieldAndTheCanvasHandle_AgreeOnTheSameShape()` - _(no documentation)_
- `public void ThousandShapes_SelectAllAndAHandleDrag_StayResponsiveAndConsistent()` - _(no documentation)_

## SelectionEditorClipboardTests (class)

`public class SelectionEditorClipboardTests`

Source: `tests/PressToShape.App.Tests/SelectionEditorClipboardTests.cs:12-286`

_(no documentation)_

**Members**

- `public void SelectAll_SelectsEverythingSelectable_WithoutAStep()` - _(no documentation)_
- `public void Deselect_ClearsTheSelection()` - _(no documentation)_
- `public void Copy_FillsTheClipboardInDocumentOrder_AndKeepsTheDocument()` - _(no documentation)_
- `public void Copy_WithoutSelection_DoesNothing()` - _(no documentation)_
- `public void Cut_IsOneStep_RemovesTheShapes_AndFillsTheClipboard()` - _(no documentation)_
- `public void Paste_TwiceShiftsTenThenTwenty_WithNewIds_OnTopOfTheActiveLayer()` - _(no documentation)_
- `public void Paste_InPlace_KeepsThePosition_AndDoesNotAdvanceTheOffset()` - _(no documentation)_
- `public void Paste_KeepsRelativePositionsAndStackingOrder()` - _(no documentation)_
- `public void NewCopy_RestartsThePasteOffset()` - _(no documentation)_
- `public void Paste_IsDisabledForAnEmptyClipboardOrAHiddenOrLockedActiveLayer()` - _(no documentation)_
- `public void Duplicate_PutsCopiesOnTopOfTheirOwnLayers_ShiftedTen_AndSelectsThem()` - _(no documentation)_
- `public void Duplicate_DoesNotTouchTheClipboard()` - _(no documentation)_
- `public void EveryOperation_UndoesInOneStep()` - _(no documentation)_
- `public void Clipboard_RaisesChangedOnSetAndClear()` - _(no documentation)_
- `public void Constructor_NullState_Throws()` - _(no documentation)_

## SelectionEditorEditTests (class)

`public class SelectionEditorEditTests`

Source: `tests/PressToShape.App.Tests/SelectionEditorEditTests.cs:11-185`

_(no documentation)_

**Members**

- `public void Nudge_MovesAllSelectedShapes_AsOneStep()` - _(no documentation)_
- `public void Nudge_Repeated_MergesIntoOneStep_AndUndoRestoresTheStart()` - _(no documentation)_
- `public void Nudge_AfterAnotherEdit_StartsANewStep()` - _(no documentation)_
- `public void Nudge_WithAnotherSelection_StartsANewStep()` - _(no documentation)_
- `public void Nudge_AfterUndo_StartsANewStep()` - _(no documentation)_
- `public void Nudge_WithoutSelectionOrDistance_DoesNothing()` - _(no documentation)_
- `public void Nudge_CanLeaveTheArtboard()` - _(no documentation)_
- `public void Apply_ChangesAllSelectedShapes_AndKeepsTheSelection()` - _(no documentation)_
- `public void Apply_UnchangedResult_RecordsNoStep()` - _(no documentation)_
- `public void ApplyTransform_ReplacesShapesTogether_AndKeepsIds()` - _(no documentation)_
- `public void ApplyTransform_Unchanged_RecordsNoStep_AndWithoutSelectionDoesNothing()` - _(no documentation)_
- `public void ApplyTransform_WrongShapes_Throws()` - _(no documentation)_
- `public void NullDelegates_Throw()` - _(no documentation)_

## SelectionEditorGroupTests (class)

`public class SelectionEditorGroupTests`

Source: `tests/PressToShape.App.Tests/SelectionEditorGroupTests.cs:13-274`

_(no documentation)_

**Members**

- `public void Nudge_Group_MovesAllLeavesAsOneStep()` - _(no documentation)_
- `public void Apply_Group_ChangesTheShapesBelowAndNotTheGroup()` - _(no documentation)_
- `public void Apply_GroupAndPlainShape_ChangesAllLeaves()` - _(no documentation)_
- `public void ApplyTransformWithFrame_ResizeGroup_ScalesLeavesAndKeepsTheLayout()` - _(no documentation)_
- `public void ApplyTransformWithFrame_ResizeGroupWithTurnedLeaf_ScalesUniformly()` - _(no documentation)_
- `public void ApplyTransformWithFrame_RotateGroup_TurnsTheLeavesAndTheGroupStaysUpright()` - _(no documentation)_
- `public void ApplyTransform_ReturningOtherShapes_Throws()` - _(no documentation)_
- `public void FrameOf_GroupWithOneTurnedLeaf_IsUprightAndNotSingle()` - _(no documentation)_
- `public void FrameOf_SinglePlainTurnedShape_StaysSingle()` - _(no documentation)_
- `public void SelectionValues_Group_GivesFrameAndMixedAppearanceOfTheLeaves()` - _(no documentation)_
- `public void Paste_CopiedGroup_GetsNewIdsForTheWholeSubtree()` - _(no documentation)_
- `public void Paste_InPlace_KeepsPosition()` - _(no documentation)_
- `public void Duplicate_Group_AddsAMovedCopyWithNewIds()` - _(no documentation)_
- `public void Duplicate_MemberOfAnEnteredGroup_LandsInTheSameGroup()` - _(no documentation)_
- `public void Cut_GroupMembers_RemovesTheGroupWhenItBecomesEmpty()` - _(no documentation)_
- `public void Cut_Group_PutsTheWholeGroupOnTheClipboard()` - _(no documentation)_

## SelectionGeometryTests (class)

`public class SelectionGeometryTests`

Source: `tests/PressToShape.App.Tests/SelectionGeometryTests.cs:8-230`

_(no documentation)_

**Members**

- `public void FrameOf_NoShapes_IsNull()` - _(no documentation)_
- `public void FrameOf_OneShape_IsItsOwnBoxAndRotation()` - _(no documentation)_
- `public void FrameOf_SeveralShapes_IsTheUprightUnionOfTheRotatedBounds()` - _(no documentation)_
- `public void HandlePosition_Upright_AtZoom100()` - _(no documentation)_
- `public void HandlePosition_RotateHandleDistanceIsConstantOnTheScreen()` - _(no documentation)_
- `public void HandlePosition_Rotated90_TurnsClockwise()` - _(no documentation)_
- `public void HandlePosition_Rotated30_StaysOnTheTurnedFrame()` - _(no documentation)_
- `public void HitHandle_UsesAGrabSizeThatIsConstantOnTheScreen(double zoom)` - _(no documentation)_
- `public void HitHandle_AFingerSizedGrabArea_ReachesFurtherThanTheDefault_ConstantOnTheScreen(double zoom)` - _(no documentation)_
- `public void HitHandle_AFingerSizedGrabArea_DoesNotCoverTheMiddleOfASmallShape()` - _(no documentation)_
- `public void HitHandle_AFingerSizedGrabArea_NeverShrinksBelowTheMouseGrabArea()` - _(no documentation)_
- `public void HitHandle_AnUnusableGrabSize_FallsBackToTheMouseSizeOrNothing_WithoutThrowing(double grabSize)` - _(no documentation)_
- `public void HitHandle_AwayFromAllHandles_IsNull()` - _(no documentation)_
- `public void HitHandle_RotateHandleIsOutsideTheFrame()` - _(no documentation)_
- `public void HitHandle_RotatedFrame_FollowsTheTurnedHandles()` - _(no documentation)_
- `public void HitHandle_EveryHandleIsReachableByItsOwnCenter_EvenOnATinyFrame()` - _(no documentation)_
- `public void HitHandle_EqualDistance_PrefersCornersOverEdges()` - _(no documentation)_
- `public void HandleDirectionDegrees_IsTheTurnedDirection()` - _(no documentation)_
- `public void NullArguments_Throw()` - _(no documentation)_

## SelectionOverlayGroupTests (class)

`public class SelectionOverlayGroupTests`

Source: `tests/PressToShape.App.Tests/SelectionOverlayGroupTests.cs:10-70`

_(no documentation)_

**Members**

- `public void SelectedGroup_HasOneUprightOutlineAndAnUprightFrameWithHandles()` - _(no documentation)_
- `public void HiddenSelectedGroup_IsNotPartOfTheOverlay()` - _(no documentation)_
- `public void EnteredGroup_HasAnOutlineEvenWithoutSelection()` - _(no documentation)_
- `public void EnteredGroup_IsPartOfTheOverlayForOtherToolsToo()` - _(no documentation)_
- `public void NoEnteredGroupOrUnknownId_HasNoEnteredOutline()` - _(no documentation)_

## SelectionOverlayTests (class)

`public class SelectionOverlayTests`

Source: `tests/PressToShape.App.Tests/SelectionOverlayTests.cs:10-173`

_(no documentation)_

**Members**

- `public void OneShape_HasAnOutline_ABoxAndNineHandles()` - _(no documentation)_
- `public void ZoomAndOrigin_MapTheOutline()` - _(no documentation)_
- `public void RotatedShape_HasATurnedOutline()` - _(no documentation)_
- `public void RotatedNonSquareShape_TurnsItsCorners()` - _(no documentation)_
- `public void SeveralShapes_HaveOneOutlineEachAndAnUprightBoxAroundAll()` - _(no documentation)_
- `public void HandleSizeStaysEightScreenPixelsAtEveryZoom()` - _(no documentation)_
- `public void HandlesSitOnTheBoxAndTheRotateHandleIs24ScreenPixelsAboveIt()` - _(no documentation)_
- `public void NoSelection_GivesNothing()` - _(no documentation)_
- `public void OtherTools_GiveOutlinesOnly()` - _(no documentation)_
- `public void TheDraggedBox_IsMappedToScreenPixels_EvenWithoutASelection()` - _(no documentation)_
- `public void HiddenOrLockedShapes_AndUnknownIds_GiveNothing()` - _(no documentation)_
- `public void NullArguments_Throw()` - _(no documentation)_

## SelectionQueriesGroupTests (class)

`public class SelectionQueriesGroupTests`

Source: `tests/PressToShape.App.Tests/SelectionQueriesGroupTests.cs:10-201`

_(no documentation)_

**Members**

- `public void TargetFor_TopLevelHit_IsTheOutermostGroup()` - _(no documentation)_
- `public void TargetFor_PlainShape_IsTheShapeItself()` - _(no documentation)_
- `public void TargetFor_EnteredOuterGroup_IsTheDirectChild()` - _(no documentation)_
- `public void TargetFor_EnteredDirectParent_IsTheLeaf()` - _(no documentation)_
- `public void TargetFor_LeafOutsideTheEnteredGroup_IsNull()` - _(no documentation)_
- `public void Selectable_TopLevel_ListsGroupsNotTheirMembers()` - _(no documentation)_
- `public void Selectable_EnteredGroup_ListsOnlyItsChildren()` - _(no documentation)_
- `public void Selectable_HiddenAndLockedObjects_AreLeftOut()` - _(no documentation)_
- `public void Selectable_EnteredGroupThatIsHiddenOrLocked_IsEmpty()` - _(no documentation)_
- `public void ShapesTouching_Box_ReturnsGroupsAtTopLevelAndChildrenInside()` - _(no documentation)_
- `public void IsSelectable_ChildOfHiddenOrLockedGroup_IsFalse()` - _(no documentation)_
- `public void IsSelectable_GroupAndChildInPlainGroup_IsTrue()` - _(no documentation)_
- `public void SelectedLeaves_Group_ReturnsItsLeavesInOrder()` - _(no documentation)_
- `public void SelectedLeaves_GroupAndItsChild_ListsTheChildOnce()` - _(no documentation)_
- `public void SelectedShapes_NestedIds_AreFoundInDocumentOrder()` - _(no documentation)_
- `public void SelectableWithParents_ReportsTheClosestGroup()` - _(no documentation)_

## SelectionQueriesTests (class)

`public class SelectionQueriesTests`

Source: `tests/PressToShape.App.Tests/SelectionQueriesTests.cs:10-152`

_(no documentation)_

**Members**

- `public void IsSelectable_VisibleUnlockedLayer_IsTrue()` - _(no documentation)_
- `public void IsSelectable_HiddenLockedOrUnknown_IsFalse()` - _(no documentation)_
- `public void Selectable_ListsShapesInDocumentOrder_AndSkipsHiddenAndLockedLayers()` - _(no documentation)_
- `public void ShapesTouching_RectangleInsideAndOutside()` - _(no documentation)_
- `public void ShapesTouching_PartlyInside_Counts()` - _(no documentation)_
- `public void ShapesTouching_EdgeContact_Counts()` - _(no documentation)_
- `public void ShapesTouching_EverythingInsideAndTheBoxAroundAll()` - _(no documentation)_
- `public void ShapesTouching_NegativeWidthAndHeight_AreNormalized()` - _(no documentation)_
- `public void ShapesTouching_RotatedShape_UsesTheTurnedBounds()` - _(no documentation)_
- `public void ShapesTouching_ZeroSizeRectangle_StillFindsAShapeUnderThePoint()` - _(no documentation)_
- `public void ShapesTouching_SkipsHiddenAndLockedLayers()` - _(no documentation)_
- `public void SelectedShapes_AreInDocumentOrder_AndUnknownIdsAreIgnored()` - _(no documentation)_
- `public void EmptyDocumentAndEmptyInputs_GiveEmptyResults()` - _(no documentation)_
- `public void ThousandShapes_AreHandledInOnePass()` - _(no documentation)_
- `public void Arguments_Null_Throw()` - _(no documentation)_

## SelectionSummaryTextTests (class)

`public class SelectionSummaryTextTests`

Source: `tests/PressToShape.App.Tests/SelectionSummaryTextTests.cs:7-46`

_(no documentation)_

**Members**

- `public void Format_English_GivesTheCountText(int count, string expected)` - _(no documentation)_
- `public void Format_German_UsesTheTranslations()` - _(no documentation)_
- `public void Format_ChangesWithTheLanguageOfTheLocalizer()` - _(no documentation)_

## SelectionTransformsTests (class)

`public class SelectionTransformsTests`

Source: `tests/PressToShape.App.Tests/SelectionTransformsTests.cs:10-537`

_(no documentation)_

**Members**

- `public void Resize_Upright_EveryHandleKeepsTheOppositePointAndFollowsThePointer()` - _(no documentation)_
- `public void Resize_Rotated_OppositePointStaysFixedAndSizeFollowsThePointerInTheTurnedFrame(double rotation)` - _(no documentation)_
- `public void Resize_KeepProportions_CornerKeepsTheRatio()` - _(no documentation)_
- `public void Resize_KeepProportions_EdgeScalesTheOtherSideAroundItsCenter()` - _(no documentation)_
- `public void Resize_FromCenter_KeepsTheCenter()` - _(no documentation)_
- `public void Resize_PastTheOppositeEdge_Flips()` - _(no documentation)_
- `public void Resize_ToTheOppositeEdge_GivesZeroSize_NotNegative()` - _(no documentation)_
- `public void Resize_LeavesStrokeAndOpacityAlone()` - _(no documentation)_
- `public void Resize_RotateHandleOrNonFiniteInput_ReturnsTheSameList()` - _(no documentation)_
- `public void Resize_DoesNotChangeTheInput()` - _(no documentation)_
- `public void Resize_Group_ScalesPositionsAndSizesTogether_AndKeepsStrokes()` - _(no documentation)_
- `public void Resize_Group_FromCenter_StaysCentered()` - _(no documentation)_
- `public void Resize_Group_FlipsPastTheOppositeEdge()` - _(no documentation)_
- `public void Resize_GroupWithARotatedShape_ScalesUniformly()` - _(no documentation)_
- `public void Rotate_Single_TheHandleFollowsThePointer()` - _(no documentation)_
- `public void Rotate_Single_FromANonZeroStartAngle_AddsTheMovement()` - _(no documentation)_
- `public void Rotate_Single_SnapsToFifteenDegrees()` - _(no documentation)_
- `public void Rotate_Group_TurnsPositionsAroundTheFrameCenter_AndAddsToEachRotation()` - _(no documentation)_
- `public void Rotate_Group_SnapsTheMovement()` - _(no documentation)_
- `public void Rotate_PointerAtTheCenterOrNonFinite_ReturnsTheSameList()` - _(no documentation)_
- `public void Move_MovesEveryShape_AndIgnoresNonFiniteDistances()` - _(no documentation)_
- `public void SetPosition_Single_MovesTheUnturnedBoxToXY()` - _(no documentation)_
- `public void SetPosition_OnlyOneCoordinate_KeepsTheOther()` - _(no documentation)_
- `public void SetPosition_Group_MovesTheBoxAroundThemAndKeepsTheLayout()` - _(no documentation)_
- `public void SetSize_Single_KeepsTheTopLeftCorner()` - _(no documentation)_
- `public void SetSize_Single_Rotated_KeepsTheTopLeftCornerOfTheTurnedBox()` - _(no documentation)_
- `public void SetSize_KeepProportions_UsesTheRatioOfTheFrame()` - _(no documentation)_
- `public void SetSize_Group_ScalesAboutTheTopLeftOfTheFrame()` - _(no documentation)_
- `public void SetSize_ZeroNegativeAndNonFinite()` - _(no documentation)_
- `public void SetSize_FromZeroWidth_CanGrowASingleShape()` - _(no documentation)_
- `public void SetRotation_Single_SetsTheAngle_Normalized()` - _(no documentation)_
- `public void SetRotation_Group_TurnsTheGroupByTheAngle()` - _(no documentation)_
- `public void ResetRotation_ClearsEveryShapesRotationAroundItsOwnCenter()` - _(no documentation)_
- `public void RandomOperations_KeepEveryNumberFiniteAndSizesNotNegative()` - _(no documentation)_
- `public void NullArguments_Throw()` - _(no documentation)_

## SelectionValuesTests (class)

`public class SelectionValuesTests`

Source: `tests/PressToShape.App.Tests/SelectionValuesTests.cs:9-107`

_(no documentation)_

**Members**

- `public void From_EmptyList_IsNull()` - _(no documentation)_
- `public void From_OneShape_GivesItsBoxRotationAndAppearance()` - _(no documentation)_
- `public void From_RotatedShape_UsesTheUnturnedBox()` - _(no documentation)_
- `public void From_SeveralShapes_GivesTheBoxAroundThemAndRotationZero()` - _(no documentation)_
- `public void From_SeveralEqualShapes_IsNotMixed()` - _(no documentation)_
- `public void From_DifferentAppearances_AreMixedPerProperty()` - _(no documentation)_
- `public void From_NoFillOnAllShapes_IsNullAndNotMixed()` - _(no documentation)_
- `public void Mixed_Of_HandlesEmptyOneAndManyValues()` - _(no documentation)_
- `public void From_Null_Throws()` - _(no documentation)_

## SettingsStoreTests (class)

`public sealed class SettingsStoreTests : IDisposable`

Source: `tests/PressToShape.App.Tests/SettingsStoreTests.cs:9-157`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public void Load_WithoutFile_ReturnsDefaults()` - _(no documentation)_
- `public void SaveThenLoad_RoundTrips_AndCreatesTheFolder()` - _(no documentation)_
- `public void Load_CorruptFile_ReturnsDefaults(string content)` - _(no documentation)_
- `public void Load_FileWithComments_IsAccepted()` - _(no documentation)_
- `public void UnknownEntries_SurviveARoundTrip()` - _(no documentation)_
- `public void Save_ToAnImpossiblePath_DoesNotThrow()` - _(no documentation)_
- `public void FailedWrite_LeavesAnExistingFileUntouched()` - _(no documentation)_
- `public void Save_LeavesNoTemporaryFiles()` - _(no documentation)_
- `public void DefaultPath_IsInsideTheApplicationDataFolder()` - _(no documentation)_
- `public void Constructor_RequiresAPath()` - _(no documentation)_

## SourceText (record)

`internal sealed record SourceText : IEquatable<SourceText>`

Source: `tests/PressToShape.App.Tests/SourceTexts.cs:10-10`

A translatable text found in the sources: its message id, optional context and where it was found.

**Members**

- `public SourceText(string? Context, string Text, string File)` - A translatable text found in the sources: its message id, optional context and where it was found.
- `public string? Context { get; init; }` - _(no documentation)_
- `public string File { get; init; }` - _(no documentation)_
- `public string Text { get; init; }` - _(no documentation)_

## SourceTexts (class)

`internal static class SourceTexts`

Source: `tests/PressToShape.App.Tests/SourceTexts.cs:16-63`

Finds the translatable texts in the App project, the way a gettext extraction would: T("..."), N("...") and TC("context", "...") in C#, and {loc:T '...'} in XAML.

**Members**

- `public IReadOnlyList<SourceText> FindAll()` - _(no documentation)_

## StatusBarViewModelTests (class)

`public class StatusBarViewModelTests`

Source: `tests/PressToShape.App.Tests/StatusBarViewModelTests.cs:10-253`

_(no documentation)_

**Members**

- `public void Defaults_ShowNothingSelected_And100Percent()` - _(no documentation)_
- `public void FormatZoom_UsesInvariantCulture(double percent, string expected)` - _(no documentation)_
- `public void ZoomCommands_ChangeTheZoomText()` - _(no documentation)_
- `public void SetZoomCommand_AppliesAPreset()` - _(no documentation)_
- `public void ZoomChangedElsewhere_UpdatesTheText()` - _(no documentation)_
- `public void FitCommand_InvokesCallback()` - _(no documentation)_
- `public void HandButton_ActivatesHandTool()` - _(no documentation)_
- `public void Presets_AreTheZoomLevels()` - _(no documentation)_
- `public void PresetCommand_SetsTheZoom()` - _(no documentation)_
- `public void SelectionSummaryChange_RaisesNotification()` - _(no documentation)_
- `public void NothingSelected_IsTranslated_AndRefreshesOnLanguageChange()` - _(no documentation)_
- `public void WithASelection_TheSummaryShowsTheCount_AndGoesBackWhenCleared()` - _(no documentation)_
- `public void SeveralShapes_ShowTheTranslatedCount(int count, string expected)` - _(no documentation)_
- `public void SelectionTextFollowsTheSelectedCountOneByOne()` - _(no documentation)_
- `public void ZoomText_UsesTheDecimalSeparatorOfTheLanguage()` - _(no documentation)_
- `public void WholeNumberZoom_LooksTheSameInBothLanguages()` - _(no documentation)_
- `public void FormatZoom_GermanCulture_UsesAComma()` - _(no documentation)_
- `public void SelectedShapeWithoutDescription_ShowsOneSelected_AndTranslatesIt()` - _(no documentation)_

## StorageFileRegistryTests (class)

`public class StorageFileRegistryTests`

Source: `tests/PressToShape.App.Tests/StorageFileRegistryTests.cs:6-88`

_(no documentation)_

**Members**

- `public void Register_ReturnsAReferenceThatEndsWithTheFileName()` - _(no documentation)_
- `public void TryGet_ReturnsTheRegisteredFile()` - _(no documentation)_
- `public void Register_TheSameFileTwice_GivesTwoWorkingReferences()` - _(no documentation)_
- `public void TryGet_UnknownReference_ReturnsFalse(string reference)` - _(no documentation)_
- `public void Register_NameWithSeparatorsOrEmpty_StillEndsWithASingleName(string name, string expectedEnd)` - _(no documentation)_
- `public void IsReference_RecognizesOnlyTheRegistryPrefix(string path, bool expected)` - _(no documentation)_
- `public void IsReference_ForARegisteredFile_IsTrue()` - _(no documentation)_

## StorageFileStoreTests (class)

`public class StorageFileStoreTests`

Source: `tests/PressToShape.App.Tests/StorageFileStoreTests.cs:11-163`

_(no documentation)_

**Members**

- `public StorageFileStoreTests()` - _(no documentation)_
- `public Task Read_PickedFile_ReturnsTheBytes()` - _(no documentation)_
- `public Task Read_EmptyFile_ReturnsNoBytes()` - _(no documentation)_
- `public Task Read_FileExactlyAtTheLimit_IsRead()` - _(no documentation)_
- `public Task Read_FileOverTheLimit_ThrowsInvalidDataException()` - _(no documentation)_
- `public Task Read_LargeFile_IsReadInSeveralChunks()` - _(no documentation)_
- `public Task Read_UnknownReference_ThrowsFileNotFoundException()` - _(no documentation)_
- `public Task Read_PlatformFailure_BecomesAnIOException()` - _(no documentation)_
- `public Task Write_ReplacesTheContent()` - _(no documentation)_
- `public Task Write_NewEmptyFile_WritesTheBytes()` - _(no documentation)_
- `public Task Write_FailingMidway_WhenTheRestoreFailsToo_StillThrowsTheOriginalError()` - _(no documentation)_
- `public Task Write_FailingMidway_WhenTheRestoreCanSucceed_LeavesTheOldContent()` - _(no documentation)_
- `public Task Write_WhenTheOldContentCannotBeRead_StillWrites()` - _(no documentation)_
- `public Task Write_CancelledBeforeStarting_DoesNotTouchTheFile()` - _(no documentation)_
- `public Task Write_UnknownReference_ThrowsFileNotFoundException()` - _(no documentation)_
- `public Task Write_NullBytes_Throws()` - _(no documentation)_
- `public void Constructor_NullRegistry_Throws()` - _(no documentation)_

## SwitchableCommand (class)

`internal sealed class SwitchableCommand : ICommand`

Source: `tests/PressToShape.App.Tests/TestCommands.cs:7-26`

A command whose CanExecute can be switched by the test.

**Members**

- `public SwitchableCommand(bool canExecute)` - _(no documentation)_
- `public EventHandler? CanExecuteChanged` - _(no documentation)_
- `public int Executions { get; private set; }` - _(no documentation)_
- `public void Set(bool canExecute)` - _(no documentation)_
- `public bool CanExecute(object? parameter)` - _(no documentation)_
- `public void Execute(object? parameter)` - _(no documentation)_

## TabletColorSwatchViewModelTests (class)

`public class TabletColorSwatchViewModelTests`

Source: `tests/PressToShape.App.Tests/TabletColorSwatchViewModelTests.cs:10-192`

_(no documentation)_

**Members**

- `public void NothingSelected_ShowsTheColorsOfTheNextShape()` - _(no documentation)_
- `public void ASelectedShape_ShowsItsFillAndStroke()` - _(no documentation)_
- `public void AShapeWithoutFillOrStroke_HasNone()` - _(no documentation)_
- `public void ShapesWithDifferentFills_ShowNoFill_ButTheSameStrokeStays()` - _(no documentation)_
- `public void AColorChangedInThePanel_IsShownAndNotified()` - _(no documentation)_
- `public void Undo_ShowsTheOldColorAgain()` - _(no documentation)_
- `public void ANewSelection_ShowsItsColors_AndNotifiesTheFlags()` - _(no documentation)_
- `public void Open_FromClosed_OpensTheProperties()` - _(no documentation)_
- `public void Open_WithAnotherTabOpen_SwitchesToTheProperties()` - _(no documentation)_
- `public void Open_WhenAlreadyOnTheProperties_KeepsThePanelOpen()` - _(no documentation)_
- `public void Open_ExpandsACollapsedAppearanceSection()` - _(no documentation)_
- `public void Name_FollowsTheLanguage()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## TabletPanelViewModelTests (class)

`public class TabletPanelViewModelTests`

Source: `tests/PressToShape.App.Tests/TabletPanelViewModelTests.cs:9-195`

_(no documentation)_

**Members**

- `public void New_IsClosedAndHasNoContent()` - _(no documentation)_
- `public void Toggle_WhenClosed_OpensTheTab()` - _(no documentation)_
- `public void Toggle_TheOpenTab_ClosesThePanel()` - _(no documentation)_
- `public void Toggle_AnotherTab_SwitchesTheContentAndStaysOpen()` - _(no documentation)_
- `public void Toggle_Assets_IsIgnored()` - _(no documentation)_
- `public void Toggle_Assets_WhileOpen_KeepsTheOpenTab()` - _(no documentation)_
- `public void Open_WhenClosed_OpensTheTab()` - _(no documentation)_
- `public void Open_AnotherTab_Switches()` - _(no documentation)_
- `public void Open_TheOpenTab_KeepsItOpen()` - _(no documentation)_
- `public void Open_Assets_IsIgnored()` - _(no documentation)_
- `public void Title_FollowsTheTabAndTheLanguage()` - _(no documentation)_
- `public void PropertyChanged_IsRaisedForOpenStateContentAndTitle()` - _(no documentation)_
- `public void Close_AppliesAFieldThatIsStillBeingTypedIn()` - _(no documentation)_
- `public void Close_WhenNothingIsBeingTypedIn_ChangesNothing()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## TabletShellViewModelTests (class)

`public class TabletShellViewModelTests`

Source: `tests/PressToShape.App.Tests/TabletShellViewModelTests.cs:13-438`

_(no documentation)_

**Members**

- `public void Shell_CreatesAllRegions()` - _(no documentation)_
- `public void TheColorSwatch_OpensThePanelOfTheShell_OnTheSharedProperties()` - _(no documentation)_
- `public void ATool_ChosenInTheToolColumn_IsTheActiveToolOfTheEditor()` - _(no documentation)_
- `public void AnEdit_ShowsInTheToolbar_TheTitle_AndTheCanvas()` - _(no documentation)_
- `public void ASelection_ShowsInThePropertiesTab()` - _(no documentation)_
- `public void TheZoomOfTheToolbar_IsTheZoomOfTheEditor()` - _(no documentation)_
- `public Task Leave_WithoutChanges_EndsTheAppWithoutAQuestion()` - _(no documentation)_
- `public Task Leave_WithChanges_Save_SavesAndEndsTheApp()` - _(no documentation)_
- `public Task Leave_WithChanges_DontSave_EndsTheAppWithoutSaving()` - _(no documentation)_
- `public Task Leave_WithChanges_Cancel_KeepsTheAppAndTheDrawing()` - _(no documentation)_
- `public Task Leave_WithChanges_SaveFailing_KeepsTheAppAndShowsTheError()` - _(no documentation)_
- `public Task Leave_WhileADialogIsOpen_ClosesItAsCancel_AndTheAppStays()` - _(no documentation)_
- `public Task Leave_WithTheOverlayDialog_DontSaveEndsTheApp()` - _(no documentation)_
- `public Task Leave_WithTheOverlayDialog_BackWhileTheQuestionIsOpen_CancelsIt()` - _(no documentation)_
- `public Task Leave_ARequestAfterACancel_AsksAgain()` - _(no documentation)_
- `public Task Back_WhileADialogIsOpen_ClosesOnlyTheDialog()` - _(no documentation)_
- `public Task Back_WhileAToolFlyoutIsOpen_ClosesOnlyTheFlyout()` - _(no documentation)_
- `public Task Back_WhileThePanelIsOpen_ClosesOnlyThePanel_EvenWithUnsavedChanges()` - _(no documentation)_
- `public Task Back_WithNothingOpen_LeavesLikeBefore()` - _(no documentation)_
- `public Task Back_WithNothingOpenAndUnsavedChanges_Asks()` - _(no documentation)_
- `public Task Back_PressedRepeatedly_ClosesFlyoutThenPanelThenAsks()` - _(no documentation)_
- `public Task NewDocument_WhileTheLayersPanelIsOpen_KeepsThePanelAndShowsTheNewDocument()` - _(no documentation)_
- `public Task RecentFiles_AreKept_ByDefault()` - _(no documentation)_
- `public Task RecentFiles_AreNotKept_WhenTheServicesSayNo()` - _(no documentation)_
- `public void TheOverflowMenu_HasNoRecentFilesList()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## TabletToolColumnViewModelTests (class)

`public class TabletToolColumnViewModelTests`

Source: `tests/PressToShape.App.Tests/TabletToolColumnViewModelTests.cs:8-72`

_(no documentation)_

**Members**

- `public void Groups_AreTheSixGroupsInOrder()` - _(no documentation)_
- `public void ColorSwatch_IsGiven_AndNullIsRefused()` - _(no documentation)_
- `public void OpeningAFlyout_ClosesTheOtherOne()` - _(no documentation)_
- `public void HasOpenFlyout_FollowsTheGroups()` - _(no documentation)_
- `public void CloseFlyouts_ClosesTheOpenOne()` - _(no documentation)_

## TabletToolbarViewModelTests (class)

`public class TabletToolbarViewModelTests`

Source: `tests/PressToShape.App.Tests/TabletToolbarViewModelTests.cs:10-245`

_(no documentation)_

**Members**

- `public void NewDocument_IsShownAsUntitled_WithoutAStar()` - _(no documentation)_
- `public void AnEdit_PutsAStarInFront_AndNotifies()` - _(no documentation)_
- `public Task AfterSaveAs_TheTitleIsTheFileName_WithoutAStar()` - _(no documentation)_
- `public void TheTitle_FollowsTheLanguage()` - _(no documentation)_
- `public void UndoAndRedoButtons_AreNamed_AndDisabledWithoutHistory()` - _(no documentation)_
- `public void UndoAndRedoButtons_FollowTheHistory_AndChangeTheDocument()` - _(no documentation)_
- `public void UndoButtonChangesNotify_WhenTheHistoryChanges()` - _(no documentation)_
- `public void Zoom_ShowsTheZoomOfTheEditor_AndItsCommandsWork()` - _(no documentation)_
- `public void TheOverflowMenu_IsTheOneThatWasGiven()` - _(no documentation)_
- `public void DuplicateAndDelete_AreDisabledWithoutSelection_AndNamed()` - _(no documentation)_
- `public void Duplicate_WithASelection_AddsACopyAndSelectsIt()` - _(no documentation)_
- `public void Delete_WithASelection_RemovesItAsOneUndoStep()` - _(no documentation)_
- `public void PanelButtons_AreProperties_Layers_History_InThatOrder()` - _(no documentation)_
- `public void PanelButton_Toggle_OpensThePanel_AndMarksOnlyItActive()` - _(no documentation)_
- `public void PanelButton_IsActive_NotifiesWhenThePanelChanges()` - _(no documentation)_
- `public void ButtonNames_FollowTheLanguage()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## TabletViewsTests (class)

`public class TabletViewsTests`

Source: `tests/PressToShape.App.Tests/TabletViewsTests.cs:20-915`

_(no documentation)_

**Members**

- `public void Toolbar_BuildsAndShowsTheTitleAndTheZoom()` - _(no documentation)_
- `public void Toolbar_FollowsTheDocument_StarUndoAndZoom()` - _(no documentation)_
- `public void Toolbar_TheButtonsHaveAccessibleNames()` - _(no documentation)_
- `public void Toolbar_WithTheTouchSizes_EveryControlIsAtLeast48()` - _(no documentation)_
- `public void Toolbar_TheOverflowButton_HasTheMenuOfTheViewModel()` - _(no documentation)_
- `public void Toolbar_ShowsTheGermanTitleAfterALanguageChange()` - _(no documentation)_
- `public void Toolbar_HasThePanelIconsInOrder_AndTheOpenOneIsChecked()` - _(no documentation)_
- `public void Toolbar_DuplicateAndDelete_AreDisabledWithoutASelection_AndActOnIt()` - _(no documentation)_
- `public void Toolbar_FitsInOneRow_AtTheTabletWidths(int width)` - _(no documentation)_
- `public void Toolbar_MenuClosed_IsRaisedWhenAMenuCloses()` - _(no documentation)_
- `public void Toolbar_TryCloseMenu_ClosesTheOpenMenu_AndSaysSo()` - _(no documentation)_
- `public void Panel_WhileClosed_IsHiddenAndShowsNoContent()` - _(no documentation)_
- `public void Panel_Open_ShowsTheTitleAndTheContentOfTheTab()` - _(no documentation)_
- `public void Panel_SwitchingTheTab_ShowsTheOtherContent()` - _(no documentation)_
- `public void Panel_ShowsTheSameInspectorAsTheShell()` - _(no documentation)_
- `public void Panel_HasTheWidthOfTheTheme()` - _(no documentation)_
- `public void Panel_WithTheTouchSizes_TheContentIsTouchSized()` - _(no documentation)_
- `public void Shell_ShowsEveryRegion_WithItsViewModel()` - _(no documentation)_
- `public void Shell_HasNoRulers_NoMenuBar_NoStatusBar_AndNoBarUnderTheCanvas()` - _(no documentation)_
- `public void Shell_IsTouchSized_WithoutAnyHelp()` - _(no documentation)_
- `public void Shell_Layout_ToolbarOnTop_ToolColumnLeft_CanvasFillsTheRest()` - _(no documentation)_
- `public void Shell_TheCanvas_IsAsWideAsTheLayoutMinusTheNarrowColumn()` - _(no documentation)_
- `public void Shell_OpeningAndClosingThePanel_DoesNotChangeTheCanvas()` - _(no documentation)_
- `public void Shell_APressOnTheCanvas_ClosesThePanel_AndStillDrawsWithTheTool()` - _(no documentation)_
- `public void Shell_APressOnAToolOrTheToolbar_KeepsThePanelOpen()` - _(no documentation)_
- `public void Shell_TheDialogOverlay_IsHidden_UntilADialogIsOpen_AndShowsItsTitleAndMessage()` - _(no documentation)_
- `public void Shell_TheDialogButtons_AreTouchSized()` - _(no documentation)_
- `public void ToolColumn_ShowsSixGroupButtonsInOrder_OnlyTheActiveOneChecked()` - _(no documentation)_
- `public void ToolColumn_MarksOnlyTheGroups()` - _(no documentation)_
- `public void ToolColumn_ChangingTheToolFromOutside_MovesTheHighlightAndTheIcon()` - _(no documentation)_
- `public void ToolColumn_ATapOnAGroupSelectsItsCurrentTool()` - _(no documentation)_
- `public void ToolColumn_TheLongPressCommand_OpensTheFlyoutWithAllToolsOfTheGroup()` - _(no documentation)_
- `public void ToolColumn_ChoosingAnEntry_SelectsTheToolAndClosesTheFlyout()` - _(no documentation)_
- `public void ToolColumn_TappingTheActiveToolAgain_KeepsItHighlighted()` - _(no documentation)_
- `public void ToolColumn_ATapOnAnotherTool_StillMovesTheHighlightBothWays()` - _(no documentation)_
- `public void ToolColumn_TappingTheActiveEntryOfAFlyoutAgain_KeepsItChecked()` - _(no documentation)_
- `public void Toolbar_ThePanelIcon_FollowsThePanelAlsoWhenThePanelClosesByItself()` - _(no documentation)_
- `public void ToolColumn_ASingleToolGroup_OpensNoFlyout()` - _(no documentation)_
- `public void ToolColumn_WithTheTouchSizes_EveryButtonIsAtLeast48_AlsoInTheFlyout()` - _(no documentation)_
- `public void ToolColumn_FitsTheHeightOfATablet()` - _(no documentation)_
- `public void ToolColumn_HasNoSwapOrResetButton_AndOneSwatchOfAtLeast48()` - _(no documentation)_
- `public void ToolColumn_IsNarrow_AndEverythingIsCentered()` - _(no documentation)_
- `public void ColorSwatch_ShowsTheFillInTheMiddleAndTheStrokeAsRing_AndFollowsChanges()` - _(no documentation)_
- `public void ColorSwatch_WithoutFillOrStroke_ShowsEmptyOutlinedParts()` - _(no documentation)_
- `public void ColorSwatch_ATap_OpensThePropertiesPanel()` - _(no documentation)_

## TestFileServices (class)

`internal static class TestFileServices`

Source: `tests/PressToShape.App.Tests/FileServiceFakes.cs:122-130`

Creates the file services of the shell from fakes.

**Members**

- `public FileServices Create(FakeFileStore? files = null, FakeFileDialogs? dialogs = null, FakeMessageDialogs? messages = null, ISettingsStore? settings = null)` - _(no documentation)_

## TestSelection (class)

`internal static class TestSelection`

Source: `tests/PressToShape.App.Tests/TestSelection.cs:10-31`

Helpers that put shapes into the editor state and select them, the way tools would.

**Members**

- `public Guid[] AddAndSelect(EditorState state, int count = 1)` - Adds rectangles to the active layer (one history step) and selects all of them. Returns their ids.

## TestShell (class)

`internal sealed class TestShell : EditorShellViewModel`

Source: `tests/PressToShape.App.Tests/TestShell.cs:10-58`

The smallest shell: only what the base class gives every layout, with fake files, dialogs and settings. For testing the parts of a layout.

**Members**

- `public TestShell(ILocalizer? localizer = null)` - _(no documentation)_
- `public ILocalizer LocalizerForTests { get; }` - The localizer the shell was made with.
- `public FileMenuCommands FileCommandsForTests { get; }` - The commands behind the file items of a menu, which the base class keeps protected.
- `public EditMenuCommands EditCommandsForTests { get; }` - The commands behind the edit items of a menu, which the base class keeps protected.
- `public ObjectMenuCommands ObjectCommandsForTests { get; }` - _(no documentation)_
- `public FakeFileStore Files { get; }` - _(no documentation)_
- `public FakeFileDialogs Dialogs { get; }` - _(no documentation)_
- `public FakeMessageDialogs Messages { get; }` - _(no documentation)_
- `public RectangleShape AddRectangle(double x = 100, double y = 100, double width = 200, double height = 100)` - Draws a rectangle on the active layer as one history step, without selecting it.
- `public RectangleShape AddAndSelectRectangle(double x = 100, double y = 100, double width = 200, double height = 100)` - Draws a rectangle and selects it.

**Relationships**

- Depends on (constructor) `PressToShape.App.Localization.ILocalizer`
- Depends on (constructor) `PressToShape.App.Tests.FakeFileDialogs`
- Depends on (constructor) `PressToShape.App.Tests.FakeFileStore`
- Depends on (constructor) `PressToShape.App.Tests.FakeMessageDialogs`
- Inherits `PressToShape.App.ViewModels.Editor.EditorShellViewModel`

## ThemeResourceTests (class)

`public class ThemeResourceTests`

Source: `tests/PressToShape.App.Tests/ThemeResourceTests.cs:9-73`

_(no documentation)_

**Members**

- `public void ThemeFile_Exists()` - _(no documentation)_
- `public void LightAndDark_DefineTheSameResourceKeys()` - _(no documentation)_
- `public void SelectionBrushes_ExistInLightAndDark()` - _(no documentation)_
- `public void NoHexColors_OutsideThemeFile()` - _(no documentation)_

## ToolDefinitionsTests (class)

`public class ToolDefinitionsTests`

Source: `tests/PressToShape.App.Tests/ToolDefinitionsTests.cs:7-81`

_(no documentation)_

**Members**

- `public void All_ListsEveryTool_InMockupOrder()` - _(no documentation)_
- `public void All_CoversEveryEnumValue()` - _(no documentation)_
- `public void ShortcutKeys_AreUnique()` - _(no documentation)_
- `public void FindByShortcut_KnownKey_ReturnsTool(char key, string expectedTool)` - _(no documentation)_
- `public void FindByShortcut_UnknownKey_ReturnsNull(char key)` - _(no documentation)_
- `public void StarAndSpiral_HaveNoShortcut(string toolName)` - _(no documentation)_
- `public void ToolTip_ShowsShortcutWhenPresent()` - _(no documentation)_

## ToolGroupButtonTests (class)

`public class ToolGroupButtonTests`

Source: `tests/PressToShape.App.Tests/ToolGroupButtonTests.cs:13-145`

_(no documentation)_

**Members**

- `public void ATap_RaisesTheClick_AndNoLongPress()` - _(no documentation)_
- `public void AHold_RunsTheLongPressCommand_AndSwallowsTheClick()` - _(no documentation)_
- `public void AHold_RunsTheLongPressCommandOnlyOnce()` - _(no documentation)_
- `public void ATapAfterAHold_SelectsAgain()` - _(no documentation)_
- `public void MovingFarBeforeTheDelay_CancelsTheLongPress()` - _(no documentation)_
- `public void ALongPressCommandThatCannotRun_IsNotRun()` - _(no documentation)_

## ToolGroupViewModelTests (class)

`public class ToolGroupViewModelTests`

Source: `tests/PressToShape.App.Tests/ToolGroupViewModelTests.cs:9-136`

_(no documentation)_

**Members**

- `public void New_StartsWithTheFirstToolOfTheGroup()` - _(no documentation)_
- `public void New_WhenTheActiveToolIsAMember_ShowsItAndIsActive()` - _(no documentation)_
- `public void SelectCurrent_MakesTheCurrentToolActive()` - _(no documentation)_
- `public void SelectMember_SetsTheActiveToolMakesItCurrentAndClosesTheFlyout()` - _(no documentation)_
- `public void SelectCurrent_AfterAChoiceInTheFlyout_UsesThatTool()` - _(no documentation)_
- `public void ActiveTool_ChangedFromOutsideToAMember_UpdatesCurrentAndTheIcon()` - _(no documentation)_
- `public void ActiveTool_ChangedToAToolOfAnotherGroup_IsNotActiveAndKeepsCurrent()` - _(no documentation)_
- `public void Name_FollowsTheLanguage()` - _(no documentation)_
- `public void IsGroup_ForASingleTool_IsFalseAndOpenFlyoutDoesNothing()` - _(no documentation)_
- `public void OpenFlyout_ForAGroup_OpensIt()` - _(no documentation)_

## ToolGroupsTests (class)

`public class ToolGroupsTests`

Source: `tests/PressToShape.App.Tests/ToolGroupsTests.cs:8-60`

_(no documentation)_

**Members**

- `public void All_HasTheSixGroupsInOrder()` - _(no documentation)_
- `public void All_ContainsEveryToolExactlyOnce()` - _(no documentation)_
- `public void Of_ForEveryTool_ReturnsTheGroupThatHoldsIt()` - _(no documentation)_
- `public void DefaultTool_IsTheFirstToolOfTheGroup()` - _(no documentation)_
- `public void IsGroup_IsFalseOnlyForTextAndEyedropper()` - _(no documentation)_

## ToolbarViewModelTests (class)

`public class ToolbarViewModelTests`

Source: `tests/PressToShape.App.Tests/ToolbarViewModelTests.cs:9-167`

_(no documentation)_

**Members**

- `public void Groups_ContainTheMockupButtons_InOrder()` - _(no documentation)_
- `public void AllButtons_AreDisabledForNow()` - _(no documentation)_
- `public void ToolToggles_FollowTheActiveTool()` - _(no documentation)_
- `public void CheckingAToggle_ChangesTheActiveTool()` - _(no documentation)_
- `public void UncheckingTheActiveToggle_KeepsItChecked()` - _(no documentation)_
- `public void ToolToggleChange_RaisesNotifications()` - _(no documentation)_
- `public void Toolbar_HasNoPositionOrSizeFields_TheyLiveInTheProperties()` - _(no documentation)_
- `public void ToolbarButtonNames_AreTranslated_AndRefreshOnLanguageChange()` - _(no documentation)_
- `public void Button_FollowsTheCommandsCanExecute()` - _(no documentation)_
- `public void Button_WithoutACommand_IsDisabled()` - _(no documentation)_
- `public void UndoAndRedoButtons_UseTheGivenCommands()` - _(no documentation)_

## ToolboxViewModelTests (class)

`public class ToolboxViewModelTests`

Source: `tests/PressToShape.App.Tests/ToolboxViewModelTests.cs:9-126`

_(no documentation)_

**Members**

- `public void Tools_AreListedInMockupOrder()` - _(no documentation)_
- `public void Select_IsActiveByDefault_AndOnlyOneToolIsActive()` - _(no documentation)_
- `public void SelectToolCommand_ChangesActiveTool_AndHighlight()` - _(no documentation)_
- `public void ToolChangedElsewhere_IsReflected()` - _(no documentation)_
- `public void TrySelectByKey_KnownKey_SelectsTool(char key, string expectedTool)` - _(no documentation)_
- `public void TrySelectByKey_UnknownKey_ReturnsFalse_AndKeepsTool(char key)` - _(no documentation)_
- `public void ToolbarItem_ToolTip_ContainsNameAndShortcut()` - _(no documentation)_
- `public void SwatchCommands_ExistButCannotExecute()` - _(no documentation)_
- `public void ToolNamesAndTooltips_AreTranslated_AndRefreshOnLanguageChange()` - _(no documentation)_

## TouchGestureTrackerTests (class)

`public class TouchGestureTrackerTests`

Source: `tests/PressToShape.App.Tests/TouchGestureTrackerTests.cs:6-352`

_(no documentation)_

**Members**

- `public void FirstTouch_IsPassedOnAsAPress()` - _(no documentation)_
- `public void OneFinger_MoveAndRelease_ArePassedOn()` - _(no documentation)_
- `public void AfterARelease_TheNextTouchIsAFirstTouchAgain_EvenWithAnotherId()` - _(no documentation)_
- `public void SecondTouch_CancelsTheRunningAction_AndStartsAGesture()` - _(no documentation)_
- `public void WhileTwoFingersAreDown_NothingIsPassedToTheTool()` - _(no documentation)_
- `public void Pinch_MovingApart_DoublesTheZoom()` - _(no documentation)_
- `public void Pinch_MovingTogether_HalvesTheZoom()` - _(no documentation)_
- `public void Pinch_StartsFromTheZoomThatWasGivenWhenTheSecondFingerArrived()` - _(no documentation)_
- `public void Pinch_TheTargetZoomIsRelativeToTheStart_NotToThePreviousMove()` - _(no documentation)_
- `public void Pinch_IsClampedToTheZoomRange()` - _(no documentation)_
- `public void BothFingersMovingTogether_PansWithoutZooming()` - _(no documentation)_
- `public void Gesture_ReportsTheMidpointBeforeAndAfter()` - _(no documentation)_
- `public void FingersAtTheSameSpot_DoNotDivideByZero()` - _(no documentation)_
- `public void OneFingerLifting_EndsTheGesture_AndTheRestIsIgnored()` - _(no documentation)_
- `public void AfterAllFingersAreUp_TheNextTouchActsAgain()` - _(no documentation)_
- `public void ANewFingerWhileTheHandIsLifting_IsIgnored_UntilEveryFingerIsUp()` - _(no documentation)_
- `public void AThirdFinger_IsIgnored_AndDoesNotDisturbTheGesture()` - _(no documentation)_
- `public void MouseAndPen_AreNotTracked_AndDoNotChangeTheState(int deviceType)` - _(no documentation)_
- `public void Cancel_DuringOneFinger_AsksToCancelTheAction_AndForgetsTheFinger()` - _(no documentation)_
- `public void Cancel_DuringAGesture_ResetsWithoutAnotherCancel()` - _(no documentation)_
- `public void Cancel_WhenIdle_DoesNothing()` - _(no documentation)_
- `public void UnknownIds_AreIgnored()` - _(no documentation)_
- `public void ADuplicatePress_IsIgnored()` - _(no documentation)_
- `public void NonFinitePositions_AreIgnored(double x, double y)` - _(no documentation)_
- `public void NonFiniteZoomWhenTheSecondFingerArrives_IsIgnored()` - _(no documentation)_
- `public void TheSecondFingersIdMayDifferFromEveryEarlierId()` - _(no documentation)_

## TouchSizesTests (class)

`public class TouchSizesTests`

Source: `tests/PressToShape.App.Tests/TouchSizesTests.cs:23-390`

_(no documentation)_

**Members**

- `public void EveryTouchKey_HasADesktopValueOfTheSameType()` - _(no documentation)_
- `public void TouchTargets_AreAtLeast48(string key)` - _(no documentation)_
- `public void ApplyTo_CanBeUsedForSeveralRoots()` - _(no documentation)_
- `public void Desktop_ToolColumn_KeepsOneColumnOf36()` - _(no documentation)_
- `public void Desktop_TappingTheActiveToolAgain_KeepsItHighlighted()` - _(no documentation)_
- `public void WithTheTouchSizes_ToolButtonsAre48_InOneColumn()` - _(no documentation)_
- `public void Desktop_PropertiesPanel_KeepsItsSizes()` - _(no documentation)_
- `public void WithTheTouchSizes_EveryControlOfThePropertiesPanel_IsAtLeast48High()` - _(no documentation)_
- `public void WithTheTouchSizes_ButtonsOfThePropertiesPanel_AreAtLeast48Wide()` - _(no documentation)_
- `public void Desktop_LayerRows_KeepTheirSize()` - _(no documentation)_
- `public void Desktop_HistoryRows_KeepTheirSize()` - _(no documentation)_
- `public void WithTheTouchSizes_LayerRowsAndTheirButtons_AreAtLeast48()` - _(no documentation)_
- `public void WithTheTouchSizes_HistoryRows_AreAtLeast48High()` - _(no documentation)_
- `public void Canvas_ShowsItsRulers_ByDefault_AndHidesThemWhenAsked()` - _(no documentation)_
- `public void Desktop_SelectionHandles_AreDrawnWith8()` - _(no documentation)_
- `public void WithTheTouchSizes_SelectionHandles_AreDrawnLarger()` - _(no documentation)_
- `public void TheTouchGrabSize_ComesFromTheTheme()` - _(no documentation)_
- `public void TheGrabAreaForAFinger_IsLargerThanTheDrawnHandle()` - _(no documentation)_

## TranslationConsistencyTests (class)

`public class TranslationConsistencyTests`

Source: `tests/PressToShape.App.Tests/TranslationConsistencyTests.cs:14-159`

Guards the German catalog against the texts in the code. These tests read de.po directly and do not need msgfmt.

**Members**

- `public void SourcesContainTranslatableTexts()` - _(no documentation)_
- `public void EveryTextInTheCode_HasAGermanTranslation()` - _(no documentation)_
- `public void EveryGermanEntry_IsUsedInTheCode()` - _(no documentation)_
- `public void GermanCatalog_HasNoDuplicateEntries()` - _(no documentation)_
- `public void GermanCatalog_HasNoFuzzyOrEmptyEntries()` - _(no documentation)_
- `public void KeyHints_AndPlaceholders_AreKeptInTheTranslation()` - _(no documentation)_
- `public void Ellipses_AreKeptInTheTranslation()` - _(no documentation)_
- `public void AltMnemonics_ArePresentInBothLanguages_WhenTheEnglishTextHasOne()` - _(no documentation)_
- `public void MenuMnemonics_AreUniquePerMenuLevel(string language)` - _(no documentation)_

## ViewGeometryTests (class)

`public class ViewGeometryTests`

Source: `tests/PressToShape.App.Tests/ViewGeometryTests.cs:7-215`

_(no documentation)_

**Members**

- `public void Origin_CentersTheArtboardThenAppliesTheOffset(double viewport, double artboard, double pan, double expected)` - _(no documentation)_
- `public void PanLimit_KeepsTheMinimumVisible(double viewport, double artboard, double expected)` - _(no documentation)_
- `public void PanLimit_AtTheLimit_LeavesExactlyTheMinimumVisible()` - _(no documentation)_
- `public void PanLimit_SmallArtboard_KeepsItsWholeSizeVisible()` - _(no documentation)_
- `public void PanLimit_UnknownViewport_HasNoLimit(double viewport)` - _(no documentation)_
- `public void PanLimit_ViewportSmallerThanTheMargin_IsNeverNegative()` - _(no documentation)_
- `public void ClampPan_LimitsTheOffset(double pan, double expected)` - _(no documentation)_
- `public void ClampPan_NonFiniteOffset_BecomesZero(double pan)` - _(no documentation)_
- `public TheoryData<double, double> ZoomPairs()` - _(no documentation)_
- `public void PanAfterZoom_KeepsTheDocumentPointUnderTheAnchor(double oldZoom, double newZoom)` - _(no documentation)_
- `public void PanAfterZoom_ZoomingAtTheCenterOfACenteredArtboard_StaysCentered()` - _(no documentation)_
- `public void PanAfterZoom_InvalidInput_IsFinite(double oldZoom, double newZoom, double anchor)` - _(no documentation)_
- `public void WheelZoom_UsesTheFactorPerNotch(double zoom, double delta, double expected)` - _(no documentation)_
- `public void WheelZoom_FractionalDelta_ScalesTheFactorInProportion()` - _(no documentation)_
- `public void WheelZoom_StopsAtTheZoomLimits(double zoom, double delta, double expected)` - _(no documentation)_
- `public void WheelZoom_NonFiniteDelta_LeavesTheZoomUnchanged(double delta)` - _(no documentation)_
- `public void RandomZoomPanAndResizeSteps_KeepTheArtboardVisibleAndEverythingFinite()` - _(no documentation)_

## ZoomLevelsTests (class)

`public class ZoomLevelsTests`

Source: `tests/PressToShape.App.Tests/ZoomLevelsTests.cs:7-89`

_(no documentation)_

**Members**

- `public void Clamp_KeepsValueInsideAllowedRange(double input, double expected)` - _(no documentation)_
- `public void NextIn_ReturnsNextPresetAbove(double current, double expected)` - _(no documentation)_
- `public void NextOut_ReturnsNextPresetBelow(double current, double expected)` - _(no documentation)_
- `public void Presets_AreAscending_AndWithinLimits()` - _(no documentation)_
- `public void FitToViewport_FitsArtboardWithMargin()` - _(no documentation)_
- `public void FitToViewport_ClampsToMaximum()` - _(no documentation)_
- `public void FitToViewport_NonPositiveSize_ReturnsNull(double vw, double vh, double aw, double ah)` - _(no documentation)_
- `public void FitToViewport_ViewportSmallerThanMargin_ClampsToMinimum()` - _(no documentation)_

## ZoomViewModelTests (class)

`public class ZoomViewModelTests`

Source: `tests/PressToShape.App.Tests/ZoomViewModelTests.cs:10-119`

_(no documentation)_

**Members**

- `public void Defaults_Show100Percent()` - _(no documentation)_
- `public void ZoomInAndOutCommands_StepThroughThePresets()` - _(no documentation)_
- `public void SetZoomCommand_AppliesAValue()` - _(no documentation)_
- `public void FitCommand_InvokesTheCallbackOnce()` - _(no documentation)_
- `public void Presets_AreTheZoomLevels_AndTheirCommandsSetTheZoom()` - _(no documentation)_
- `public void ZoomChangedElsewhere_RaisesNotificationsForTheValueAndTheText()` - _(no documentation)_
- `public void ZoomText_FollowsTheDecimalSeparatorOfTheLanguage()` - _(no documentation)_
- `public void WholeNumberPresets_LookTheSameInEveryLanguage()` - _(no documentation)_
- `public void FormatZoom_UsesTheGivenCulture(double percent, string expected)` - _(no documentation)_
