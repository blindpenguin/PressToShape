# Custom controls

Classes that derive from `Avalonia.Controls.Control` and have no XAML file, read from their registration calls.

## CollapsibleSection

`T:PressToShape.App.Controls.CollapsibleSection` at src/PressToShape.App/Controls/CollapsibleSection.cs:12, base `HeaderedContentControl`.
A titled section whose whole header row toggles the content. Used by the inspector panels. The look is defined in Styles/Controls.axaml.

**Properties**

- `IsExpanded` (Styled, `bool`, default true) at line 14

**Events**

none found


## DocumentCanvasControl

`T:PressToShape.App.Controls.DocumentCanvasControl` at src/PressToShape.App/Controls/DocumentCanvasControl.cs:15, base `Control`.
Draws the shapes of a document. It only draws: PressToShape.App.Workspace.CanvasScene decides what is drawn and where. Shape colors are document data, so this control uses no theme brushes. It does not handle pointer input.

**Properties**

- `Document` (Styled, `Document?`, default not set) at line 17
- `Draft` (Styled, `ScreenShape?`, default not set) at line 33
- `OriginX` (Styled, `double`, default not set) at line 25
- `OriginY` (Styled, `double`, default not set) at line 29
- `Overlay` (Styled, `SelectionOverlay?`, default not set) at line 37
- `SelectionBrush` (Styled, `IBrush?`, default not set) at line 41
- `SelectionHandleFillBrush` (Styled, `IBrush?`, default not set) at line 45
- `ZoomPercent` (Styled, `double`, default 100) at line 21

**Events**

none found


## RulerControl

`T:PressToShape.App.Controls.RulerControl` at src/PressToShape.App/Controls/RulerControl.cs:14, base `Control`.
A horizontal or vertical ruler. It asks PressToShape.App.Workspace.RulerTicks for the ticks of the current zoom and origin and draws them with the theme brushes. It does not know about documents or the canvas.

**Properties**

- `Orientation` (Styled, `Orientation`, default Orientation.Horizontal) at line 16
- `OriginPx` (Styled, `double`, default not set) at line 24
- `ZoomPercent` (Styled, `double`, default 100) at line 20

**Events**

none found

