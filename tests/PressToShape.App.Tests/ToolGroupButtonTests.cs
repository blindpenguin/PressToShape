using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Controls;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class ToolGroupButtonTests
{
    private sealed class Fixture
    {
        public Fixture()
        {
            Button = new ToolGroupButton
            {
                Width = 48,
                Height = 48,
                Command = new RelayCommand(() => Taps++),
                LongPressCommand = new RelayCommand(() => LongPresses++),
                Clock = () => Now,
            };
            Window = new Window { Width = 200, Height = 200, Content = new Border { Child = Button, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top } };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Center = Button.TranslatePoint(new Point(24, 24), Window)!.Value;
        }

        public ToolGroupButton Button { get; }

        public Window Window { get; }

        public Point Center { get; }

        public long Now { get; set; }

        public int Taps { get; private set; }

        public int LongPresses { get; private set; }

        public void Down() => Window.MouseDown(Center, MouseButton.Left);

        public void Up() => Window.MouseUp(Center, MouseButton.Left);
    }

    [Fact]
    public void ATap_RaisesTheClick_AndNoLongPress() => HeadlessUi.Run(() =>
    {
        var f = new Fixture();

        f.Down();
        f.Now = 100;
        f.Button.OnTimerTick();
        f.Up();

        Assert.Equal(1, f.Taps);
        Assert.Equal(0, f.LongPresses);
        f.Window.Close();
    });

    [Fact]
    public void AHold_RunsTheLongPressCommand_AndSwallowsTheClick() => HeadlessUi.Run(() =>
    {
        var f = new Fixture();

        f.Down();
        f.Now = LongPressTracker.DelayMs;
        f.Button.OnTimerTick();
        Assert.Equal(1, f.LongPresses);
        f.Up();

        Assert.Equal(0, f.Taps);
        Assert.False(f.Button.IsChecked);
        f.Window.Close();
    });

    [Fact]
    public void AHold_RunsTheLongPressCommandOnlyOnce() => HeadlessUi.Run(() =>
    {
        var f = new Fixture();

        f.Down();
        f.Now = 1000;
        f.Button.OnTimerTick();
        f.Now = 2000;
        f.Button.OnTimerTick();
        f.Up();

        Assert.Equal(1, f.LongPresses);
        f.Window.Close();
    });

    [Fact]
    public void ATapAfterAHold_SelectsAgain() => HeadlessUi.Run(() =>
    {
        var f = new Fixture();
        f.Down();
        f.Now = 1000;
        f.Button.OnTimerTick();
        f.Up();

        f.Now = 2000;
        f.Down();
        f.Up();

        Assert.Equal(1, f.Taps);
        f.Window.Close();
    });

    [Fact]
    public void MovingFarBeforeTheDelay_CancelsTheLongPress() => HeadlessUi.Run(() =>
    {
        var f = new Fixture();

        f.Down();
        f.Window.MouseMove(new Point(f.Center.X + LongPressTracker.Slop + 5, f.Center.Y));
        f.Now = 1000;
        f.Button.OnTimerTick();
        f.Up();

        Assert.Equal(0, f.LongPresses);
        f.Window.Close();
    });

    [Fact]
    public void ALongPressCommandThatCannotRun_IsNotRun() => HeadlessUi.Run(() =>
    {
        var f = new Fixture();
        var ran = 0;
        f.Button.LongPressCommand = new RelayCommand(() => ran++, () => false);

        f.Down();
        f.Now = 1000;
        f.Button.OnTimerTick();
        f.Up();

        Assert.Equal(0, ran);
        f.Window.Close();
    });
}
