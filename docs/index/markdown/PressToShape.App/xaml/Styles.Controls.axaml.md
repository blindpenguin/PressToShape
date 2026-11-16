# Controls.axaml (XAML)

Project: PressToShape.App. File: `src/PressToShape.App/Styles/Controls.axaml`. Root: `Styles`.
No `x:Class`.

## Bindings

- line 23: PART_ContentPresenter.Content = TemplateBinding `Header` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 24: PART_ContentPresenter.ContentTemplate = TemplateBinding `HeaderTemplate` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 25: PART_ContentPresenter.Foreground = TemplateBinding `Foreground` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 26: PART_ContentPresenter.FontSize = TemplateBinding `FontSize` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 57: ToggleButton.IsChecked = TemplateBinding `IsExpanded` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 60: ContentPresenter.Content = TemplateBinding `Header` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 70: ContentPresenter.Content = TemplateBinding `Content` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 71: ContentPresenter.ContentTemplate = TemplateBinding `ContentTemplate` -> unresolved: a TemplateBinding targets the templated control, not the data context
- line 72: ContentPresenter.IsVisible = TemplateBinding `IsExpanded` -> unresolved: a TemplateBinding targets the templated control, not the data context

## Resource references

- line 10: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 17: PART_LayoutRoot.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 31: PART_SelectedPipe.Background = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 38: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 41: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 44: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 54: Border.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 61: ContentPresenter.FontSize = DynamicResource `SectionHeaderFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SectionHeaderFontSize` (verified)
- line 65: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 65: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 66: materialIcons:MaterialIcon.Height = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 66: materialIcons:MaterialIcon.Width = DynamicResource `PanelIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelIconSize` (verified)
- line 85: Setter.MinWidth = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 86: Setter.MinHeight = DynamicResource `ToolbarButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolbarButtonSize` (verified)
- line 87: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 88: Setter.CornerRadius = StaticResource `ControlCornerRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlCornerRadius` (verified)
- line 93: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 96: Setter.Background = DynamicResource `ControlPressedBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlPressedBrush@Light` (verified)
- line 100: Setter.Foreground = DynamicResource `TextDisabledBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextDisabledBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextDisabledBrush@Light` (verified)
- line 108: Setter.Width = DynamicResource `ToolButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolButtonSize` (verified)
- line 109: Setter.Height = DynamicResource `ToolButtonSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ToolButtonSize` (verified)
- line 110: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 111: Setter.CornerRadius = StaticResource `ControlCornerRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlCornerRadius` (verified)
- line 116: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 119: Setter.Background = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 120: Setter.Foreground = DynamicResource `AppAccentForegroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentForegroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentForegroundBrush@Light` (verified)
- line 123: Setter.Background = DynamicResource `AppAccentBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/AppAccentBrush@Light` (verified)
- line 126: Setter.Foreground = DynamicResource `TextDisabledBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextDisabledBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextDisabledBrush@Light` (verified)
- line 137: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 141: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 144: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 148: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 151: Setter.Background = DynamicResource `ControlHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlHoverBrush@Light` (verified)
- line 164: Setter.Background = DynamicResource `PanelBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/PanelBackgroundBrush@Light` (verified)
- line 165: Setter.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 168: Setter.FontSize = DynamicResource `SectionHeaderFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SectionHeaderFontSize` (verified)
- line 170: Setter.Foreground = DynamicResource `TextPrimaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextPrimaryBrush@Light` (verified)
- line 174: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 181: Setter.Foreground = DynamicResource `TextSecondaryBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TextSecondaryBrush@Light` (verified)
- line 184: Setter.MinHeight = DynamicResource `FieldHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/FieldHeight` (verified)
- line 185: Setter.Height = DynamicResource `FieldHeight` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/FieldHeight` (verified)
- line 188: Setter.Background = DynamicResource `ControlBackgroundBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlBackgroundBrush@Light` (verified)
- line 189: Setter.BorderBrush = DynamicResource `BorderSubtleBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/BorderSubtleBrush@Light` (verified)
- line 190: Setter.CornerRadius = StaticResource `ControlCornerRadius` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/ControlCornerRadius` (verified)
- line 191: Setter.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)
- line 199: Setter.MinHeight = DynamicResource `TouchTargetSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/TouchTargetSize` (verified)
- line 207: Setter.Fill = DynamicResource `RingWedgeBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeBrush@Light` (verified)
- line 208: Setter.Stroke = DynamicResource `RingWedgeOutlineBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeOutlineBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeOutlineBrush@Light` (verified)
- line 212: Setter.Fill = DynamicResource `RingWedgeHoverBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeHoverBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeHoverBrush@Light` (verified)
- line 215: Setter.Fill = DynamicResource `RingWedgeFocusBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeFocusBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeFocusBrush@Light` (verified)
- line 216: Setter.Stroke = DynamicResource `SelectionOutlineBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionOutlineBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/SelectionOutlineBrush@Light` (verified)
- line 220: Setter.Fill = DynamicResource `RingWedgeBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeBrush@Light` (verified)
- line 223: Setter.Foreground = DynamicResource `RingIconBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconBrush@Light` (verified)
- line 224: Setter.Width = DynamicResource `RingIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconSize` (verified)
- line 225: Setter.Height = DynamicResource `RingIconSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconSize` (verified)
- line 228: Setter.Foreground = DynamicResource `RingIconUnavailableBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconUnavailableBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconUnavailableBrush@Light` (verified)
- line 231: Setter.Fill = DynamicResource `RingCenterBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingCenterBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingCenterBrush@Light` (verified)
- line 232: Setter.Stroke = DynamicResource `RingWedgeOutlineBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeOutlineBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingWedgeOutlineBrush@Light` (verified)
- line 236: Setter.Foreground = DynamicResource `RingIconBrush` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconBrush@Dark`, `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/RingIconBrush@Light` (verified)
- line 237: Setter.FontSize = DynamicResource `UiFontSize` -> `R:src/PressToShape.App/Styles/Theme.axaml#/ResourceDictionary/UiFontSize` (verified)

