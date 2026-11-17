using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesViewModelPathOperationsTests
{
    private static RectangleShape Rect(double x) => new() { X = x, Y = 0, Width = 100, Height = 100 };

    private static (EditorState State, PropertiesViewModel Properties) Create(PathMenuCommands? paths = null, Tool tool = Tool.Select, int rectangles = 3)
    {
        var state = new EditorState { ActiveTool = tool };
        for (var i = 0; i < rectangles; i++)
        {
            var rectangle = Rect(i * 50);
            state.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(state.ActiveLayerId, rectangle));
        }

        return (state, new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create(), paths: paths));
    }

    private static void SelectFirst(EditorState state, int count) =>
        state.SetSelection(state.Document.Layers[0].Shapes.Take(count).Select(shape => shape.Id));

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void ShowPathOperations_SelectTool_NeedsTwoOrMoreSelected(int selected, bool expected)
    {
        var (state, properties) = Create();

        SelectFirst(state, selected);

        Assert.Equal(expected, properties.ShowPathOperations);
    }

    [Theory]
    [InlineData((int)Tool.Select, true)]
    [InlineData((int)Tool.DirectSelect, true)]
    [InlineData((int)Tool.Pen, false)]
    [InlineData((int)Tool.Node, false)]
    [InlineData((int)Tool.Rectangle, false)]
    [InlineData((int)Tool.Ellipse, false)]
    [InlineData((int)Tool.Text, false)]
    [InlineData((int)Tool.Eyedropper, false)]
    [InlineData((int)Tool.Hand, false)]
    [InlineData((int)Tool.Zoom, false)]
    public void ShowPathOperations_TwoSelected_DependsOnTheTool(int tool, bool expected)
    {
        var (state, properties) = Create();
        SelectFirst(state, 2);

        state.ActiveTool = (Tool)tool;

        Assert.Equal(expected, properties.ShowPathOperations);
    }

    [Fact]
    public void ShowPathOperations_WhenTheSelectionCountChanges_RaisesPropertyChanged()
    {
        var (state, properties) = Create();
        var raised = new List<string?>();
        properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        SelectFirst(state, 2);

        Assert.Contains(nameof(PropertiesViewModel.ShowPathOperations), raised);
        Assert.True(properties.ShowPathOperations);

        raised.Clear();
        SelectFirst(state, 1);

        Assert.Contains(nameof(PropertiesViewModel.ShowPathOperations), raised);
        Assert.False(properties.ShowPathOperations);
    }

    [Fact]
    public void ShowPathOperations_WhenTheToolChanges_RaisesPropertyChanged()
    {
        var (state, properties) = Create();
        SelectFirst(state, 2);
        var raised = new List<string?>();
        properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        state.ActiveTool = Tool.Pen;

        Assert.Contains(nameof(PropertiesViewModel.ShowPathOperations), raised);
        Assert.False(properties.ShowPathOperations);
    }

    [Fact]
    public void IsPathOperationsExpanded_Default_IsTrue()
    {
        var (_, properties) = Create();

        Assert.True(properties.IsPathOperationsExpanded);
    }

    [Fact]
    public void PathOperations_WithoutCommands_CannotRun()
    {
        var (_, properties) = Create();

        var commands = properties.PathOperations;

        Assert.All(
            new[] { commands.Union, commands.Difference, commands.Intersection, commands.Exclusion, commands.Combine, commands.BreakApart },
            command => Assert.False(command.CanExecute(null)));
    }

    [Fact]
    public void PathOperations_WithCommands_AreTheSameInstances()
    {
        var run = new RelayCommand(() => { });
        var given = new PathMenuCommands(run, run, run, run, run, run);
        var (_, properties) = Create(given);

        Assert.Same(given, properties.PathOperations);
    }

    [Fact]
    public void Shell_PathOperationsAreTheShellCommands_AndOneRunIsOneUnionStep()
    {
        var shell = new TestShell();
        var a = shell.AddRectangle(0, 0, 100, 100);
        var b = shell.AddRectangle(50, 50, 100, 100);
        shell.State.SetSelection([a.Id, b.Id]);
        var union = shell.Inspector.Properties.PathOperations.Union;
        var steps = shell.State.History.Steps.Count;

        Assert.Same(shell.PathCommandsForTests.Union, union);
        Assert.True(shell.Inspector.Properties.ShowPathOperations);
        Assert.True(union.CanExecute(null));

        union.Execute(null);

        Assert.Equal(steps + 1, shell.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.Union, shell.State.History.Steps[^1]);
        Assert.IsType<PathShape>(Assert.Single(shell.State.Document.Layers[0].Shapes));
    }

    [Fact]
    public void Shell_WithAGroupSelected_NoPathOperationCanRun()
    {
        var shell = new TestShell();
        var a = shell.AddRectangle(0, 0, 100, 100);
        var b = shell.AddRectangle(50, 50, 100, 100);
        var c = shell.AddRectangle(200, 0, 100, 100);
        shell.State.SetSelection([a.Id, b.Id]);
        shell.Objects.Group();
        var group = shell.State.SelectedShapeIds.Single();
        shell.State.SetSelection([group, c.Id]);
        var commands = shell.Inspector.Properties.PathOperations;

        Assert.True(shell.Inspector.Properties.ShowPathOperations);
        Assert.False(commands.Union.CanExecute(null));
        Assert.False(commands.Difference.CanExecute(null));
        Assert.False(commands.Intersection.CanExecute(null));
        Assert.False(commands.Exclusion.CanExecute(null));
        Assert.False(commands.Combine.CanExecute(null));
    }
}
