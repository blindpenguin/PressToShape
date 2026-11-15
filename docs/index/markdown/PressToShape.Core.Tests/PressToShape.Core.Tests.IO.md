# PressToShape.Core.Tests.IO

Project: PressToShape.Core.Tests

## ProjectFilePaletteTests (class)

`public class ProjectFilePaletteTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFilePaletteTests.cs:11-83`

_(no documentation)_

**Members**

- `public void WriteThenRead_Palette_KeepsOrderAndAlpha()` - _(no documentation)_
- `public void Write_EmptyPalette_WritesNoPaletteField()` - _(no documentation)_
- `public void Read_FileWithoutPalette_LoadsWithEmptyPalette()` - _(no documentation)_
- `public void Read_InvalidPaletteEntry_FailsWithInvalidValue(string palette)` - _(no documentation)_
- `public void WriteThenRead_StrokeWithAlpha_IsKept()` - _(no documentation)_

## ProjectFileTests (class)

`public class ProjectFileTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFileTests.cs:13-308`

_(no documentation)_

**Members**

- `public void WriteThenRead_Sample_ReturnsTheSameContent()` - _(no documentation)_
- `public void WriteThenRead_FreshDocument_ReturnsTheSameContent()` - _(no documentation)_
- `public void WriteThenRead_ShapeOrderAndLayerOrder_AreKept()` - _(no documentation)_
- `public void WriteThenRead_NullFillAndStroke_StayNull()` - _(no documentation)_
- `public void WriteThenRead_Alpha_IsKept()` - _(no documentation)_
- `public void WriteThenRead_ExtremeNumbers_AreExact(double value)` - _(no documentation)_
- `public void Write_ContainsFormatNameAndVersion()` - _(no documentation)_
- `public void Write_Output_IsUtf8WithoutByteOrderMark()` - _(no documentation)_
- `public void Write_ColorsAreRrggbbaaText_AndNoColorIsNull()` - _(no documentation)_
- `public void WriteAndRead_UnderAGermanCulture_UseInvariantNumbers()` - _(no documentation)_
- `public void Write_LoneSurrogateInName_IsReplacedAndStillReadable()` - _(no documentation)_
- `public void Write_NullDocument_Throws()` - _(no documentation)_
- `public void Read_UnknownExtraEntries_AreIgnored()` - _(no documentation)_
- `public void Read_Success_HasNoErrorAndNoDetail()` - _(no documentation)_
- `public void WriteThenRead_RotatedShapes_KeepTheirRotationExactly()` - _(no documentation)_
- `public void Write_RotationEntry_IsOnlyWrittenForRotatedShapes_AndTheVersionStaysOne()` - _(no documentation)_
- `public void Read_FileWithoutRotationEntries_ReadsAsZero()` - _(no documentation)_
- `public void Read_RotationOutsideTheRange_IsNormalized_AndNullMeansNone(string value, double expected)` - _(no documentation)_
- `public void Read_RotationOfTheWrongType_IsRejectedLikeOtherBadValues(string value)` - _(no documentation)_
- `public void Read_RotationThatOverflowsADouble_IsRejected()` - _(no documentation)_

## ProjectFileValidationTests (class)

`public class ProjectFileValidationTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFileValidationTests.cs:8-331`

_(no documentation)_

**Members**

