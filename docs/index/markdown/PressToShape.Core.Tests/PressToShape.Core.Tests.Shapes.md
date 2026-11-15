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
