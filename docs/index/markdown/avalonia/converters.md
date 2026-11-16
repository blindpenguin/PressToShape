# Value converters

Classes that implement `IValueConverter` or `IMultiValueConverter`. Uses are the XAML bindings that name the converter.

## CheckableToggleTypeConverter

`T:PressToShape.App.Views.Desktop.CheckableToggleTypeConverter` at src/PressToShape.App/Views/Desktop/CheckableToggleTypeConverter.cs:9, implements `IValueConverter`.
Maps the view model flag IsCheckable to a radio toggle for menu items. Other items show no toggle.

- used at src/PressToShape.App/Views/Desktop/MenuBarView.axaml:38
- used at src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml:35

## GestureTextConverter

`T:PressToShape.App.Views.Desktop.GestureTextConverter` at src/PressToShape.App/Views/Desktop/GestureTextConverter.cs:9, implements `IValueConverter`.
Turns a shortcut text such as "Ctrl+Shift+S" into a Avalonia.Input.KeyGesture so the menu can display it.

- used at src/PressToShape.App/Views/Desktop/MenuBarView.axaml:37

## CoreColorBrushConverter

`T:PressToShape.App.Views.Editor.CoreColorBrushConverter` at src/PressToShape.App/Views/Editor/CoreColorConverter.cs:28, implements `IValueConverter`.
Turns the Core color of a shape into a brush, for swatches. It does not convert back.

- used at src/PressToShape.App/Views/Editor/PropertiesView.axaml:147
- used at src/PressToShape.App/Views/Editor/PropertiesView.axaml:156
- used at src/PressToShape.App/Views/Editor/PropertiesView.axaml:187
- used at src/PressToShape.App/Views/Editor/PropertiesView.axaml:196
- used at src/PressToShape.App/Views/Editor/PropertiesView.axaml:243
- used at src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml:30
- used at src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml:36

## CoreColorConverter

`T:PressToShape.App.Views.Editor.CoreColorConverter` at src/PressToShape.App/Views/Editor/CoreColorConverter.cs:12, implements `IValueConverter`.
Turns the Core color of a shape into an Avalonia color and back. Alpha is kept.

not used

## ObjectIndentConverter

`T:PressToShape.App.Views.Editor.ObjectIndentConverter` at src/PressToShape.App/Views/Editor/ObjectIndentConverter.cs:8, implements `IValueConverter`.
Turns the depth of a row of the object list into the width of the empty space in front of it. The view sets PressToShape.App.Views.Editor.ObjectIndentConverter.Indent from the theme.

- used at src/PressToShape.App/Views/Editor/LayersView.axaml:79