- `public void Read_TheValidBaseText_Succeeds()` - _(no documentation)_
- `public void Read_NotAProjectFile_ReturnsNotProjectFile(string json)` - _(no documentation)_
- `public void Read_EmptyInput_ReturnsNotProjectFile()` - _(no documentation)_
- `public void Read_BadFormatNameOrVersion_ReturnsNotProjectFile(string from, string to)` - _(no documentation)_
- `public void Read_NewerVersion_ReturnsNewerVersion(string from, string to)` - _(no documentation)_
- `public void Read_BadArtboard_ReturnsNotProjectFile(string from, string to)` - _(no documentation)_
- `public void Read_ArtboardSizeNotPositive_ReturnsInvalidValue(string from, string to)` - _(no documentation)_
- `public void Read_BadLayersOrLayerEntries_ReturnsNotProjectFile(string from, string to)` - _(no documentation)_
- `public void Read_BadIds_ReturnsInvalidValue(string from, string to)` - _(no documentation)_
- `public void Read_DuplicateLayerIds_ReturnsInvalidValue()` - _(no documentation)_
- `public void Read_DuplicateShapeIdsAcrossLayers_ReturnsInvalidValue()` - _(no documentation)_
- `public void Read_ShapeType_OnlyKnownTypesAreAccepted(string type)` - _(no documentation)_
- `public void Read_BadShapeEntries_ReturnsNotProjectFile(string from, string to)` - _(no documentation)_
- `public void Read_ValuesTheModelRejects_ReturnsInvalidValue(string from, string to)` - _(no documentation)_
- `public void Read_BadColorText_ReturnsInvalidValue(string color)` - _(no documentation)_
- `public void Read_UpperCaseColorText_IsAccepted()` - _(no documentation)_
- `public void Read_NumbersThatAreNotFinite_Fail(string from, string to)` - _(no documentation)_
- `public void Read_Comments_AreNotAccepted()` - _(no documentation)_
- `public void Read_TrailingCommas_AreNotAccepted()` - _(no documentation)_
- `public void Read_NestingDeeperThanTheLimit_ReturnsNotProjectFile()` - _(no documentation)_
- `public void Read_InputLargerThanTheLimit_ReturnsTooLargeBeforeParsing()` - _(no documentation)_
- `public void Read_InvalidUtf8InAString_ReturnsNotProjectFile()` - _(no documentation)_
- `public void Read_ByteOrderMark_IsNotAccepted()` - _(no documentation)_
- `public void Read_EveryTruncationOfAValidFile_NeverThrows()` - _(no documentation)_
- `public void Read_SingleByteCorruptions_NeverThrow()` - _(no documentation)_

## SvgExportTests (class)

`public class SvgExportTests`

Source: `tests/PressToShape.Core.Tests/IO/SvgExportTests.cs:13-359`

_(no documentation)_

**Members**

- `public void ToUtf8_Root_HasArtboardSizeAndViewBoxAndNoBackground()` - _(no documentation)_
- `public void ToUtf8_Output_IsUtf8WithoutByteOrderMarkAndHasADeclaration()` - _(no documentation)_
- `public void ToUtf8_VisibleLayers_BecomeGroupsInDocumentOrder()` - _(no documentation)_
- `public void ToUtf8_HiddenLayer_IsLeftOut_AndLockedLayerIsExported()` - _(no documentation)_
- `public void ToUtf8_Rectangle_HasPositionAndSize()` - _(no documentation)_
- `public void ToUtf8_Ellipse_HasCenterAndRadii()` - _(no documentation)_
- `public void ToUtf8_ShapesInStackingOrder_KeepTheOrder()` - _(no documentation)_
- `public void ToUtf8_OpaqueFillAndStroke_HaveNoOpacityAttributes()` - _(no documentation)_
- `public void ToUtf8_TransparentFillAndStroke_HaveOpacityAttributes()` - _(no documentation)_
- `public void ToUtf8_NoFill_WritesFillNone()` - _(no documentation)_
- `public void ToUtf8_NoStrokeOrZeroStrokeWidth_WriteNoStrokeAttributes()` - _(no documentation)_
- `public void ToUtf8_ShapeOpacityBelowOne_WritesOpacity(double opacity, string expected)` - _(no documentation)_
- `public void ToUtf8_ZeroSizeShape_IsExported()` - _(no documentation)_
- `public void ToUtf8_UnderAGermanCulture_UsesADotAndNoExponent()` - _(no documentation)_
- `public void ToUtf8_LayerNameWithSpecialCharacters_RoundTripsThroughTheXmlParser(string name)` - _(no documentation)_
- `public void ToUtf8_ControlCharactersAndLoneSurrogatesInAName_AreReplaced()` - _(no documentation)_
- `public void ToUtf8_EmptyLayer_GivesAnEmptyGroup()` - _(no documentation)_
- `public void ToUtf8_DocumentWithOnlyHiddenLayers_IsAnEmptyValidSvg()` - _(no documentation)_
- `public void ToUtf8_Output_HasNoScriptsLinksOrExternalReferences()` - _(no documentation)_
- `public void ToUtf8_NullDocument_Throws()` - _(no documentation)_
- `public void ToUtf8_ManyShapes_ExportsAllOfThem()` - _(no documentation)_
- `public void ToUtf8_RotatedRectangle_HasATransformAroundItsCenter()` - _(no documentation)_
- `public void ToUtf8_RotatedEllipse_HasATransformAroundItsCenter()` - _(no documentation)_
- `public void ToUtf8_UnrotatedShapes_HaveNoTransform()` - _(no documentation)_
- `public void ToUtf8_RotationNumbers_UseADotUnderAGermanCulture()` - _(no documentation)_
- `public void ToUtf8_RotatedShapeWithOpacityAndStroke_KeepsAllAttributes()` - _(no documentation)_
