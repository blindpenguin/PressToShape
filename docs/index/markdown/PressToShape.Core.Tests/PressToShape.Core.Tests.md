# PressToShape.Core.Tests

Project: PressToShape.Core.Tests

## CoreArchitectureTests (class)

`public class CoreArchitectureTests`

Source: `tests/PressToShape.Core.Tests/CoreArchitectureTests.cs:9-89`

_(no documentation)_

**Members**

- `public void CoreAssembly_IsLoadable()` - _(no documentation)_
- `public void Core_References_NoAvaloniaAssembly()` - _(no documentation)_
- `public void Core_References_NoOtherSolutionProject()` - _(no documentation)_
- `public void Clipper2_IsUsedOnlyByBooleanEngine()` - _(no documentation)_

## FakeTextMeasurer (class)

`internal sealed class FakeTextMeasurer : ITextMeasurer`

Source: `tests/PressToShape.Core.Tests/FakeTextMeasurer.cs:14-81`

A deterministic text engine for tests. Each character is 0.6 * FontSize + LetterSpacing wide, a line is FontSize * LineSpacing * 1.2 high and the first baseline is one font size below the top. Area text wraps on spaces at the box width (a word longer than the width stays whole on its own line), \n always breaks. Alignment is relative to the box width for area text and to the widest line for point text.

**Members**

- `public double CharWidth(TextShape text)` - _(no documentation)_
- `public double CharWidth(TextShape text)` - _(no documentation)_
- `public double CharWidth(TextShape text)` - _(no documentation)_
- `public double LineHeight(TextShape text)` - _(no documentation)_
- `public double LineHeight(TextShape text)` - _(no documentation)_
- `public double LineHeight(TextShape text)` - _(no documentation)_
- `public TextLayoutResult Layout(TextShape text)` - _(no documentation)_
- `public TextLayoutResult Layout(TextShape text)` - _(no documentation)_
- `public TextLayoutResult Layout(TextShape text)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.Core.Shapes.ITextMeasurer`

## FakeTextMeasurerTests (class)

`public class FakeTextMeasurerTests`

Source: `tests/PressToShape.Core.Tests/FakeTextMeasurerTests.cs:7-113`

_(no documentation)_

**Members**

- `public void Layout_EmptyText_HasNoLines()` - _(no documentation)_
- `public void Layout_PointText_OneLineHasSizeOfItsCharacters()` - _(no documentation)_
- `public void Layout_Newline_BreaksTheLine()` - _(no documentation)_
- `public void Layout_PointText_DoesNotWrap()` - _(no documentation)_
- `public void Layout_AreaText_WrapsOnSpacesAtTheBoxWidth()` - _(no documentation)_
- `public void Layout_AreaTextNarrowBox_KeepsOneWordPerLine()` - _(no documentation)_
- `public void Layout_PointText_AlignsShorterLinesToTheWidest(TextAlignType alignment, double expectedX)` - _(no documentation)_
- `public void Layout_AreaText_AlignsToTheBox(TextAlignType alignment, double expectedX)` - _(no documentation)_
- `public void Outline_TextWithBlanks_GivesOneContourPerVisibleCharacter()` - _(no documentation)_
- `public void Outline_NothingVisible_ReturnsNull(string content)` - _(no documentation)_

## FakeTextOutliner (class)

`internal sealed class FakeTextOutliner : ITextOutliner`

Source: `tests/PressToShape.Core.Tests/FakeTextMeasurer.cs:84-118`

A deterministic outliner for tests: one square contour for each character that is not white space.

**Members**

- `public IReadOnlyList<DocumentSubPath>? Outline(TextShape text)` - _(no documentation)_
- `public IReadOnlyList<DocumentSubPath>? Outline(TextShape text)` - _(no documentation)_
- `public IReadOnlyList<DocumentSubPath>? Outline(TextShape text)` - _(no documentation)_

**Relationships**

- Implements `PressToShape.Core.Shapes.ITextOutliner`
