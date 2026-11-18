# PressToShape.Core.Tests.IO

Project: PressToShape.Core.Tests

## CompoundPathFileTests (class)

`public class CompoundPathFileTests`

Source: `tests/PressToShape.Core.Tests/IO/CompoundPathFileTests.cs:11-162`

_(no documentation)_

**Members**

- `public void WriteThenRead_CompoundPath_KeepsEverySubPath()` - _(no documentation)_
- `public void Write_CompoundPath_UsesSubPathsAndNoNodesOfItsOwn()` - _(no documentation)_
- `public void Write_PathWithOneSubPath_WritesNodesAsBefore()` - _(no documentation)_
- `public void Read_FileWithoutSubPaths_StillLoadsAsOneSubPath()` - _(no documentation)_
- `public void Read_SubPaths_Succeeds()` - _(no documentation)_
- `public void Read_InvalidSubPaths_FailsWithInvalidValue(string body)` - _(no documentation)_
- `public void Read_SubPathWithMissingParts_FailsWithNotProjectFile(string body)` - _(no documentation)_
- `public void Read_MoreNodesInAllSubPathsThanTheLimit_FailsWithInvalidValue()` - _(no documentation)_
- `public void ToUtf8_CompoundPath_WritesOneMRunPerSubPathInOnePathElement()` - _(no documentation)_

## LayerRoundTripTests (class)

`public class LayerRoundTripTests`

Source: `tests/PressToShape.Core.Tests/IO/LayerRoundTripTests.cs:12-106`

_(no documentation)_

**Members**

- `public void WriteThenRead_LayersInChangedOrder_KeepCountOrderNamesAndFlags()` - _(no documentation)_
- `public void WriteThenRead_LayersWithShapesAndGroups_KeepContents()` - _(no documentation)_
- `public void WriteThenRead_AfterLayerCommands_KeepsTheResult()` - _(no documentation)_
- `public void ToUtf8_LayersInChangedOrder_ExportsVisibleLayersInStackingOrder()` - _(no documentation)_
- `public void ToUtf8_AfterMoveLayerAbove_FollowsTheNewOrder()` - _(no documentation)_

## ProjectFileGroupTests (class)

`public class ProjectFileGroupTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFileGroupTests.cs:13-239`

_(no documentation)_

**Members**

- `public void WriteThenRead_NestedGroupsNamesAndFlags_AreKept()` - _(no documentation)_
- `public void Write_Group_HasNoBoxAndOptionalEntriesOnlyWhenSet()` - _(no documentation)_
- `public void Write_HiddenLockedNamedShape_WritesTheEntries()` - _(no documentation)_
- `public void Read_FileFromBeforeGroups_LoadsWithDefaults()` - _(no documentation)_
- `public void Read_GroupInFile_BoxIsDerivedFromChildren()` - _(no documentation)_
- `public void Read_64NestedGroups_Loads()` - _(no documentation)_
- `public void Read_65NestedGroups_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_EmptyGroup_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_GroupWithoutChildren_FailsWithNotProjectFile()` - _(no documentation)_
- `public void Read_GroupWithNullChild_FailsWithNotProjectFile()` - _(no documentation)_
- `public void Read_DuplicateIdInsideGroup_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_GroupIdSameAsChild_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_UnknownTypeInsideGroup_FailsWithUnknownShapeType()` - _(no documentation)_
- `public void Read_FlagsAndNameInFile_AreRead()` - _(no documentation)_
- `public void Read_NewerVersionThanKnown_Fails()` - _(no documentation)_
- `public void Read_NullBytesOrArbitraryText_NeverThrows()` - _(no documentation)_

## ProjectFileGuideTests (class)

`public sealed class ProjectFileGuideTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFileGuideTests.cs:9-147`

_(no documentation)_

**Members**

