# Views and view models

Links come from `x:DataType`, DataContext bindings and assignments, and the ViewLocator convention. `inferred` links are not proven, the reason is given.

## Views

### RingMenuControl

`T:PressToShape.App.Controls.RingMenuControl` at src/PressToShape.App/Controls/RingMenuControl.cs:23. No XAML file.
The ring context menu. It fills the canvas area (so a press beside the ring is caught here and closes it), and draws the wedges, their icons and the center around the point where the ring was opened. It draws what PressToShape.App.ViewModels.Editor.RingMenuViewModel holds and forwards pointer and key input to it; PressToShape.App.Workspace.RingGeometry does the math. The colors and sizes come from the theme (see the styles of the classes ringWedge, ringIcon, ringCenter and ringCenterText).

- view model `RingMenuViewModel` (`T:PressToShape.App.ViewModels.Editor.RingMenuViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Editor/CanvasAreaView.axaml:67 (src/PressToShape.App/Views/Editor/CanvasAreaView.axaml:67); 0 commands, 5 test classes

### DesktopShellView

`T:PressToShape.App.Views.Desktop.DesktopShellView` at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml.cs:8. XAML: `src/PressToShape.App/Views/Desktop/DesktopShellView.axaml`, 20 bindings.
_(no documentation)_

- view model `DesktopShellViewModel` (`T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:1); 5 commands, 5 test classes

### MenuBarView

`T:PressToShape.App.Views.Desktop.MenuBarView` at src/PressToShape.App/Views/Desktop/MenuBarView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Desktop/MenuBarView.axaml`, 13 bindings.
_(no documentation)_

- view model `MenuBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.MenuBarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:35 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:35); 0 commands, 4 test classes
- view model `MenuBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.MenuBarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/MenuBarView.axaml:1); 0 commands, 4 test classes

### StatusBarView

`T:PressToShape.App.Views.Desktop.StatusBarView` at src/PressToShape.App/Views/Desktop/StatusBarView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Desktop/StatusBarView.axaml`, 11 bindings.
_(no documentation)_

- view model `StatusBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.StatusBarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:49); 4 commands, 3 test classes
- view model `StatusBarViewModel` (`T:PressToShape.App.ViewModels.Desktop.StatusBarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/StatusBarView.axaml:1); 4 commands, 3 test classes

### ToolbarView

`T:PressToShape.App.Views.Desktop.ToolbarView` at src/PressToShape.App/Views/Desktop/ToolbarView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Desktop/ToolbarView.axaml`, 13 bindings.
_(no documentation)_

- view model `ToolbarViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:38 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:38); 0 commands, 2 test classes
- view model `ToolbarViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Desktop/ToolbarView.axaml:1); 0 commands, 2 test classes

### CanvasAreaView

`T:PressToShape.App.Views.Editor.CanvasAreaView` at src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs:12. XAML: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml`, 34 bindings.
_(no documentation)_

- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:42); 0 commands, 19 test classes
- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/CanvasAreaView.axaml:1); 0 commands, 19 test classes
- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:28 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:28); 0 commands, 19 test classes
- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/DocumentCanvasControlNodeTests.cs:97); 0 commands, 19 test classes
- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/RingMenuControlTests.cs:43); 0 commands, 19 test classes
- view model `CanvasAreaViewModel` (`T:PressToShape.App.ViewModels.Editor.CanvasAreaViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:327); 0 commands, 19 test classes

### HistoryView

`T:PressToShape.App.Views.Editor.HistoryView` at src/PressToShape.App/Views/Editor/HistoryView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/HistoryView.axaml`, 7 bindings.
_(no documentation)_

- view model `HistoryViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/HistoryView.axaml:1); 0 commands, 5 test classes
- view model `HistoryViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:249); 0 commands, 5 test classes
- view model `HistoryViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.HistoryViewModel maps to PressToShape.App.Views.Editor.HistoryView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:35 (src/PressToShape.App/Views/Editor/InspectorView.axaml:35); 0 commands, 5 test classes

### InspectorView

`T:PressToShape.App.Views.Editor.InspectorView` at src/PressToShape.App/Views/Editor/InspectorView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/InspectorView.axaml`, 5 bindings.
_(no documentation)_

