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

`T:PressToShape.App.Controls.DocumentCanvasControl` at src/PressToShape.App/Controls/DocumentCanvasControl.cs:17, base `Control`.
Draws the shapes of a document. It only draws: PressToShape.App.Workspace.CanvasScene decides what is drawn and where. Shape colors are document data, so this control uses no theme brushes. It does not handle pointer input.

**Properties**

- `DimOpacity` (Styled, `double`, default 0.35) at line 71
- `Document` (Styled, `Document?`, default not set) at line 19
- `Draft` (Styled, `ScreenShape?`, default not set) at line 35
- `EnteredGroupId` (Styled, `Guid?`, default not set) at line 67
- `GridBrush` (Styled, `IBrush?`, default not set) at line 83
- `GridSpacing` (Styled, `double`, default 10) at line 79
- `GuideBrush` (Styled, `IBrush?`, default not set) at line 91
- `HandleSize` (Styled, `double`, default SelectionOverlay.HandleSize) at line 63
- `HiddenShapeId` (Styled, `Guid?`, default not set) at line 51
- `NodeOverlay` (Styled, `ScreenNodeOverlay?`, default not set) at line 47
- `OriginX` (Styled, `double`, default not set) at line 27
- `OriginY` (Styled, `double`, default not set) at line 31
- `Overlay` (Styled, `SelectionOverlay?`, default not set) at line 39
- `PenOverlay` (Styled, `ScreenPenOverlay?`, default not set) at line 43
- `SelectionBrush` (Styled, `IBrush?`, default not set) at line 55
- `SelectionHandleFillBrush` (Styled, `IBrush?`, default not set) at line 59
- `ShowGrid` (Styled, `bool`, default not set) at line 75
- `SnapGuides` (Styled, `IReadOnlyList<ScreenSnapGuide>?`, default not set) at line 87
- `ZoomPercent` (Styled, `double`, default 100) at line 23

**Events**

none found


## RingMenuControl

`T:PressToShape.App.Controls.RingMenuControl` at src/PressToShape.App/Controls/RingMenuControl.cs:23, base `Panel`.
The ring context menu. It fills the canvas area (so a press beside the ring is caught here and closes it), and draws the wedges, their icons and the center around the point where the ring was opened. It draws what PressToShape.App.ViewModels.Editor.RingMenuViewModel holds and forwards pointer and key input to it; PressToShape.App.Workspace.RingGeometry does the math. The colors and sizes come from the theme (see the styles of the classes ringWedge, ringIcon, ringCenter and ringCenterText).

**Properties**

- `InnerRadius` (Styled, `double`, default 34) at line 25
- `OuterRadius` (Styled, `double`, default 110) at line 28

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


## ToolGroupButton

`T:PressToShape.App.Controls.ToolGroupButton` at src/PressToShape.App/Controls/ToolGroupButton.cs:16, base `ToolToggleButton`.
The button of a tool group in the tablet tool column. A tap selects the current tool of the group (see PressToShape.App.Controls.ToolToggleButton). Holding the pointer down for a while runs PressToShape.App.Controls.ToolGroupButton.LongPressCommand (which opens the flyout of the group), and the click that follows the release is then swallowed, so a long press never also selects the tool. The decision is made by PressToShape.App.Workspace.LongPressTracker; this control only passes it pointer events and the time. It looks like a plain toggle button, so the tool styles apply to it.

**Properties**

- `LongPressCommand` (Styled, `ICommand?`, default not set) at line 20

**Events**

none found


## ToolToggleButton

`T:PressToShape.App.Controls.ToolToggleButton` at src/PressToShape.App/Controls/ToolToggleButton.cs:12, base `ToggleButton`.
A toggle button for something whose state lives in a view model: a tool in the tool column, or the icon that shows whether a panel is open. Its checked state comes only from the binding. A tap runs the command and does not flip the button by itself, so tapping the active tool again keeps it active (a plain toggle button would show it as off while the tool is still the active one, and would also lose its binding). It looks like a plain toggle button, so the tool styles apply.

**Properties**

none found

**Events**

none found

