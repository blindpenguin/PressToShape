# Styles and resources

Only XAML files of this solution are read. Keys that the framework or a theme defines show up as not defined here, and keys that override a framework key show up as never referenced, because the framework uses them without any XAML naming them.

## Files

### src/PressToShape.App/PressToShapeApp.axaml

- 0 resources, 0 styles
- includes `src/PressToShape.App/Styles/Controls.axaml` (StyleInclude avares://PressToShape.App/Styles/Controls.axaml)
- includes `src/PressToShape.App/Styles/Theme.axaml` (ResourceInclude avares://PressToShape.App/Styles/Theme.axaml)

### src/PressToShape.App/Styles/Controls.axaml

- 2 resources, 41 styles
- style `^:pointerover /template/ Border#PART_LayoutRoot` at line 37, 1 setters
- style `^:pointerover` at line 40, 1 setters
- style `^:selected` at line 43, 1 setters
- style `^:selected /template/ Border#PART_SelectedPipe` at line 46, 1 setters
- style `Button.icon` at line 81, 9 setters
- style `Button.icon:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 92, 1 setters
- style `Button.icon:pressed /template/ ContentPresenter#PART_ContentPresenter` at line 95, 1 setters
- style `Button.icon:disabled /template/ ContentPresenter#PART_ContentPresenter` at line 98, 2 setters
- style `ToggleButton.tool` at line 104, 9 setters
- style `ToggleButton.tool:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 115, 1 setters
- style `ToggleButton.tool:checked /template/ ContentPresenter#PART_ContentPresenter` at line 118, 2 setters
- style `ToggleButton.tool:checked:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 122, 1 setters
- style `ToggleButton.tool:disabled /template/ ContentPresenter#PART_ContentPresenter` at line 125, 1 setters
- style `ToggleButton.snap` at line 130, 7 setters
- style `ToggleButton.snap:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 139, 1 setters
- style `ToggleButton.snap:checked /template/ ContentPresenter#PART_ContentPresenter` at line 142, 2 setters
- style `ToggleButton.snap:checked:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 146, 1 setters
- style `Button.choice` at line 151, 2 setters
- style `Button.choice.selected /template/ ContentPresenter#PART_ContentPresenter` at line 155, 2 setters
- style `Button.choice.selected:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 159, 1 setters
- style `ToggleButton.section-toggle` at line 164, 7 setters
- style `ToggleButton.section-toggle /template/ ContentPresenter#PART_ContentPresenter` at line 173, 2 setters
- style `ToggleButton.section-toggle:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 177, 1 setters
- style `ToggleButton.section-toggle:checked /template/ ContentPresenter#PART_ContentPresenter` at line 180, 2 setters
- style `ToggleButton.section-toggle:checked:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 184, 1 setters
- style `ToggleButton.section-toggle:checked materialIcons|MaterialIcon.collapsed` at line 187, 1 setters
- style `ToggleButton.section-toggle:not(:checked) materialIcons|MaterialIcon.expanded` at line 190, 1 setters
- style `Border.panel` at line 197, 2 setters
- style `TextBlock.section-header` at line 201, 4 setters
- style `TextBlock.secondary` at line 207, 1 setters
- style `TextBlock.unit` at line 212, 3 setters
- style `TextBox.field` at line 217, 8 setters
- style `.touch Button, .touch ToggleButton, .touch DropDownButton, .touch CheckBox, .touch TextBox, .touch MenuItem, .touch ListBoxItem, .touch TabItem, .touch Slider` at line 232, 1 setters
- style `Path.ringWedge` at line 240, 3 setters
- style `Path.ringWedge.hovered` at line 245, 1 setters
- style `Path.ringWedge.focused` at line 248, 3 setters
- style `Path.ringWedge.unavailable.hovered` at line 253, 1 setters
- style `materialIcons|MaterialIcon.ringIcon` at line 256, 3 setters
- style `materialIcons|MaterialIcon.ringIcon.unavailable` at line 261, 1 setters
- style `Ellipse.ringCenter` at line 264, 3 setters
- style `TextBlock.ringCenterText` at line 269, 4 setters

### src/PressToShape.App/Styles/Theme.axaml

- 124 resources, 0 styles, variants Dark, Light

### src/PressToShape.App/Views/Desktop/MenuBarView.axaml

- 4 resources, 0 styles

### src/PressToShape.App/Views/Desktop/StatusBarView.axaml

- 0 resources, 2 styles
- style `TextBlock.status` at line 14, 3 setters
- style `Button.icon.small` at line 19, 2 setters

### src/PressToShape.App/Views/Desktop/ToolbarView.axaml

- 1 resources, 1 styles
- style `Border.group-separator` at line 25, 3 setters

### src/PressToShape.App/Views/Editor/HistoryView.axaml

- 0 resources, 2 styles
- style `ListBoxItem` at line 13, 2 setters
- style `Grid.redoable` at line 18, 1 setters

### src/PressToShape.App/Views/Editor/InspectorView.axaml

- 0 resources, 1 styles
- style `TabItem` at line 12, 1 setters

### src/PressToShape.App/Views/Editor/LayersView.axaml

- 1 resources, 8 styles
- style `Button.rowButton` at line 19, 2 setters
- style `Border.objectRow` at line 23, 3 setters
- style `Border.objectRow:pointerover` at line 28, 1 setters
- style `Border.objectRow.current` at line 32, 1 setters
- style `Border.objectRow.selected` at line 35, 1 setters
- style `Border.objectRow.activeLayer TextBlock` at line 39, 1 setters
- style `Border.objectRow.drop` at line 42, 1 setters
- style `Border.objectRow.inactive TextBlock` at line 45, 1 setters

### src/PressToShape.App/Views/Editor/PropertiesView.axaml

- 0 resources, 4 styles
- style `TextBlock.label` at line 16, 3 setters
- style `Button.swatchButton` at line 21, 4 setters
- style `Border.smallSwatch` at line 27, 5 setters
- style `Border.swatch` at line 34, 5 setters

### src/PressToShape.App/Views/Editor/SwatchesView.axaml

- 0 resources, 1 styles
- style `Button.swatchAction` at line 14, 4 setters

### src/PressToShape.App/Views/Editor/ToolboxView.axaml

- 1 resources, 0 styles

### src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml

- 0 resources, 1 styles
- style `Button.colorSwatch` at line 13, 9 setters

### src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml

- 1 resources, 1 styles
- style `ToggleButton.tool.flyoutTool` at line 16, 7 setters

### src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml

- 2 resources, 1 styles
- style `MenuFlyoutPresenter` at line 18, 1 setters

## Include graph from the application file

- src/PressToShape.App/PressToShapeApp.axaml
  - src/PressToShape.App/Styles/Controls.axaml
  - src/PressToShape.App/Styles/Theme.axaml

## Keys referenced but not defined

- `FluentTopLevelMenuItem`, 1 uses
- `{x:Type MenuItem}`, 4 uses

## Keys defined but never referenced

- `{x:Type controls:CollapsibleSection}` (ControlTheme) at src/PressToShape.App/Styles/Controls.axaml:51
- `SystemAccentColor` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:13
- `SystemAccentColorLight1` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:14
- `SystemAccentColorLight2` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:15
- `SystemAccentColorLight3` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:16
- `SystemAccentColorDark1` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:17
- `SystemAccentColorDark2` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:18
- `SystemAccentColorDark3` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:19
- `RulerTickBrush` (SolidColorBrush, Dark) at src/PressToShape.App/Styles/Theme.axaml:51
- `RulerTextBrush` (SolidColorBrush, Dark) at src/PressToShape.App/Styles/Theme.axaml:52
- `SystemAccentColor` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:73
- `SystemAccentColorLight1` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:74
- `SystemAccentColorLight2` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:75
- `SystemAccentColorLight3` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:76
- `SystemAccentColorDark1` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:77
- `SystemAccentColorDark2` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:78
- `SystemAccentColorDark3` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:79
- `RulerTickBrush` (SolidColorBrush, Light) at src/PressToShape.App/Styles/Theme.axaml:111
- `RulerTextBrush` (SolidColorBrush, Light) at src/PressToShape.App/Styles/Theme.axaml:112
- `PanelSpacing` (x:Double) at src/PressToShape.App/Styles/Theme.axaml:140
- `RowIndentSize` (x:Double) at src/PressToShape.App/Styles/Theme.axaml:178
- `TouchHandleGrabSize` (x:Double) at src/PressToShape.App/Styles/Theme.axaml:182
- `TouchSizes` (ResourceDictionary) at src/PressToShape.App/Styles/Theme.axaml:205
- `GestureTextConverter` (views:GestureTextConverter) at src/PressToShape.App/Views/Desktop/MenuBarView.axaml:12
- `CheckableToggleTypeConverter` (views:CheckableToggleTypeConverter) at src/PressToShape.App/Views/Desktop/MenuBarView.axaml:13
- `IndentConverter` (views:ObjectIndentConverter) at src/PressToShape.App/Views/Editor/LayersView.axaml:14
- `CheckableToggleTypeConverter` (desktopViews:CheckableToggleTypeConverter) at src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml:24
