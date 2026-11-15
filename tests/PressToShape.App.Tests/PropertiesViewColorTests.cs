using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;
using AutomationProperties = Avalonia.Automation.AutomationProperties;

namespace PressToShape.App.Tests;

// Only what bindings and view models cannot show: the real view's enabled states and the Shift state of a swatch click.
public class PropertiesViewColorTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);

    private static (Window Window, PropertiesView View, EditorState State, PropertiesViewModel Properties) Show(Document? document = null)
    {
        var state = new EditorState(document ?? Document.Create("L"));
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());
        var view = new PropertiesView { DataContext = properties };
        var window = new Window { Width = 400, Height = 900, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (window, view, state, properties);
    }

    private static T Named<T>(PropertiesView view, string name) where T : Control =>
        view.GetVisualDescendants().OfType<T>().First(control => control.Name == name);

    private static TextBox TextBoxNamed(PropertiesView view, string automationName) =>
        view.GetVisualDescendants().OfType<TextBox>().First(box => AutomationProperties.GetName(box) == automationName);

    [Fact]
    public void NothingSelected_ColorControlsAreEnabled_StrokeWidthAndOpacityAreNot() => HeadlessUi.Run(() =>
    {
        var (window, view, _, _) = Show();

        Assert.True(Named<Button>(view, "FillSwatchButton").IsEnabled);
        Assert.True(Named<Button>(view, "StrokeSwatchButton").IsEnabled);
        Assert.False(TextBoxNamed(view, "Stroke width").IsEnabled);
        Assert.False(Named<Slider>(view, "OpacitySlider").IsEnabled);

        window.Close();
    });

    [Fact]
    public void ShiftClickOnASwatch_AppliesItToTheStroke_AndAPlainClickToTheFill() => HeadlessUi.Run(() =>
    {
        var document = Document.Create("L").AddSwatch(Red);
        var (window, view, state, _) = Show(document);
        var shape = new RectangleShape { Width = 10, Height = 10 };
        state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, shape));
        state.SelectOnly(shape.Id);
        Dispatcher.UIThread.RunJobs();
        var swatch = Named<ItemsControl>(view, "PaletteStrip").GetVisualDescendants().OfType<Button>().Single();
        var center = swatch.TranslatePoint(new Point(swatch.Bounds.Width / 2, swatch.Bounds.Height / 2), window)!.Value;

        window.MouseDown(center, MouseButton.Left, RawInputModifiers.Shift);
        window.MouseUp(center, MouseButton.Left, RawInputModifiers.Shift);
        var afterShift = state.Document.FindShape(shape.Id)!.Shape.Appearance;
        Assert.Equal(Red, afterShift.Stroke);
        Assert.Equal(Appearance.Default.Fill, afterShift.Fill);

        window.MouseDown(center, MouseButton.Left);
        window.MouseUp(center, MouseButton.Left);
        Assert.Equal(Red, state.Document.FindShape(shape.Id)!.Shape.Appearance.Fill);

        window.Close();
    });

    // The picker has no template (and no size) unless the app includes the ColorPicker theme, which FluentTheme does not do.
    [Fact]
    public void TheApp_IncludesTheColorPickerTheme()
    {
        var xaml = System.IO.File.ReadAllText(RepoPaths.Combine("src/PressToShape.App/PressToShapeApp.axaml"));

        Assert.Contains("avares://Avalonia.Controls.ColorPicker/Themes/Fluent/Fluent.xaml", xaml, System.StringComparison.Ordinal);
    }

    [Fact]
    public void TheColorPicker_WithTheAppsStyles_HasASize() => HeadlessUi.Run(() =>
    {
        var picker = new ColorView { IsAlphaVisible = true, IsHexInputVisible = true };
        var window = new Window { Width = 400, Height = 900, Content = new StackPanel { Children = { picker } } };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.True(picker.Bounds.Height > 100, $"The ColorView is {picker.Bounds.Height} high.");

        window.Close();
    });
}
