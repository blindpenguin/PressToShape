using System.Linq;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PressToShape.App.Controls;
using PressToShape.App.Views.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class PropertiesViewPathOperationsTests
{
    private static readonly string[] Names = ["Union", "Difference", "Intersection", "Exclusion", "Combine"];

    private static (TestShell Shell, PropertiesView View, Window Window) Show(bool touch = false)
    {
        var shell = new TestShell();
        var view = new PropertiesView { DataContext = shell.Inspector.Properties };
        var host = new Border { Child = view };
        if (touch)
        {
            host.Classes.Add("touch");
            TouchSizes.ApplyTo(host);
        }

        var window = new Window { Width = 420, Height = 1400, Content = host };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (shell, view, window);
    }

    private static void SelectTwoRectangles(TestShell shell)
    {
        var a = shell.AddRectangle(0, 0, 100, 100);
        var b = shell.AddRectangle(50, 50, 100, 100);
        shell.State.SetSelection([a.Id, b.Id]);
        Dispatcher.UIThread.RunJobs();
    }

    private static TextBlock? Header(PropertiesView view, string text) =>
        view.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Text == text && t.IsEffectivelyVisible);

    private static Button[] OperationButtons(PropertiesView view) =>
        view.GetVisualDescendants().OfType<Button>()
            .Where(b => Names.Contains(AutomationProperties.GetName(b)) && b.IsEffectivelyVisible)
            .OrderBy(b => b.TranslatePoint(new Point(0, 0), view)!.Value.X)
            .ToArray();

    [Fact]
    public void TheSection_WithTwoSelected_SitsAfterAlignAndDistributeAndBeforeQuickActions() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        SelectTwoRectangles(shell);

        var align = Header(view, "Align & Distribute");
        var operations = Header(view, "Path Operations");
        var quick = Header(view, "Quick Actions");

        Assert.NotNull(align);
        Assert.NotNull(operations);
        Assert.NotNull(quick);
        var y = (TextBlock t) => t.TranslatePoint(new Point(0, 0), view)!.Value.Y;
        Assert.True(y(align) < y(operations));
        Assert.True(y(operations) < y(quick));

        window.Close();
    });

    [Fact]
    public void TheSection_HasFiveButtonsInOrder_WithTooltipsThatMatchTheirNames() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        SelectTwoRectangles(shell);

        var buttons = OperationButtons(view);

        Assert.Equal(Names, buttons.Select(AutomationProperties.GetName));
        Assert.Equal(Names, buttons.Select(b => ToolTip.GetTip(b) as string));

        window.Close();
    });

    [Fact]
    public void TheSection_WithOneSelectedObject_IsHidden() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        var a = shell.AddRectangle(0, 0, 100, 100);
        shell.AddRectangle(50, 50, 100, 100);
        shell.State.SetSelection([a.Id]);
        Dispatcher.UIThread.RunJobs();

        Assert.Null(Header(view, "Path Operations"));
        Assert.Empty(OperationButtons(view));

        window.Close();
    });

    [Fact]
    public void TheSection_WithThePenTool_IsHidden_AndAppearsAgainWithSelect() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        SelectTwoRectangles(shell);

        shell.State.ActiveTool = Tool.Pen;
        Dispatcher.UIThread.RunJobs();
        Assert.Null(Header(view, "Path Operations"));

        shell.State.ActiveTool = Tool.DirectSelect;
        Dispatcher.UIThread.RunJobs();
        Assert.NotNull(Header(view, "Path Operations"));

        window.Close();
    });

    [Fact]
    public void TheButtons_WithTwoRectangles_AreEnabled_AndAUnionButtonRunsTheUnion() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        SelectTwoRectangles(shell);

        var buttons = OperationButtons(view);
        Assert.All(buttons, b => Assert.True(b.IsEffectivelyEnabled, AutomationProperties.GetName(b)));

        buttons[0].Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(HistoryStepType.Union, shell.State.History.Steps[^1]);
        Assert.IsType<PathShape>(Assert.Single(shell.State.Document.Layers[0].Shapes));

        window.Close();
    });

    [Fact]
    public void TheButtons_WithTwoOpenPaths_BooleansAreDisabledAndCombineIsEnabled() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        var first = PathShape.Create([new DocumentNode(0, 0), new DocumentNode(100, 100)]);
        var second = PathShape.Create([new DocumentNode(0, 100), new DocumentNode(100, 0)]);
        shell.State.Edit(HistoryStepType.DrawPath, d => d.AddShape(shell.State.ActiveLayerId, first));
        shell.State.Edit(HistoryStepType.DrawPath, d => d.AddShape(shell.State.ActiveLayerId, second));
        shell.State.SetSelection([first.Id, second.Id]);
        Dispatcher.UIThread.RunJobs();

        var buttons = OperationButtons(view).ToDictionary(b => AutomationProperties.GetName(b)!);

        Assert.NotNull(Header(view, "Path Operations"));
        foreach (var name in new[] { "Union", "Difference", "Intersection", "Exclusion" })
        {
            Assert.False(buttons[name].IsEffectivelyEnabled, name);
        }

        Assert.True(buttons["Combine"].IsEffectivelyEnabled);

        window.Close();
    });

    [Fact]
    public void TheButtons_WithTheTouchSizes_AreAtLeast48() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show(touch: true);
        SelectTwoRectangles(shell);

        var buttons = OperationButtons(view);

        Assert.Equal(5, buttons.Length);
        Assert.All(buttons, b => Assert.True(b.Bounds.Width >= 48 && b.Bounds.Height >= 48, $"{AutomationProperties.GetName(b)} {b.Bounds.Width:0}x{b.Bounds.Height:0}"));

        window.Close();
    });

    private static Button[] QuickActionButtons(PropertiesView view) =>
        view.GetVisualDescendants().OfType<Button>()
            .Where(b => AutomationProperties.GetName(b) is "Group" or "Ungroup" or "Mask" && b.IsEffectivelyVisible)
            .OrderBy(b => b.TranslatePoint(new Point(0, 0), view)!.Value.X)
            .ToArray();

    [Fact]
    public void QuickActions_AreIconButtonsWithTooltipsAndNames_InOrder() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        var a = shell.AddRectangle(0, 0, 100, 100);
        shell.State.SetSelection([a.Id]);
        Dispatcher.UIThread.RunJobs();

        var buttons = QuickActionButtons(view);

        Assert.Equal(["Group", "Ungroup", "Mask"], buttons.Select(AutomationProperties.GetName));
        Assert.Equal(["Group", "Ungroup", "Mask"], buttons.Select(b => ToolTip.GetTip(b) as string));
        Assert.All(buttons, b => Assert.Contains("icon", b.Classes));
        Assert.All(buttons, b => Assert.IsType<Material.Icons.Avalonia.MaterialIcon>(b.Content));

        window.Close();
    });

    [Fact]
    public void QuickActions_GroupFollowsTheSelection_AndRunsTheGroupCommand() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        var a = shell.AddRectangle(0, 0, 100, 100);
        var b = shell.AddRectangle(50, 50, 100, 100);
        shell.State.SetSelection([a.Id]);
        Dispatcher.UIThread.RunJobs();
        var group = QuickActionButtons(view).First(button => AutomationProperties.GetName(button) == "Group");
        Assert.False(group.IsEffectivelyEnabled);

        shell.State.SetSelection([a.Id, b.Id]);
        Dispatcher.UIThread.RunJobs();
        Assert.True(group.IsEffectivelyEnabled);
        group.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.IsType<GroupShape>(Assert.Single(shell.State.Document.Layers[0].Shapes));

        window.Close();
    });

    [Fact]
    public void QuickActions_WithTheTouchSizes_AreAtLeast48() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show(touch: true);
        var a = shell.AddRectangle(0, 0, 100, 100);
        shell.State.SetSelection([a.Id]);
        Dispatcher.UIThread.RunJobs();

        var buttons = QuickActionButtons(view);

        Assert.Equal(3, buttons.Length);
        Assert.All(buttons, b => Assert.True(b.Bounds.Width >= 48 && b.Bounds.Height >= 48, $"{AutomationProperties.GetName(b)} {b.Bounds.Width:0}x{b.Bounds.Height:0}"));

        window.Close();
    });
}
