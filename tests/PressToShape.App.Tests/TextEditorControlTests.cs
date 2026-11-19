using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PressToShape.App.Controls;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

[Collection(HeadlessUi.CollectionName)]
public class TextEditorControlTests
{
    private sealed class CountingCommand : ICommand
    {
        public int Count { get; private set; }

        public event System.EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => Count++;
    }

    private static ScreenShape Shape(TextShape? text = null, double zoom = 100) =>
        CanvasScene.BuildShape(text ?? new TextShape { X = 20, Y = 30, Width = 100, Height = 40, Text = "Hello", FontSize = 20 }, zoom, 0, 0, new FakeTextMeasurer());

    // The control sits in a panel in a window, like in the canvas view, with a second control that can take the focus.
    private sealed record Setup(Window Window, TextEditorControl Editor, Button Other, CountingCommand Commit);

    private static Setup Show(ScreenShape? shape = null)
    {
        var commit = new CountingCommand();
        var editor = new TextEditorControl { Shape = shape, CommitCommand = commit };
        var other = new Button { Content = "other", Focusable = true };
        var window = new Window { Content = new StackPanel { Children = { new Panel { Width = 400, Height = 300, Children = { editor } }, other } }, Width = 500, Height = 500 };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        return new Setup(window, editor, other, commit);
    }

    private static KeyEventArgs RaiseKey(Control target, Key key, KeyModifiers modifiers = KeyModifiers.None)
    {
        var args = new KeyEventArgs { RoutedEvent = InputElement.KeyDownEvent, Key = key, KeyModifiers = modifiers, Source = target };
        target.RaiseEvent(args);
        return args;
    }

    [Fact]
    public void Shape_Null_ShowsNothing() => HeadlessUi.Run(() =>
    {
        var s = Show();

        Assert.False(((Control)s.Editor.Children[0]).IsVisible);
        s.Window.Close();
    });

    [Fact]
    public void Shape_Set_ShowsTheBoxAtTheTextWithItsFontAndContent() => HeadlessUi.Run(() =>
    {
        var s = Show();
        s.Editor.EditedText = "Hello";

        s.Editor.Shape = Shape(new TextShape { X = 20, Y = 30, Width = 100, Height = 40, Text = "Hello", FontSize = 20, IsBold = true, IsItalic = true, LetterSpacing = 2, Alignment = TextAlignType.Center });

        var frame = (Border)s.Editor.Children[0];
        Assert.True(frame.IsVisible);
        Assert.Equal(19, Canvas.GetLeft(frame));
        Assert.Equal(29, Canvas.GetTop(frame));
        Assert.Equal("Hello", s.Editor.Box.Text);
        Assert.Equal(20, s.Editor.Box.FontSize);
        Assert.Equal(Avalonia.Media.FontWeight.Bold, s.Editor.Box.FontWeight);
        Assert.Equal(Avalonia.Media.FontStyle.Italic, s.Editor.Box.FontStyle);
        Assert.Equal(2, s.Editor.Box.LetterSpacing);
        Assert.Equal(Avalonia.Media.TextAlignment.Center, s.Editor.Box.TextAlignment);
        Assert.Equal(Avalonia.Media.TextWrapping.NoWrap, s.Editor.Box.TextWrapping);
        s.Window.Close();
    });

    [Fact]
    public void Shape_AreaText_WrapsAtTheBoxWidthAndIsAtLeastAsHighAsTheBox() => HeadlessUi.Run(() =>
    {
        var s = Show();

        s.Editor.Shape = Shape(new TextShape { X = 20, Y = 30, Width = 100, Height = 40, Text = "a b c", FontSize = 20, IsArea = true });

        var frame = (Border)s.Editor.Children[0];
        Assert.Equal(Avalonia.Media.TextWrapping.Wrap, s.Editor.Box.TextWrapping);
        Assert.Equal(102, frame.Width);
        Assert.Equal(42, frame.MinHeight);
        s.Window.Close();
    });

