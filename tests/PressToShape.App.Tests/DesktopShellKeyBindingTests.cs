using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Desktop;
using PressToShape.App.Views.Desktop;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

// The window-level shortcut of the desktop shell for Convert Object to Path. The menu item only shows the shortcut text.
[Collection(HeadlessUi.CollectionName)]
public class DesktopShellKeyBindingTests
{
    private sealed class NoShutdown : IAppShutdown
    {
        public void Shutdown()
        {
        }
    }

    private static (DesktopShellViewModel Shell, DesktopShellView View, Window Window) Show()
    {
        var shell = new DesktopShellViewModel(new NoShutdown(), FakeLocalizer.Create(), TestFileServices.Create());
        var view = new DesktopShellView { DataContext = shell };
        var window = new Window { Width = 1280, Height = 800, Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return (shell, view, window);
    }

    [Fact]
    public void TheShell_HasAKeyBindingForCtrlShiftO_ThatIsTheConvertCommand() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();

        var binding = view.KeyBindings.Single(b => b.Gesture.Equals(KeyGesture.Parse("Ctrl+Shift+O")));

        Assert.Same(shell.ConvertShapesToPathsCommand, binding.Command);

        window.Close();
    });

    [Fact]
    public void CtrlShiftO_WithARectangleSelected_ConvertsItToAPathAsOneStep() => HeadlessUi.Run(() =>
    {
        var (shell, view, window) = Show();
        var rectangle = new RectangleShape { Width = 100, Height = 50 };
        shell.State.Edit(HistoryStepType.DrawRectangle, d => d.AddShape(shell.State.ActiveLayerId, rectangle));
        shell.State.SetSelection([rectangle.Id]);
        view.Focus();
        Dispatcher.UIThread.RunJobs();
        var steps = shell.State.History.Steps.Count;

        window.KeyPress(Key.O, RawInputModifiers.Control | RawInputModifiers.Shift, PhysicalKey.O, "O");
        Dispatcher.UIThread.RunJobs();

        Assert.IsType<PathShape>(Assert.Single(shell.State.Document.Layers[0].Shapes));
        Assert.Equal(steps + 1, shell.State.History.Steps.Count);
        Assert.Equal(HistoryStepType.ConvertToPath, shell.State.History.Steps[^1]);

        window.Close();
    });
}
