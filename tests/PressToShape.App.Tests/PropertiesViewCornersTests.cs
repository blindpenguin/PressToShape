using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class PropertiesViewCornersTests
{
    private const string LinkedName = "Corner radius";

    private static readonly string[] FieldNames =
        [LinkedName, "Top left radius", "Top right radius", "Bottom right radius", "Bottom left radius"];

    private static (EditorState State, PropertiesViewModel Properties, PropertiesView View, Window Window, RectangleShape Rectangle) Show(
        Func<Shape, bool>? selectWhen = null, bool touch = false)
    {
        var rectangle = new RectangleShape { Id = Guid.NewGuid(), X = 100, Y = 50, Width = 200, Height = 100, Radii = new CornerRadii(5, 10, 15, 20) };
        var ellipse = new EllipseShape { Id = Guid.NewGuid(), X = 400, Y = 50, Width = 100, Height = 100 };
        var document = Document.Create("x");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, [rectangle, ellipse])) { ActiveTool = Tool.Select };
        state.Select((selectWhen?.Invoke(ellipse) ?? false) ? ellipse.Id : rectangle.Id);
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());
        var view = new PropertiesView { DataContext = properties };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 420, Height = 1400, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (state, properties, view, window, rectangle);
    }

    private static TextBox? Field(PropertiesView view, string name) =>
        view.GetVisualDescendants().OfType<TextBox>().FirstOrDefault(t => Avalonia.Automation.AutomationProperties.GetName(t) == name);

    private static ToggleButton? LinkToggle(PropertiesView view) =>
        view.GetVisualDescendants().OfType<ToggleButton>().FirstOrDefault(t => Avalonia.Automation.AutomationProperties.GetName(t) == "Link corners");

    [Fact]
    public void CornersSection_OneRectangleSelected_ShowsTheFiveFieldsAndTheToggle() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show();

        foreach (var name in FieldNames)
        {
            Assert.True(Field(view, name)!.IsEffectivelyVisible, name);
        }

        Assert.True(LinkToggle(view)!.IsEffectivelyVisible);
        window.Close();
    });

    [Fact]
    public void CornersSection_AnEllipseSelected_IsHidden() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show(selectWhen: _ => true);

        foreach (var name in FieldNames)
        {
            Assert.False(Field(view, name)?.IsEffectivelyVisible ?? false, name);
        }

        window.Close();
    });

    [Fact]
    public void CornersSection_Fields_ShowTheRadiiAndTheLinkedFieldIsEmptyWhenTheyDiffer() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show();

        Assert.Equal("5", Field(view, "Top left radius")!.Text);
        Assert.Equal("10", Field(view, "Top right radius")!.Text);
        Assert.Equal("15", Field(view, "Bottom right radius")!.Text);
        Assert.Equal("20", Field(view, "Bottom left radius")!.Text);
        Assert.Equal(string.Empty, Field(view, LinkedName)!.Text);
        window.Close();
    });

    [Fact]
    public void CornersSection_ViewModelChange_UpdatesTheText() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, rectangle) = Show();

        state.Edit(
            PressToShape.Core.History.HistoryStepType.ChangeCornerRadius,
            document => document.UpdateShape(rectangle.Id, _ => rectangle with { Radii = CornerRadii.Uniform(30) }));
        Dispatcher.UIThread.RunJobs();

        Assert.Equal("30", Field(view, LinkedName)!.Text);
        Assert.Equal("30", Field(view, "Bottom right radius")!.Text);
        window.Close();
    });

    [Fact]
    public void CornersSection_LinkToggle_IsBoundToTheSharedState() => HeadlessUi.Run(() =>
    {
        var (state, _, view, window, _) = Show();
        var toggle = LinkToggle(view)!;
        Assert.True(toggle.IsChecked);

        state.LinkCornerRadii = false;
        Dispatcher.UIThread.RunJobs();

        Assert.False(toggle.IsChecked);
        window.Close();
    });

    [Fact]
    public void CornersSection_OnTheTabletLayout_IsStillShown() => HeadlessUi.Run(() =>
    {
        var (_, _, view, window, _) = Show(touch: true);

        Assert.True(Field(view, LinkedName)!.IsEffectivelyVisible);
        Assert.True(LinkToggle(view)!.IsEffectivelyVisible);
        window.Close();
    });
}