- `public void Write_DocumentWithoutGuides_HasNoGuidesEntry()` - _(no documentation)_
- `public void WriteThenRead_Guides_KeepOrientationPositionAndOrder()` - _(no documentation)_
- `public void Read_ALoadedGuide_GetsANewId()` - _(no documentation)_
- `public void Write_Guides_UseTheNamesHorizontalAndVertical()` - _(no documentation)_
- `public void Read_FileWithoutGuides_HasNone()` - _(no documentation)_
- `public void Read_VersionOneFile_HasNoGuides()` - _(no documentation)_
- `public void Read_DamagedGuides_AreSkippedAndTheRestLoads()` - _(no documentation)_
- `public void Read_GuidesEntryNull_MeansNoGuides()` - _(no documentation)_
- `public void FormatVersion_StaysTwo_AndANewerFileStillGivesTheError()` - _(no documentation)_
- `public void SvgExport_IgnoresGuides()` - _(no documentation)_
- `public void WriteThenRead_ShapesAndPaletteAreUnaffectedByGuides()` - _(no documentation)_

## ProjectFileNodeTypeTests (class)

`public class ProjectFileNodeTypeTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFileNodeTypeTests.cs:10-161`

_(no documentation)_

**Members**

- `public void WriteThenRead_EachType_IsKept()` - _(no documentation)_
- `public void WriteThenRead_CornerWithHandlesOnOneLine_StaysACorner()` - _(no documentation)_
- `public void WriteThenRead_SmoothNodeThatWouldDeriveAsSymmetric_StaysSmooth()` - _(no documentation)_
- `public void Write_TypeThatEqualsTheDerivedType_IsNotWritten()` - _(no documentation)_
- `public void Read_FileWithoutTypes_GetsTheTypesThatTheHandlesGive()` - _(no documentation)_
- `public void Read_NodeWithAType_UsesIt(string text, NodeType expected)` - _(no documentation)_
- `public void Read_UnknownType_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_TypeThatIsNotText_Fails(string entry)` - _(no documentation)_
- `public void Read_UpperCaseType_IsUnknown()` - _(no documentation)_
- `public void WriteThenRead_PathInsideAGroup_KeepsItsTypes()` - _(no documentation)_

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

## ProjectFilePathTests (class)

`public class ProjectFilePathTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFilePathTests.cs:10-212`

_(no documentation)_

**Members**

- `public void WriteThenRead_PathWithCurvesClosedEvenOddAndRotation_IsKept()` - _(no documentation)_
- `public void WriteThenRead_OpenPathWithDefaults_WritesNoClosedOrFillRuleEntry()` - _(no documentation)_
- `public void WriteThenRead_PathInsideAGroup_IsKept()` - _(no documentation)_
- `public void Read_MinimalPath_Succeeds()` - _(no documentation)_
- `public void Read_PathWithOneNode_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_PathWithEmptyNodes_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_PathWithoutNodes_FailsWithNotProjectFile()` - _(no documentation)_
- `public void Read_NodeWithoutAnY_FailsWithNotProjectFile()` - _(no documentation)_
- `public void Read_NullNode_FailsWithNotProjectFile()` - _(no documentation)_
- `public void Read_HalfAHandle_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_NumberThatIsNotFinite_Fails()` - _(no documentation)_
- `public void Read_NumberWrittenAsText_Fails()` - _(no documentation)_
- `public void Read_UnknownFillRule_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_TooManyNodes_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_PathWithNegativeWidth_FailsWithInvalidValue()` - _(no documentation)_
- `public void Read_FileWithoutPaths_StillLoads()` - _(no documentation)_

## ProjectFileRadiiTests (class)

`public class ProjectFileRadiiTests`

Source: `tests/PressToShape.Core.Tests/IO/ProjectFileRadiiTests.cs:10-125`

_(no documentation)_

**Members**

- `public void WriteThenRead_EqualRadii_AreKept()` - _(no documentation)_
- `public void WriteThenRead_DifferentRadii_AreKeptPerCorner()` - _(no documentation)_
- `public void Write_ZeroRadii_WritesNoRadiiField()` - _(no documentation)_
- `public void Write_Radii_AreWrittenInTheOrderTopLeftTopRightBottomRightBottomLeft()` - _(no documentation)_
- `public void Write_RadiusAboveTheLimit_IsWrittenAsStored()` - _(no documentation)_
- `public void Read_RectangleWithoutRadii_HasSquareCorners()` - _(no documentation)_
- `public void Read_RadiusAboveTheLimit_LoadsAndIsLimitedWhenUsed()` - _(no documentation)_
- `public void Read_InvalidRadii_FailsWithInvalidValue(string extra)` - _(no documentation)_
- `public void Read_RadiiOfTheWrongJsonType_Fails(string extra)` - _(no documentation)_

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

