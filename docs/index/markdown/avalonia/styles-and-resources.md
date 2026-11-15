# Styles and resources

Only XAML files of this solution are read. Keys that the framework or a theme defines show up as not defined here, and keys that override a framework key show up as never referenced, because the framework uses them without any XAML naming them.

## Files

### src/PressToShape.App/PressToShapeApp.axaml

- 0 resources, 0 styles
- includes `src/PressToShape.App/Styles/Controls.axaml` (StyleInclude avares://PressToShape.App/Styles/Controls.axaml)
- includes `src/PressToShape.App/Styles/Theme.axaml` (ResourceInclude avares://PressToShape.App/Styles/Theme.axaml)

### src/PressToShape.App/Styles/Controls.axaml

- 2 resources, 25 styles
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
- style `ToggleButton.section-toggle` at line 130, 7 setters
- style `ToggleButton.section-toggle /template/ ContentPresenter#PART_ContentPresenter` at line 139, 2 setters
- style `ToggleButton.section-toggle:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 143, 1 setters
- style `ToggleButton.section-toggle:checked /template/ ContentPresenter#PART_ContentPresenter` at line 146, 2 setters
- style `ToggleButton.section-toggle:checked:pointerover /template/ ContentPresenter#PART_ContentPresenter` at line 150, 1 setters
- style `ToggleButton.section-toggle:checked materialIcons|MaterialIcon.collapsed` at line 153, 1 setters
- style `ToggleButton.section-toggle:not(:checked) materialIcons|MaterialIcon.expanded` at line 156, 1 setters
- style `Border.panel` at line 163, 2 setters
- style `TextBlock.section-header` at line 167, 4 setters
- style `TextBlock.secondary` at line 173, 1 setters
- style `TextBlock.unit` at line 178, 3 setters
- style `TextBox.field` at line 183, 8 setters

### src/PressToShape.App/Styles/Theme.axaml

- 71 resources, 0 styles, variants Dark, Light

### src/PressToShape.App/Views/Desktop/MenuBarView.axaml

- 4 resources, 0 styles

### src/PressToShape.App/Views/Desktop/StatusBarView.axaml

- 0 resources, 2 styles
- style `TextBlock.status` at line 13, 3 setters
- style `Button.icon.small` at line 18, 2 setters

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

- 0 resources, 1 styles
- style `ListBoxItem` at line 13, 2 setters

### src/PressToShape.App/Views/Editor/PropertiesView.axaml

- 0 resources, 3 styles
- style `TextBlock.label` at line 15, 3 setters
- style `Button.swatchButton` at line 20, 4 setters
- style `Border.swatch` at line 26, 5 setters

### src/PressToShape.App/Views/Editor/ToolboxView.axaml

- 1 resources, 0 styles

## Include graph from the application file

- src/PressToShape.App/PressToShapeApp.axaml
  - src/PressToShape.App/Styles/Controls.axaml
  - src/PressToShape.App/Styles/Theme.axaml

## Keys referenced but not defined

- `FluentTopLevelMenuItem`, 1 uses
- `{x:Type MenuItem}`, 2 uses

## Keys defined but never referenced

- `{x:Type controls:CollapsibleSection}` (ControlTheme) at src/PressToShape.App/Styles/Controls.axaml:51
- `SystemAccentColor` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:13
- `SystemAccentColorLight1` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:14
- `SystemAccentColorLight2` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:15
- `SystemAccentColorLight3` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:16
- `SystemAccentColorDark1` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:17
- `SystemAccentColorDark2` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:18
- `SystemAccentColorDark3` (Color, Dark) at src/PressToShape.App/Styles/Theme.axaml:19
- `SelectedItemBrush` (SolidColorBrush, Dark) at src/PressToShape.App/Styles/Theme.axaml:22
- `RulerTickBrush` (SolidColorBrush, Dark) at src/PressToShape.App/Styles/Theme.axaml:51
- `RulerTextBrush` (SolidColorBrush, Dark) at src/PressToShape.App/Styles/Theme.axaml:52
- `SystemAccentColor` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:57
- `SystemAccentColorLight1` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:58
- `SystemAccentColorLight2` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:59
- `SystemAccentColorLight3` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:60
- `SystemAccentColorDark1` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:61
- `SystemAccentColorDark2` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:62
- `SystemAccentColorDark3` (Color, Light) at src/PressToShape.App/Styles/Theme.axaml:63
- `SelectedItemBrush` (SolidColorBrush, Light) at src/PressToShape.App/Styles/Theme.axaml:66
- `RulerTickBrush` (SolidColorBrush, Light) at src/PressToShape.App/Styles/Theme.axaml:95
- `RulerTextBrush` (SolidColorBrush, Light) at src/PressToShape.App/Styles/Theme.axaml:96
- `PanelSpacing` (x:Double) at src/PressToShape.App/Styles/Theme.axaml:108
- `GestureTextConverter` (views:GestureTextConverter) at src/PressToShape.App/Views/Desktop/MenuBarView.axaml:12
- `CheckableToggleTypeConverter` (views:CheckableToggleTypeConverter) at src/PressToShape.App/Views/Desktop/MenuBarView.axaml:13