- view model `InspectorViewModel` (`T:PressToShape.App.ViewModels.Editor.InspectorViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:46 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:46); 0 commands, 10 test classes
- view model `InspectorViewModel` (`T:PressToShape.App.ViewModels.Editor.InspectorViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/InspectorView.axaml:1); 0 commands, 10 test classes
- view model `InspectorViewModel` (`T:PressToShape.App.ViewModels.Editor.InspectorViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/LayersViewTests.cs:152); 0 commands, 10 test classes

### LayersView

`T:PressToShape.App.Views.Editor.LayersView` at src/PressToShape.App/Views/Editor/LayersView.axaml.cs:16. XAML: `src/PressToShape.App/Views/Editor/LayersView.axaml`, 30 bindings.
The Layers tab. The view only turns pointer input into calls of the view model: a press selects, a double-tap renames, and a drag (after a long press for a finger) drops an object above another one of the same group.

- view model `LayersViewModel` (`T:PressToShape.App.ViewModels.Editor.LayersViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/LayersView.axaml:1); 1 commands, 5 test classes
- view model `LayersViewModel` (`T:PressToShape.App.ViewModels.Editor.LayersViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/LayersViewTests.cs:42); 1 commands, 5 test classes
- view model `LayersViewModel` (`T:PressToShape.App.ViewModels.Editor.LayersViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:244); 1 commands, 5 test classes
- view model `LayersViewModel` (`T:PressToShape.App.ViewModels.Editor.LayersViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.LayersViewModel maps to PressToShape.App.Views.Editor.LayersView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:29 (src/PressToShape.App/Views/Editor/InspectorView.axaml:29); 1 commands, 5 test classes

### PlaceholderPanelView

`T:PressToShape.App.Views.Editor.PlaceholderPanelView` at src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml`, 1 bindings.
_(no documentation)_

- view model `PlaceholderPanelViewModel` (`T:PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml:1); 0 commands, 2 test classes
- view model `PlaceholderPanelViewModel` (`T:PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.PlaceholderPanelViewModel maps to PressToShape.App.Views.Editor.PlaceholderPanelView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:32 (src/PressToShape.App/Views/Editor/InspectorView.axaml:32); 0 commands, 2 test classes

### PropertiesView

`T:PressToShape.App.Views.Editor.PropertiesView` at src/PressToShape.App/Views/Editor/PropertiesView.axaml.cs:15. XAML: `src/PressToShape.App/Views/Editor/PropertiesView.axaml`, 109 bindings.
The Properties tab. The code-behind only forwards: the number fields (shared with the contextual bar of the tablet layout), the pickers' open and close, the pick, add-to-palette and recent-color buttons of the pickers, the palette swatches (with the Shift state), and the opacity slider's drag. Every decision is in PressToShape.App.ViewModels.Editor.PropertiesViewModel.

- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/PropertiesView.axaml:1); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/PropertiesViewColorTests.cs:29); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/PropertiesViewCornersTests.cs:35); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/PropertiesViewNodeTests.cs:27); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/PropertiesViewNodeTests.cs:190); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/PropertiesViewPathTests.cs:23); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:159); 12 commands, 19 test classes
- view model `PropertiesViewModel` (`T:PressToShape.App.ViewModels.Editor.PropertiesViewModel`), inferred: ViewLocator (registered in Application.DataTemplates at src/PressToShape.App/PressToShapeApp.axaml:10) picks views by name: 'ViewModel' is replaced by 'View' in the full type name, so PressToShape.App.ViewModels.Editor.PropertiesViewModel maps to PressToShape.App.Views.Editor.PropertiesView. Evidence: ContentControl Content binding at src/PressToShape.App/Views/Editor/InspectorView.axaml:26 (src/PressToShape.App/Views/Editor/InspectorView.axaml:26); 12 commands, 19 test classes

### SwatchesView

`T:PressToShape.App.Views.Editor.SwatchesView` at src/PressToShape.App/Views/Editor/SwatchesView.axaml.cs:6. XAML: `src/PressToShape.App/Views/Editor/SwatchesView.axaml`, 2 bindings.
The fill and stroke swatches with the swap and reset buttons. Used by the desktop tool column and the tablet tool column.

- view model `ToolboxViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/SwatchesView.axaml:1); 3 commands, 6 test classes

### ToolboxView

`T:PressToShape.App.Views.Editor.ToolboxView` at src/PressToShape.App/Views/Editor/ToolboxView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Editor/ToolboxView.axaml`, 6 bindings.
_(no documentation)_

- view model `ToolboxViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:41 (src/PressToShape.App/Views/Desktop/DesktopShellView.axaml:41); 3 commands, 6 test classes
- view model `ToolboxViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`), verified: x:DataType (src/PressToShape.App/Views/Editor/ToolboxView.axaml:1); 3 commands, 6 test classes
- view model `ToolboxViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolboxViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TouchSizesTests.cs:67); 3 commands, 6 test classes

### MainWindow

`T:PressToShape.App.Views.MainWindow` at src/PressToShape.App/Views/MainWindow.axaml.cs:6. XAML: `src/PressToShape.App/Views/MainWindow.axaml`, 1 bindings.
_(no documentation)_