## Resources defined

- line 9: `InspectorTabItemTheme` (ControlTheme)
- line 51: `{x:Type controls:CollapsibleSection}` (ControlTheme)

## Styles

- line 37: `^:pointerover /template/ Border#PART_LayoutRoot`, sets Background, uses ControlHoverBrush
- line 40: `^:pointerover`, sets Foreground, uses TextPrimaryBrush
- line 43: `^:selected`, sets Foreground, uses TextPrimaryBrush
- line 46: `^:selected /template/ Border#PART_SelectedPipe`, sets IsVisible
- line 81: `Button.icon`, sets Background, BorderThickness, CornerRadius, Foreground, HorizontalContentAlignment, MinHeight, MinWidth, Padding, VerticalContentAlignment, uses ControlCornerRadius, TextPrimaryBrush, ToolbarButtonSize
- line 92: `Button.icon:pointerover /template/ ContentPresenter#PART_ContentPresenter`, sets Background, uses ControlHoverBrush
- line 95: `Button.icon:pressed /template/ ContentPresenter#PART_ContentPresenter`, sets Background, uses ControlPressedBrush
- line 98: `Button.icon:disabled /template/ ContentPresenter#PART_ContentPresenter`, sets Background, Foreground, uses TextDisabledBrush
- line 104: `ToggleButton.tool`, sets Background, BorderThickness, CornerRadius, Foreground, Height, HorizontalContentAlignment, Padding, VerticalContentAlignment, Width, uses ControlCornerRadius, TextPrimaryBrush, ToolButtonSize
- line 115: `ToggleButton.tool:pointerover /template/ ContentPresenter#PART_ContentPresenter`, sets Background, uses ControlHoverBrush
- line 118: `ToggleButton.tool:checked /template/ ContentPresenter#PART_ContentPresenter`, sets Background, Foreground, uses AppAccentBrush, AppAccentForegroundBrush
- line 122: `ToggleButton.tool:checked:pointerover /template/ ContentPresenter#PART_ContentPresenter`, sets Background, uses AppAccentBrush
- line 125: `ToggleButton.tool:disabled /template/ ContentPresenter#PART_ContentPresenter`, sets Foreground, uses TextDisabledBrush
- line 130: `ToggleButton.section-toggle`, sets Background, BorderThickness, CornerRadius, Foreground, HorizontalAlignment, HorizontalContentAlignment, Padding, uses TextPrimaryBrush
- line 139: `ToggleButton.section-toggle /template/ ContentPresenter#PART_ContentPresenter`, sets Background, Foreground, uses TextPrimaryBrush
- line 143: `ToggleButton.section-toggle:pointerover /template/ ContentPresenter#PART_ContentPresenter`, sets Background, uses ControlHoverBrush
- line 146: `ToggleButton.section-toggle:checked /template/ ContentPresenter#PART_ContentPresenter`, sets Background, Foreground, uses TextPrimaryBrush
- line 150: `ToggleButton.section-toggle:checked:pointerover /template/ ContentPresenter#PART_ContentPresenter`, sets Background, uses ControlHoverBrush
- line 153: `ToggleButton.section-toggle:checked materialIcons|MaterialIcon.collapsed`, sets IsVisible
- line 156: `ToggleButton.section-toggle:not(:checked) materialIcons|MaterialIcon.expanded`, sets IsVisible
- line 163: `Border.panel`, sets Background, BorderBrush, uses BorderSubtleBrush, PanelBackgroundBrush
- line 167: `TextBlock.section-header`, sets FontSize, FontWeight, Foreground, VerticalAlignment, uses SectionHeaderFontSize, TextPrimaryBrush
- line 173: `TextBlock.secondary`, sets Foreground, uses TextSecondaryBrush
- line 178: `TextBlock.unit`, sets Foreground, Margin, VerticalAlignment, uses TextSecondaryBrush
- line 183: `TextBox.field`, sets Background, BorderBrush, CornerRadius, FontSize, Height, MinHeight, Padding, VerticalContentAlignment, uses BorderSubtleBrush, ControlBackgroundBrush, ControlCornerRadius, FieldHeight, UiFontSize
- line 198: `.touch Button, .touch ToggleButton, .touch DropDownButton, .touch CheckBox, .touch TextBox, .touch MenuItem, .touch ListBoxItem, .touch TabItem, .touch Slider`, sets MinHeight, uses TouchTargetSize
- line 206: `Path.ringWedge`, sets Fill, Stroke, StrokeThickness, uses RingWedgeBrush, RingWedgeOutlineBrush
- line 211: `Path.ringWedge.hovered`, sets Fill, uses RingWedgeHoverBrush
- line 214: `Path.ringWedge.focused`, sets Fill, Stroke, StrokeThickness, uses RingWedgeFocusBrush, SelectionOutlineBrush
- line 219: `Path.ringWedge.unavailable.hovered`, sets Fill, uses RingWedgeBrush
- line 222: `materialIcons|MaterialIcon.ringIcon`, sets Foreground, Height, Width, uses RingIconBrush, RingIconSize
- line 227: `materialIcons|MaterialIcon.ringIcon.unavailable`, sets Foreground, uses RingIconUnavailableBrush
- line 230: `Ellipse.ringCenter`, sets Fill, Stroke, StrokeThickness, uses RingCenterBrush, RingWedgeOutlineBrush
- line 235: `TextBlock.ringCenterText`, sets FontSize, Foreground, TextAlignment, TextWrapping, uses RingIconBrush, UiFontSize

## Templates

- line 14: ControlTemplate
- line 53: ControlTemplate