## SvgExportGroupTests (class)

`public class SvgExportGroupTests`

Source: `tests/PressToShape.Core.Tests/IO/SvgExportGroupTests.cs:11-116`

_(no documentation)_

**Members**

- `public void ToUtf8_Group_BecomesGWithChildrenInOrder()` - _(no documentation)_
- `public void ToUtf8_NestedGroups_Nest()` - _(no documentation)_
- `public void ToUtf8_NamedGroup_HasInkscapeLabel_UnnamedHasNone()` - _(no documentation)_
- `public void ToUtf8_HiddenLeaf_IsLeftOut()` - _(no documentation)_
- `public void ToUtf8_HiddenGroup_IsLeftOutWithItsChildren()` - _(no documentation)_
- `public void ToUtf8_HiddenChildInVisibleGroup_IsLeftOutOnly()` - _(no documentation)_
- `public void ToUtf8_LockedShapes_AreExported()` - _(no documentation)_
- `public void ToUtf8_RotatedChildInGroup_KeepsItsOwnTransform()` - _(no documentation)_
- `public void ToUtf8_GroupWithOnlyHiddenChildren_IsAnEmptyG()` - _(no documentation)_

## SvgExportPathTests (class)

`public class SvgExportPathTests`

Source: `tests/PressToShape.Core.Tests/IO/SvgExportPathTests.cs:12-159`

_(no documentation)_

**Members**

- `public void ToUtf8_ClosedLinePath_WritesMLLAndZ()` - _(no documentation)_
- `public void ToUtf8_OpenLinePath_HasNoZ()` - _(no documentation)_
- `public void ToUtf8_CurvedSegment_WritesC()` - _(no documentation)_
- `public void ToUtf8_SegmentWithOnlyOneHandle_UsesTheAnchorForTheMissingOne()` - _(no documentation)_
- `public void ToUtf8_ClosedPathWithACurvedClosingSegment_WritesTheCurveBeforeZ()` - _(no documentation)_
- `public void ToUtf8_EvenOddPath_WritesFillRule()` - _(no documentation)_
- `public void ToUtf8_PathAppearance_IsWrittenLikeOtherShapes()` - _(no documentation)_
- `public void ToUtf8_RotatedPath_TurnsAroundTheCenterOfItsBox()` - _(no documentation)_
- `public void ToUtf8_PathAfterAResizeWithWith_WritesTheScaledCoordinates()` - _(no documentation)_
- `public void ToUtf8_PathInAGroup_IsWrittenInsideTheGroup()` - _(no documentation)_
- `public void ToUtf8_HiddenPathAndPathInAHiddenLayer_AreLeftOut()` - _(no documentation)_
- `public void ToUtf8_GermanCulture_StillWritesDots()` - _(no documentation)_

## SvgExportRoundedTests (class)

`public class SvgExportRoundedTests`

Source: `tests/PressToShape.Core.Tests/IO/SvgExportRoundedTests.cs:11-119`

_(no documentation)_

**Members**

- `public void ToUtf8_ZeroRadii_IsAPlainRectWithoutRadiusAttributes()` - _(no documentation)_
- `public void ToUtf8_EqualRadii_WritesRxAndRy()` - _(no documentation)_
- `public void ToUtf8_EqualRadiiAboveTheLimit_WritesTheLimitedRadius()` - _(no documentation)_
- `public void ToUtf8_DifferentStoredRadiiThatAreLimitedToTheSame_StayARect()` - _(no documentation)_
- `public void ToUtf8_DifferentRadii_IsAPathWithArcs()` - _(no documentation)_
- `public void ToUtf8_DifferentRadiiOnEveryCorner_HasFourArcsAndCloses()` - _(no documentation)_
- `public void ToUtf8_DifferentRadii_KeepsFillStrokeOpacityAndRotation()` - _(no documentation)_
- `public void ToUtf8_EqualRadii_KeepsFillStrokeOpacityAndRotation()` - _(no documentation)_

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
