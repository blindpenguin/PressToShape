# PressToShape.Core.Tests.Documents

Project: PressToShape.Core.Tests

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