- view model `DesktopShellViewModel` (`T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`), verified: DataContext assignment (src/PressToShape.App/AppLayout.cs:40); 5 commands, 5 test classes
- view model `DesktopShellViewModel` (`T:PressToShape.App.ViewModels.Desktop.DesktopShellViewModel`), verified: x:DataType (src/PressToShape.App/Views/MainWindow.axaml:1); 5 commands, 5 test classes

### MessageDialogView

`T:PressToShape.App.Views.MessageDialogView` at src/PressToShape.App/Views/MessageDialogView.axaml.cs:6. XAML: `src/PressToShape.App/Views/MessageDialogView.axaml`, 6 bindings.
The message and the buttons of a message dialog. Shown in a window on the desktop and over the layout on a platform without windows.

- view model `MessageDialogViewModel` (`T:PressToShape.App.ViewModels.MessageDialogViewModel`), verified: x:DataType (src/PressToShape.App/Views/MessageDialogView.axaml:1); 0 commands, 6 test classes
- view model `MessageDialogViewModel` (`T:PressToShape.App.ViewModels.MessageDialogViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:46 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:46); 0 commands, 6 test classes

### MessageDialogWindow

`T:PressToShape.App.Views.MessageDialogWindow` at src/PressToShape.App/Views/MessageDialogWindow.axaml.cs:7. XAML: `src/PressToShape.App/Views/MessageDialogWindow.axaml`, 1 bindings.
_(no documentation)_

- view model `MessageDialogViewModel` (`T:PressToShape.App.ViewModels.MessageDialogViewModel`), verified: DataContext assignment (src/PressToShape.App/Services/AvaloniaMessageDialogs.cs:43); 0 commands, 6 test classes
- view model `MessageDialogViewModel` (`T:PressToShape.App.ViewModels.MessageDialogViewModel`), verified: x:DataType (src/PressToShape.App/Views/MessageDialogWindow.axaml:1); 0 commands, 6 test classes

### TabletColorSwatchView

`T:PressToShape.App.Views.Tablet.TabletColorSwatchView` at src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml.cs:6. XAML: `src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml`, 9 bindings.
The small fill and stroke swatch of the tablet tool column. All behavior is in PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel.

- view model `TabletColorSwatchViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`), verified: x:DataType (src/PressToShape.App/Views/Tablet/TabletColorSwatchView.axaml:1); 1 commands, 3 test classes
- view model `TabletColorSwatchViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletColorSwatchViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:82 (src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:82); 1 commands, 3 test classes

### TabletPanelView

`T:PressToShape.App.Views.Tablet.TabletPanelView` at src/PressToShape.App/Views/Tablet/TabletPanelView.axaml.cs:6. XAML: `src/PressToShape.App/Views/Tablet/TabletPanelView.axaml`, 3 bindings.
The tabbed panel of the tablet layout, with an icon for each tab. All state is in the inspector view model.

- view model `TabletPanelViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`), verified: x:DataType (src/PressToShape.App/Views/Tablet/TabletPanelView.axaml:1); 0 commands, 7 test classes
- view model `TabletPanelViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:29 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:29); 0 commands, 7 test classes
- view model `TabletPanelViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletPanelViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:271); 0 commands, 7 test classes

### TabletShellView

`T:PressToShape.App.Views.Tablet.TabletShellView` at src/PressToShape.App/Views/Tablet/TabletShellView.axaml.cs:19. XAML: `src/PressToShape.App/Views/Tablet/TabletShellView.axaml`, 7 bindings.
The root of the tablet layout. The code-behind has three view-only jobs that need the platform or the pointer: it hands the Back button to the view model (after closing an open toolbar menu, which only the view knows about), it closes the panel when the canvas is touched, and it keeps the layout above the on-screen keyboard. Avalonia already keeps the content out from under the system bars, and the keyboard does not resize the view but only reports the area it covers (found on the device), so the bottom margin is set here.

- view model `TabletShellViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel`), verified: DataContext assignment (src/PressToShape.App/AppLayout.cs:72); 0 commands, 5 test classes
- view model `TabletShellViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel`), verified: x:DataType (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:1); 0 commands, 5 test classes
- view model `TabletShellViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletShellViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:370); 0 commands, 5 test classes

### TabletToolColumnView

`T:PressToShape.App.Views.Tablet.TabletToolColumnView` at src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml.cs:5. XAML: `src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml`, 18 bindings.
_(no documentation)_

- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:24 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:24); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: x:DataType (src/PressToShape.App/Views/Tablet/TabletToolColumnView.axaml:1); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:588); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:604); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:620); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:636); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:649); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:667); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:688); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:706); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:725); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:765); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:779); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:797); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:813); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:828); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:858); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:883); 0 commands, 3 test classes
- view model `TabletToolColumnViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolColumnViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:902); 0 commands, 3 test classes

### TabletToolbarView

