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

## CanvasAreaViewModelTests (class)

`public class CanvasAreaViewModelTests`

Source: `tests/PressToShape.App.Tests/CanvasAreaViewModelTests.cs:12-1075`

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

## CanvasInteractionTests (class)

`public class CanvasInteractionTests`

Source: `tests/PressToShape.App.Tests/CanvasInteractionTests.cs:11-1395`

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

Source: `tests/PressToShape.App.Tests/DocumentFileServiceTests.cs:18-776`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public Task Save_WithoutAFile_AsksForOneAndWritesTheProject()` - _(no documentation)_
- `public Task Save_WithAFile_WritesWithoutADialog()` - _(no documentation)_
- `public Task SaveAs_Cancelled_WritesNothingAndChangesNothing()` - _(no documentation)_
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

## FakeFileStore (class)

`internal sealed class FakeFileStore : IFileStore`

Source: `tests/PressToShape.App.Tests/FileServiceFakes.cs:11-48`

An in-memory file system. Can be told to fail on write or read.

**Members**

- `public Dictionary<string, byte[]> Files { get; }` - _(no documentation)_
- `public Exception? WriteFailure { get; set; }` - _(no documentation)_
- `public Exception? ReadFailure { get; set; }` - _(no documentation)_
- `public int WriteCount { get; private set; }` - _(no documentation)_
- `public byte[] ReadAllBytes(string path, long maxBytes)` - _(no documentation)_
- `public void WriteAllBytesAtomic(string path, byte[] bytes)` - _(no documentation)_
- `public bool Exists(string path)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.App.Services.IFileStore`

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

## HistoryViewModelTests (class)

`public class HistoryViewModelTests`

Source: `tests/PressToShape.App.Tests/HistoryViewModelTests.cs:13-236`

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

Source: `tests/PressToShape.App.Tests/LayersViewModelTests.cs:12-319`

_(no documentation)_

**Members**

- `public void Constructor_NullState_Throws()` - _(no documentation)_
- `public void Items_Initially_ListTheLayersTopmostFirst()` - _(no documentation)_
- `public void Items_NewLayer_StartsVisibleAndUnlocked()` - _(no documentation)_
- `public void ToggleVisibility_Executed_HidesTheLayerInTheDocument()` - _(no documentation)_
- `public void ToggleVisibility_Twice_ReturnsToTheOriginalValue()` - _(no documentation)_
- `public void ToggleLock_Executed_LocksTheLayerInTheDocument()` - _(no documentation)_
- `public void Toggle_Executed_LeavesTheEarlierDocumentUnchanged()` - _(no documentation)_
- `public void Sync_OnlyLayerValuesChanged_KeepsTheSameItemInstances()` - _(no documentation)_
- `public void Sync_NameChanged_RaisesPropertyChangedOnTheItem()` - _(no documentation)_
- `public void Sync_LayerAdded_RebuildsTheItemsWithTheNewLayerOnTop()` - _(no documentation)_
- `public void Sync_LayerRemoved_RebuildsTheItems()` - _(no documentation)_
- `public void Toggle_AfterRebuild_WorksOnTheNewItems()` - _(no documentation)_
- `public void Items_ArtboardSizeChangeOnly_KeepsTheItems()` - _(no documentation)_
- `public void AddLayerCommand_CannotExecuteYet()` - _(no documentation)_
- `public void LayerItem_NullArguments_Throw()` - _(no documentation)_
- `public void SelectedItem_Initially_IsTheTopmostLayer()` - _(no documentation)_
- `public void SelectedItem_Set_ChangesTheActiveLayer()` - _(no documentation)_
- `public void SelectedItem_ActiveLayerChangedInTheState_FollowsAndNotifies()` - _(no documentation)_
- `public void SelectedItem_SetToNull_KeepsTheActiveLayer()` - _(no documentation)_
- `public void SelectedItem_ShapeEdit_KeepsTheSameRowWithoutRebuilding()` - _(no documentation)_
- `public void SelectedItem_LayerRemoved_SelectsTheNewActiveLayerInTheRebuiltList()` - _(no documentation)_
- `public void SelectedItem_OtherLayerRemoved_KeepsTheActiveLayerSelected()` - _(no documentation)_
- `public void ToggleVisibility_OnAnotherRow_DoesNotChangeTheActiveLayer()` - _(no documentation)_
- `public void Toggles_RecordTheFourStepTypes()` - _(no documentation)_
- `public void Toggle_ThenUndo_RestoresTheLayerAndTheRow()` - _(no documentation)_

## LocalFileStoreTests (class)

`public sealed class LocalFileStoreTests : IDisposable`

Source: `tests/PressToShape.App.Tests/LocalFileStoreTests.cs:9-203`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
- `public void Write_NewFileInNewFolder_CreatesBoth()` - _(no documentation)_
- `public void Write_ExistingFile_IsReplaced()` - _(no documentation)_
- `public void Write_Success_LeavesNoTemporaryFile()` - _(no documentation)_
- `public void Write_TargetIsAnExistingDirectory_ThrowsAndLeavesNoTemporaryFile()` - _(no documentation)_
- `public void Write_FolderIsAFile_Throws()` - _(no documentation)_
- `public void Write_FolderIsReadOnly_ThrowsAndKeepsTheOldFile()` - _(no documentation)_
- `public void Read_ExistingFile_ReturnsTheBytes()` - _(no documentation)_
- `public void Read_EmptyFile_ReturnsNoBytes()` - _(no documentation)_
- `public void Read_FileOverTheLimit_ThrowsInvalidDataException()` - _(no documentation)_
- `public void Read_FileExactlyAtTheLimit_IsRead()` - _(no documentation)_
- `public void Read_MissingFile_ThrowsFileNotFoundException()` - _(no documentation)_
- `public void Read_MissingFolder_ThrowsDirectoryNotFoundException()` - _(no documentation)_
- `public void Exists_ReportsFilesOnly()` - _(no documentation)_
- `public void Arguments_EmptyPathOrNullBytes_Throw()` - _(no documentation)_

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

## MainViewModelTests (class)

`public class MainViewModelTests`

Source: `tests/PressToShape.App.Tests/MainViewModelTests.cs:6-15`

_(no documentation)_

**Members**

- `public void Greeting_Default_IsWelcomeMessage()` - _(no documentation)_

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

Source: `tests/PressToShape.App.Tests/MenuBarViewModelTests.cs:12-565`

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

## PropertiesViewModelTests (class)

`public class PropertiesViewModelTests`

Source: `tests/PressToShape.App.Tests/PropertiesViewModelTests.cs:14-461`

_(no documentation)_

**Members**

- `public void NothingSelected_DisablesEverything_AndShowsTheHeader()` - _(no documentation)_
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
- `public void StrokeColorEdit_IsAChangeStrokeStep_ForAllSelectedShapes()` - _(no documentation)_
- `public void ClosingThePickerWithoutAChange_MakesNoStep()` - _(no documentation)_
- `public void NoneAndColorOutsideThePicker_AreImmediateSteps()` - _(no documentation)_
- `public void ColorField_WithMixedFills_BecomesAColorOnTheFirstUpdate()` - _(no documentation)_
- `public void ResetTransform_IsOneStep_ClearsTheRotation_AndDisablesItself()` - _(no documentation)_
- `public void SelectionChanges_CreateNoSteps_AndSectionFlagsKeepWorking()` - _(no documentation)_
- `public void Constructor_NullArguments_Throw()` - _(no documentation)_

## RecentFilesTests (class)

`public sealed class RecentFilesTests : IDisposable`

Source: `tests/PressToShape.App.Tests/RecentFilesTests.cs:12-239`

_(no documentation)_

**Members**

- `public void Dispose()` - _(no documentation)_
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

## SelectionGeometryTests (class)

`public class SelectionGeometryTests`

Source: `tests/PressToShape.App.Tests/SelectionGeometryTests.cs:8-182`

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
- `public void HitHandle_AwayFromAllHandles_IsNull()` - _(no documentation)_
- `public void HitHandle_RotateHandleIsOutsideTheFrame()` - _(no documentation)_
- `public void HitHandle_RotatedFrame_FollowsTheTurnedHandles()` - _(no documentation)_
- `public void HitHandle_EveryHandleIsReachableByItsOwnCenter_EvenOnATinyFrame()` - _(no documentation)_
- `public void HitHandle_EqualDistance_PrefersCornersOverEdges()` - _(no documentation)_
- `public void HandleDirectionDegrees_IsTheTurnedDirection()` - _(no documentation)_
- `public void NullArguments_Throw()` - _(no documentation)_

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

## TranslationConsistencyTests (class)

`public class TranslationConsistencyTests`

Source: `tests/PressToShape.App.Tests/TranslationConsistencyTests.cs:13-152`

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