    [Fact]
    public void Shape_AtZoomAndRotation_ScalesTheFontAndTurnsTheBox() => HeadlessUi.Run(() =>
    {
        var s = Show();

        s.Editor.Shape = Shape(new TextShape { X = 20, Y = 30, Width = 100, Height = 40, Text = "Hello", FontSize = 20, Rotation = 30 }, zoom: 200);

        var frame = (Border)s.Editor.Children[0];
        Assert.Equal(40, s.Editor.Box.FontSize);
        var turn = Assert.IsType<Avalonia.Media.RotateTransform>(frame.RenderTransform);
        Assert.Equal(30, turn.Angle);
        s.Window.Close();
    });

    [Fact]
    public void Typing_WritesTheContentBackToEditedText() => HeadlessUi.Run(() =>
    {
        var s = Show(Shape());

        s.Editor.Box.Text = "Typed";

        Assert.Equal("Typed", s.Editor.EditedText);
        s.Window.Close();
    });

    [Fact]
    public void EditedText_ChangedFromOutside_UpdatesTheBoxWithoutEchoing() => HeadlessUi.Run(() =>
    {
        var s = Show(Shape());
        List<string> written = [];
        s.Editor.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextEditorControl.EditedTextProperty)
            {
                written.Add((string)e.NewValue!);
            }
        };

        s.Editor.EditedText = "From the view model";

        Assert.Equal("From the view model", s.Editor.Box.Text);
        Assert.Single(written);
        s.Window.Close();
    });

    [Fact]
    public void Shape_StartsEditing_TheBoxGetsTheFocusWithTheCaretAtTheEnd() => HeadlessUi.Run(() =>
    {
        var s = Show();
        s.Editor.EditedText = "Hello";

        s.Editor.Shape = Shape();
        Dispatcher.UIThread.RunJobs();

        Assert.True(s.Editor.Box.IsFocused);
        Assert.Equal(5, s.Editor.Box.CaretIndex);
        s.Window.Close();
    });

    [Fact]
    public void Escape_RunsTheCommitCommandAndStopsTheKey() => HeadlessUi.Run(() =>
    {
        var s = Show(Shape());

        var args = RaiseKey(s.Editor.Box, Key.Escape);

        Assert.Equal(1, s.Commit.Count);
        Assert.True(args.Handled);
        s.Window.Close();
    });

    [Theory]
    [InlineData(Key.V)]
    [InlineData(Key.P)]
    public void OtherKeys_AreNotMarkedAsHandled_SoTheTextInputStillComes(Key key) => HeadlessUi.Run(() =>
    {
        var s = Show(Shape());

        var args = RaiseKey(s.Editor.Box, key);

        Assert.False(args.Handled);
        Assert.Equal(0, s.Commit.Count);
        s.Window.Close();
    });

    [Fact]
    public void LosingTheFocusToAnotherControl_RunsTheCommitCommand() => HeadlessUi.Run(() =>
    {
        var s = Show();
        s.Editor.Shape = Shape();
        Dispatcher.UIThread.RunJobs();

        s.Other.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(1, s.Commit.Count);
        s.Window.Close();
    });

    [Fact]
    public void LosingTheFocusWhileNotEditing_DoesNotRunTheCommitCommand() => HeadlessUi.Run(() =>
    {
        var s = Show();

        s.Other.Focus();
        Dispatcher.UIThread.RunJobs();

        Assert.Equal(0, s.Commit.Count);
        s.Window.Close();
    });

    [Fact]
    public void LineHeight_IsGivenToTheBox() => HeadlessUi.Run(() =>
    {
        var s = Show(Shape());

        s.Editor.LineHeight = 33;

        Assert.Equal(33, s.Editor.Box.LineHeight);
        s.Window.Close();
    });

    [Fact]
    public void Shape_SetBackToNull_HidesTheBoxAgain() => HeadlessUi.Run(() =>
    {
        var s = Show(Shape());

        s.Editor.Shape = null;

        Assert.False(((Control)s.Editor.Children[0]).IsVisible);
        s.Window.Close();
    });
}