`T:PressToShape.App.Views.Tablet.TabletToolbarView` at src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml.cs:13. XAML: `src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml`, 41 bindings.
The compact toolbar of the tablet layout. All behavior is in PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel. The view only knows which of its two menus (the overflow menu and the zoom list) is open, so the Back button can close it first: a menu is not part of the view model.

- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext binding at src/PressToShape.App/Views/Tablet/TabletShellView.axaml:21 (src/PressToShape.App/Views/Tablet/TabletShellView.axaml:21); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: x:DataType (src/PressToShape.App/Views/Tablet/TabletToolbarView.axaml:1); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:72); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:85); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:108); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:124); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:138); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:157); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:171); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:192); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:216); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:233); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:252); 0 commands, 2 test classes
- view model `TabletToolbarViewModel` (`T:PressToShape.App.ViewModels.Tablet.TabletToolbarViewModel`), verified: DataContext assignment (tests/PressToShape.App.Tests/TabletViewsTests.cs:745); 0 commands, 2 test classes

## View models without a view

Types that derive from a base class of a linked view model but that no view links to. A view may still create them in code.

- `BareShell` (`T:PressToShape.App.Tests.EditorShellViewModelRingTests.BareShell`) at tests/PressToShape.App.Tests/EditorShellViewModelRingTests.cs:18
- `BareShell` (`T:PressToShape.App.Tests.EditorShellViewModelTests.BareShell`) at tests/PressToShape.App.Tests/EditorShellViewModelTests.cs:17
- `BareShell` (`T:PressToShape.App.Tests.RingMenuControlTests.BareShell`) at tests/PressToShape.App.Tests/RingMenuControlTests.cs:28
- `TestShell` (`T:PressToShape.App.Tests.TestShell`) at tests/PressToShape.App.Tests/TestShell.cs:10
- `MenuItemViewModel` (`T:PressToShape.App.ViewModels.Desktop.MenuItemViewModel`) at src/PressToShape.App/ViewModels/Desktop/MenuItemViewModel.cs:15
- `ToolbarButtonViewModel` (`T:PressToShape.App.ViewModels.Desktop.ToolbarButtonViewModel`) at src/PressToShape.App/ViewModels/Desktop/ToolbarButtonViewModel.cs:8
- `ColorFieldViewModel` (`T:PressToShape.App.ViewModels.Editor.ColorFieldViewModel`) at src/PressToShape.App/ViewModels/Editor/ColorFieldViewModel.cs:12
- `HistoryItemViewModel` (`T:PressToShape.App.ViewModels.Editor.HistoryItemViewModel`) at src/PressToShape.App/ViewModels/Editor/HistoryItemViewModel.cs:11
- `NumberFieldViewModel` (`T:PressToShape.App.ViewModels.Editor.NumberFieldViewModel`) at src/PressToShape.App/ViewModels/Editor/NumberFieldViewModel.cs:13
- `ObjectRowViewModel` (`T:PressToShape.App.ViewModels.Editor.ObjectRowViewModel`) at src/PressToShape.App/ViewModels/Editor/ObjectRowViewModel.cs:11
- `PaletteViewModel` (`T:PressToShape.App.ViewModels.Editor.PaletteViewModel`) at src/PressToShape.App/ViewModels/Editor/PaletteViewModel.cs:15
- `RingSlotViewModel` (`T:PressToShape.App.ViewModels.Editor.RingSlotViewModel`) at src/PressToShape.App/ViewModels/Editor/RingSlotViewModel.cs:16
- `ToolItemViewModel` (`T:PressToShape.App.ViewModels.Editor.ToolItemViewModel`) at src/PressToShape.App/ViewModels/Editor/ToolItemViewModel.cs:10
- `ZoomViewModel` (`T:PressToShape.App.ViewModels.Editor.ZoomViewModel`) at src/PressToShape.App/ViewModels/Editor/ZoomViewModel.cs:13
- `MessageDialogHost` (`T:PressToShape.App.ViewModels.Tablet.MessageDialogHost`) at src/PressToShape.App/ViewModels/Tablet/MessageDialogHost.cs:12
- `OverflowMenuViewModel` (`T:PressToShape.App.ViewModels.Tablet.OverflowMenuViewModel`) at src/PressToShape.App/ViewModels/Tablet/OverflowMenuViewModel.cs:17
- `PanelButtonViewModel` (`T:PressToShape.App.ViewModels.Tablet.PanelButtonViewModel`) at src/PressToShape.App/ViewModels/Tablet/PanelButtonViewModel.cs:12
- `ToolGroupViewModel` (`T:PressToShape.App.ViewModels.Tablet.ToolGroupViewModel`) at src/PressToShape.App/ViewModels/Tablet/ToolGroupViewModel.cs:19

## Views without a view model

none found
