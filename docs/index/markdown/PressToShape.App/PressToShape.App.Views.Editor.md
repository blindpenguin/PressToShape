# PressToShape.App.Views.Editor

Project: PressToShape.App

## CanvasAreaView (class)

`public partial class CanvasAreaView : UserControl`

Source: `src/PressToShape.App/Views/Editor/CanvasAreaView.axaml.cs:12-413`

_(no documentation)_

**Members**

- `internal Panel Viewport` _(generated)_ - _(no documentation)_
- `internal RingMenuControl RingMenu` _(generated)_ - _(no documentation)_
- `public StyledProperty<bool> ShowRulersProperty` - Whether the rulers (and the corner between them) are shown. The desktop layout shows them, the tablet layout does not.
- `internal ScrollBar VerticalBar` _(generated)_ - _(no documentation)_
- `internal ScrollBar HorizontalBar` _(generated)_ - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
- `public CanvasAreaView()` - _(no documentation)_
- `public bool ShowRulers { get; set; }` - _(no documentation)_
- `protected void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)` - _(no documentation)_
- `protected void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)` - _(no documentation)_
- `protected void OnDataContextChanged(EventArgs e)` - _(no documentation)_

## CanvasCursors (class)

`internal static class CanvasCursors`

Source: `src/PressToShape.App/Views/Editor/CanvasCursors.cs:17-78`

The pointer shapes of the canvas. Avalonia has no standard rotate, open hand, closed hand or magnifier, so the cursors come from pre-drawn images (Assets/Cursors, made from Lucide icons by tools/PressToShape.CursorImages) in the size that fits the screen scale. The plain arrow stays the system cursor. If an image cannot be loaded or the platform rejects it, a standard cursor is used instead.

**Members**

- `public Cursor For(CanvasCursorType type, double scale)` - The cursor for a type on a screen with this scale (1.0 is 100%). Created on first use and then reused.

## CoreColorBrushConverter (class)

`internal sealed class CoreColorBrushConverter : IValueConverter`

Source: `src/PressToShape.App/Views/Editor/CoreColorConverter.cs:28-37`

Turns the Core color of a shape into a brush, for swatches. It does not convert back.

**Members**

- `public CoreColorBrushConverter Instance { get; }` - _(no documentation)_
- `public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_
- `public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_

## CoreColorConverter (class)

`internal sealed class CoreColorConverter : IValueConverter`

Source: `src/PressToShape.App/Views/Editor/CoreColorConverter.cs:12-25`

Turns the Core color of a shape into an Avalonia color and back. Alpha is kept.

**Members**

- `public CoreColorConverter Instance { get; }` - _(no documentation)_
- `public Color ToAvalonia(Color color)` - _(no documentation)_
- `public Color ToCore(Color color)` - _(no documentation)_
- `public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_
- `public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_

## HistoryView (class)

`public partial class HistoryView : UserControl`

Source: `src/PressToShape.App/Views/Editor/HistoryView.axaml.cs:5-20`

_(no documentation)_

**Members**

- `public HistoryView()` - _(no documentation)_
- `internal ListBox StepList` _(generated)_ - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## InspectorView (class)

`public partial class InspectorView : UserControl`

Source: `src/PressToShape.App/Views/Editor/InspectorView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public InspectorView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## LayersView (class)

`public partial class LayersView : UserControl`

Source: `src/PressToShape.App/Views/Editor/LayersView.axaml.cs:16-208`

The Layers tab. The view only turns pointer input into calls of the view model: a press selects, a double-tap renames, and a drag (after a long press for a finger) drops an object above another one of the same group or onto a layer, or a layer above another layer.

**Members**

- `internal ItemsControl RowList` _(generated)_ - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
- `public LayersView()` - _(no documentation)_
- `protected void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)` - _(no documentation)_

## ObjectIndentConverter (class)

`internal sealed class ObjectIndentConverter : IValueConverter`

Source: `src/PressToShape.App/Views/Editor/ObjectIndentConverter.cs:8-18`

Turns the depth of a row of the object list into the width of the empty space in front of it. The view sets PressToShape.App.Views.Editor.ObjectIndentConverter.Indent from the theme.

**Members**

- `public double Indent { get; set; }` - The width for one level, in device-independent pixels.
- `public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_
- `public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)` - _(no documentation)_

## PlaceholderPanelView (class)

`public partial class PlaceholderPanelView : UserControl`

Source: `src/PressToShape.App/Views/Editor/PlaceholderPanelView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public PlaceholderPanelView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## PropertiesView (class)

`public partial class PropertiesView : UserControl`

Source: `src/PressToShape.App/Views/Editor/PropertiesView.axaml.cs:15-143`

The Properties tab. The code-behind only forwards: the number fields (shared with the contextual bar of the tablet layout), the pickers' open and close, the pick, add-to-palette and recent-color buttons of the pickers, the palette swatches (with the Shift state), and the opacity slider's drag. Every decision is in PressToShape.App.ViewModels.Editor.PropertiesViewModel.

**Members**

- `internal Button FillSwatchButton` _(generated)_ - _(no documentation)_
- `internal ColorView FillColorView` _(generated)_ - _(no documentation)_
- `internal Button StrokeSwatchButton` _(generated)_ - _(no documentation)_
- `internal ColorView StrokeColorView` _(generated)_ - _(no documentation)_
- `internal ItemsControl PaletteStrip` _(generated)_ - _(no documentation)_
- `public PropertiesView()` - _(no documentation)_
- `internal Slider OpacitySlider` _(generated)_ - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## SwatchesView (class)

`public partial class SwatchesView : UserControl`

Source: `src/PressToShape.App/Views/Editor/SwatchesView.axaml.cs:6-12`

The fill and stroke swatches with the swap and reset buttons. Used by the desktop tool column and the tablet tool column.

**Members**

- `public SwatchesView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).

## ToolboxView (class)

`public partial class ToolboxView : UserControl`

Source: `src/PressToShape.App/Views/Editor/ToolboxView.axaml.cs:5-11`

_(no documentation)_

**Members**

- `public ToolboxView()` - _(no documentation)_
- `public void InitializeComponent(bool loadXaml = true)` _(generated)_ - Wires up the controls and optionally loads XAML markup and attaches dev tools (if Avalonia.Diagnostics package is referenced).
