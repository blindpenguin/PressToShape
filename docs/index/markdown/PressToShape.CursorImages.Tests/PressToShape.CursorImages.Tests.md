# PressToShape.CursorImages.Tests

Project: PressToShape.CursorImages.Tests

## CommandTests (class)

`public sealed class CommandTests : IDisposable`

Source: `tests/PressToShape.CursorImages.Tests/CommandTests.cs:9-176`

_(no documentation)_

**Members**

- `public CommandTests()` - _(no documentation)_
- `public void Dispose()` - _(no documentation)_
- `public void Generate_WritesTwentyTwoImagesOfTheRightSize()` - _(no documentation)_
- `public void Generate_Twice_GivesIdenticalFiles()` - _(no documentation)_
- `public void Check_AfterGenerate_Passes()` - _(no documentation)_
- `public void Check_ImageMissing_FailsAndNamesTheFile()` - _(no documentation)_
- `public void Check_OnePixelChanged_FailsAndNamesTheFile()` - _(no documentation)_
- `public void Check_ImageOfTheWrongSize_Fails()` - _(no documentation)_
- `public void Generate_TargetCannotBeWritten_FailsAndLeavesNoTemporaryFile()` - _(no documentation)_
- `public void WriteAtomic_ReplacesAnExistingFile()` - _(no documentation)_
- `public void Generate_UnsupportedSvg_FailsWithFileNameAndWritesNothing()` - _(no documentation)_
- `public void Run_BadArguments_PrintsUsageAndFails(params string[] args)` - _(no documentation)_

## CursorRendererTests (class)

`public class CursorRendererTests`

Source: `tests/PressToShape.CursorImages.Tests/CursorRendererTests.cs:9-178`

_(no documentation)_

**Members**

- `public void Render_Size_IsExact(int size)` - _(no documentation)_
- `public void Render_Background_IsTransparent()` - _(no documentation)_
- `public void Render_Crosshair_DrawsTheRingAndLeavesTheMiddleOpen(int size)` - _(no documentation)_
- `public void Render_OutermostPixels_AreTransparentForEveryIcon(int size)` - _(no documentation)_
- `public void Render_Line_IsWhiteInTheMiddleAndDarkAtTheRim()` - _(no documentation)_
- `public void Render_SameIconTwice_GivesIdenticalPixels()` - _(no documentation)_
- `public void Render_LargeImage_HasTheSameShapeAsTheSmallImageAtDoubleSize()` - _(no documentation)_
- `public void Entries_HaveTheNamesOfTheApp()` - _(no documentation)_
- `public void Entries_UseUniqueNamesAndExistingIcons()` - _(no documentation)_
- `public void Sizes_AreTheTwoImageSizes()` - _(no documentation)_
- `public void Render_DrawingFails_DisposesTheBitmapAndThrowsTheOriginalException()` - _(no documentation)_
- `public void Render_Works_ReturnsABitmapThatTheCallerOwns()` - _(no documentation)_

## ImageFilesTests (class)

`public sealed class ImageFilesTests : IDisposable`

Source: `tests/PressToShape.CursorImages.Tests/ImageFilesTests.cs:7-72`

_(no documentation)_

**Members**

- `public ImageFilesTests()` - _(no documentation)_
- `public void Dispose()` - _(no documentation)_
- `public void WriteAtomic_MoveFailsAndTheCleanupFailsToo_ThrowsTheMoveException()` - _(no documentation)_
- `public void WriteAtomic_MoveFailsAndTheCleanupHasNoAccess_ThrowsTheMoveException()` - _(no documentation)_
- `public void WriteAtomic_MoveFails_RemovesTheTemporaryFileAndThrows()` - _(no documentation)_
- `public void WriteAtomic_Works_WritesTheContentAndLeavesNoTemporaryFile()` - _(no documentation)_

## RepoPaths (class)

`internal static class RepoPaths`

Source: `tests/PressToShape.CursorImages.Tests/RepoPaths.cs:6-23`

_(no documentation)_

**Members**

- `public string Root { get; }` - The repository root: the folder above the test output that holds PressToShape.sln.
- `public string Combine(string relativePath)` - _(no documentation)_

## SvgIconTests (class)

`public sealed class SvgIconTests : IDisposable`

Source: `tests/PressToShape.CursorImages.Tests/SvgIconTests.cs:9-148`

_(no documentation)_

**Members**

- `public SvgIconTests()` - _(no documentation)_
- `public void Dispose()` - _(no documentation)_
- `public void Load_Path_ReadsItsBounds()` - _(no documentation)_
- `public void Load_Circle_ReadsItsBounds()` - _(no documentation)_
- `public void Load_Line_ReadsItsBounds()` - _(no documentation)_
- `public void Load_SeveralElements_AreJoinedIntoOnePath()` - _(no documentation)_
- `public void Load_UnsupportedElement_FailsWithFileAndElementName(string body, string element)` - _(no documentation)_
- `public void Load_MissingFile_FailsWithFileName()` - _(no documentation)_
- `public void Load_InvalidPathData_FailsWithFileName()` - _(no documentation)_
- `public void Load_MissingOrBadNumber_Fails()` - _(no documentation)_
- `public void Load_NotXml_FailsWithFileName()` - _(no documentation)_
- `public void Load_OtherViewBox_Fails()` - _(no documentation)_
- `public void Load_TheBundledLucideIcons_AllLoad()` - _(no documentation)_
