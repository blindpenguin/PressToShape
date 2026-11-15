using System;
using System.Collections.Generic;
using PressToShape.App.ViewModels.Tablet;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class TabletColorSwatchViewModelTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);
    private static readonly Color Blue = Color.FromRgb(0, 0, 255);

    private static (TabletColorSwatchViewModel Swatch, TestShell Shell, TabletPanelViewModel Panel) Make(Dictionary<string, string>? german = null)
    {
        var shell = new TestShell(FakeLocalizer.Create(german: german));
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);
        return (new TabletColorSwatchViewModel(shell.Inspector.Properties, panel, shell.LocalizerForTests), shell, panel);
    }

    private static RectangleShape Rectangle(Color? fill, Color? stroke) =>
        new() { Width = 10, Height = 10, Appearance = Appearance.Default with { Fill = fill, Stroke = stroke } };

    private static void Select(TestShell shell, params RectangleShape[] shapes)
    {
        foreach (var shape in shapes)
        {
            var added = shape;
            shell.State.Edit(PressToShape.Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(shell.State.ActiveLayerId, added));
        }

        shell.State.SetSelection(Array.ConvertAll(shapes, s => s.Id));
    }

    [Fact]
    public void NothingSelected_ShowsTheColorsOfTheNextShape()
    {
        var (swatch, shell, _) = Make();
        shell.State.DefaultAppearance = Appearance.Default with { Fill = Red, Stroke = Green };

        Assert.True(swatch.HasFill);
        Assert.Equal(Red, swatch.FillColor);
        Assert.True(swatch.HasStroke);
        Assert.Equal(Green, swatch.StrokeColor);
    }

    [Fact]
    public void ASelectedShape_ShowsItsFillAndStroke()
    {
        var (swatch, shell, _) = Make();

        Select(shell, Rectangle(Red, Blue));

        Assert.Equal(Red, swatch.FillColor);
        Assert.Equal(Blue, swatch.StrokeColor);
        Assert.True(swatch.HasFill && swatch.HasStroke);
    }

    [Fact]
    public void AShapeWithoutFillOrStroke_HasNone()
    {
        var (swatch, shell, _) = Make();

        Select(shell, Rectangle(null, null));

        Assert.False(swatch.HasFill);
        Assert.False(swatch.HasStroke);
    }

    [Fact]
    public void ShapesWithDifferentFills_ShowNoFill_ButTheSameStrokeStays()
    {
        var (swatch, shell, _) = Make();

        Select(shell, Rectangle(Red, Blue), Rectangle(Green, Blue));

        Assert.False(swatch.HasFill);
        Assert.True(swatch.HasStroke);
        Assert.Equal(Blue, swatch.StrokeColor);
    }

    [Fact]
    public void AColorChangedInThePanel_IsShownAndNotified()
    {
        var (swatch, shell, _) = Make();
        Select(shell, Rectangle(Red, Blue));
        var raised = new List<string?>();
        swatch.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.Inspector.Properties.Fill.SetColor(Green);

        Assert.Equal(Green, swatch.FillColor);
        Assert.Contains(nameof(TabletColorSwatchViewModel.FillColor), raised);
    }

    [Fact]
    public void Undo_ShowsTheOldColorAgain()
    {
        var (swatch, shell, _) = Make();
        Select(shell, Rectangle(Red, Blue));
        shell.Inspector.Properties.Fill.SetColor(Green);

        shell.UndoCommand.Execute(null);

        Assert.Equal(Red, swatch.FillColor);
    }

    [Fact]
    public void ANewSelection_ShowsItsColors_AndNotifiesTheFlags()
    {
        var (swatch, shell, _) = Make();
        Select(shell, Rectangle(Red, Blue));
        var raised = new List<string?>();
        swatch.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        shell.State.ClearSelection();
        shell.State.DefaultAppearance = Appearance.Default with { Fill = null, Stroke = Green };

        Assert.False(swatch.HasFill);
        Assert.Equal(Green, swatch.StrokeColor);
        Assert.Contains(nameof(TabletColorSwatchViewModel.HasFill), raised);
    }

    [Fact]
    public void Open_FromClosed_OpensTheProperties()
    {
        var (swatch, shell, panel) = Make();

        swatch.OpenCommand.Execute(null);

        Assert.True(panel.IsOpen);
        Assert.Same(shell.Inspector.Properties, panel.Content);
    }

    [Fact]
    public void Open_WithAnotherTabOpen_SwitchesToTheProperties()
    {
        var (swatch, shell, panel) = Make();
        panel.Toggle(InspectorTab.History);

        swatch.OpenCommand.Execute(null);

        Assert.Same(shell.Inspector.Properties, panel.Content);
    }

    [Fact]
    public void Open_WhenAlreadyOnTheProperties_KeepsThePanelOpen()
    {
        var (swatch, _, panel) = Make();
        panel.Toggle(InspectorTab.Properties);

        swatch.OpenCommand.Execute(null);

        Assert.True(panel.IsOpen);
        Assert.Equal(InspectorTab.Properties, panel.OpenTab);
    }

    [Fact]
    public void Open_ExpandsACollapsedAppearanceSection()
    {
        var (swatch, shell, _) = Make();
        shell.Inspector.Properties.IsAppearanceExpanded = false;

        swatch.OpenCommand.Execute(null);

        Assert.True(shell.Inspector.Properties.IsAppearanceExpanded);
    }

    [Fact]
    public void Name_FollowsTheLanguage()
    {
        var (swatch, shell, _) = Make(new Dictionary<string, string> { ["Fill and stroke color"] = "Füll- und Konturfarbe" });
        Assert.Equal("Fill and stroke color", swatch.Name);

        shell.LocalizerForTests.SetLanguage("de");

        Assert.Equal("Füll- und Konturfarbe", swatch.Name);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var shell = new TestShell();
        var panel = new TabletPanelViewModel(shell.Inspector, shell.LocalizerForTests);

        Assert.Throws<ArgumentNullException>(() => new TabletColorSwatchViewModel(null!, panel, shell.LocalizerForTests));
        Assert.Throws<ArgumentNullException>(() => new TabletColorSwatchViewModel(shell.Inspector.Properties, null!, shell.LocalizerForTests));
        Assert.Throws<ArgumentNullException>(() => new TabletColorSwatchViewModel(shell.Inspector.Properties, panel, null!));
    }
}
