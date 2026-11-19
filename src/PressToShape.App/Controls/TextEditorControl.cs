using System.Windows.Input;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using PressToShape.App.Services;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;

namespace PressToShape.App.Controls;

/// <summary>
/// The editor of a text on the canvas: a borderless <see cref="TextBox"/> at the place of the text, with its font and its rotation, so the
/// user types where the text is. It brings the caret, the selection, the clipboard, the input method and the on-screen keyboard. The control
/// only shows what the view model gives it and reports the content and the end of the editing: it decides nothing about the document.
/// </summary>
internal sealed class TextEditorControl : Canvas
{
    /// <summary>The text that is edited, in screen space with its font. <c>null</c> when nothing is edited: the control is then empty.</summary>
    public static readonly StyledProperty<ScreenShape?> ShapeProperty =
        AvaloniaProperty.Register<TextEditorControl, ScreenShape?>(nameof(Shape));

    /// <summary>The content of the text. The user's typing is written back to it.</summary>
    public static readonly StyledProperty<string> EditedTextProperty =
        AvaloniaProperty.Register<TextEditorControl, string>(nameof(EditedText), string.Empty, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>The height of a line in screen pixels. <c>NaN</c> leaves it to the font.</summary>
    public static readonly StyledProperty<double> LineHeightProperty =
        AvaloniaProperty.Register<TextEditorControl, double>(nameof(LineHeight), double.NaN);

    /// <summary>Ends the editing. Run on Escape and when the editor loses the focus to another control.</summary>
    public static readonly StyledProperty<ICommand?> CommitCommandProperty =
        AvaloniaProperty.Register<TextEditorControl, ICommand?>(nameof(CommitCommand));

    /// <summary>The brush of the text when the text has no fill. It comes from the theme.</summary>
    public static readonly StyledProperty<IBrush?> FallbackForegroundProperty =
        AvaloniaProperty.Register<TextEditorControl, IBrush?>(nameof(FallbackForeground));

    private readonly TextBox _box;
    private readonly Border _frame;
    private bool _writingBox;
    private bool _focusPending;

    public TextEditorControl()
    {
        _box = new TextBox
        {
            Classes = { "textEditor" },
            AcceptsReturn = true,
        };
        _frame = new Border { Classes = { "textEditorFrame" }, Child = _box, IsVisible = false };
        Children.Add(_frame);

        // The property, not the TextChanged event: the event comes later, and the box must follow every keystroke.
        _box.PropertyChanged += OnBoxPropertyChanged;
        _box.LostFocus += OnBoxLostFocus;

        // Escape ends the editing. Every other key is left alone: a key that is marked as handled gets no text input on some platforms, and the
        // shell already keeps its tool and edit shortcuts to itself while a text box has the focus.
        AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
    }

    public ScreenShape? Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }

    public string EditedText
    {
        get => GetValue(EditedTextProperty);
        set => SetValue(EditedTextProperty, value);
    }

    public double LineHeight
    {
        get => GetValue(LineHeightProperty);
        set => SetValue(LineHeightProperty, value);
    }

    public ICommand? CommitCommand
    {
        get => GetValue(CommitCommandProperty);
        set => SetValue(CommitCommandProperty, value);
    }

    public IBrush? FallbackForeground
    {
        get => GetValue(FallbackForegroundProperty);
        set => SetValue(FallbackForegroundProperty, value);
    }

    /// <summary>The box that is typed in. For tests.</summary>
    internal TextBox Box => _box;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShapeProperty)
        {
            var wasEditing = change.OldValue is ScreenShape;
            UpdateBox();
            if (Shape is null)
            {
                _focusPending = false;
                _frame.LayoutUpdated -= OnFrameLayoutUpdated;
            }

            if (!wasEditing && Shape is not null)
            {
                // The press that started the editing still has the focus on the canvas, and the box cannot take it before it has been laid out.
                // The editor takes it in the first layout pass after that, and again from the dispatcher in case no pass comes.
                if (!_focusPending)
                {
                    _focusPending = true;
                    _frame.LayoutUpdated += OnFrameLayoutUpdated;
                }

                Dispatcher.UIThread.Post(FocusBox, DispatcherPriority.Input);
            }
        }
        else if (change.Property == EditedTextProperty)
        {
            WriteBox(EditedText);
        }
        else if (change.Property == LineHeightProperty)
        {
            _box.LineHeight = LineHeight;
        }
        else if (change.Property == AutomationProperties.NameProperty)
        {
            // The name is translated in the XAML, the box is what a screen reader meets.
            AutomationProperties.SetName(_box, AutomationProperties.GetName(this));
        }
        else if (change.Property == FallbackForegroundProperty)
        {
            UpdateBox();
        }
    }

    private void UpdateBox()
    {
        if (Shape is not { Text: { } text } shape)
        {
            _frame.IsVisible = false;
            return;
        }

        _frame.IsVisible = true;
        _box.Foreground = shape.Fill is { } fill
            ? new ImmutableSolidColorBrush(Avalonia.Media.Color.FromArgb(fill.A, fill.R, fill.G, fill.B))
            : FallbackForeground;
        _box.FontFamily = TextFonts.TypefaceFor(text.FontFamily, text.IsBold, text.IsItalic).FontFamily;
        _box.FontSize = text.FontSize;
        _box.FontWeight = text.IsBold ? FontWeight.Bold : FontWeight.Normal;
        _box.FontStyle = text.IsItalic ? FontStyle.Italic : FontStyle.Normal;
        _box.LetterSpacing = text.LetterSpacing;
        _box.LineHeight = LineHeight;
        _box.TextAlignment = text.Alignment switch
        {
            TextAlignType.Center => TextAlignment.Center,
            TextAlignType.Right => TextAlignment.Right,
            _ => TextAlignment.Left,
        };

        // An area text wraps at its box and is at least as high as it. A point text grows with what is typed.
        var border = _frame.BorderThickness.Left;
        _box.TextWrapping = text.IsArea ? TextWrapping.Wrap : TextWrapping.NoWrap;
        _box.MinWidth = text.IsArea ? 0 : text.FontSize;
        _frame.Width = text.IsArea ? shape.Width + 2 * border : double.NaN;
        _frame.MinHeight = text.IsArea ? shape.Height + 2 * border : 0;
        SetLeft(_frame, shape.X - border);
        SetTop(_frame, shape.Y - border);
        _frame.RenderTransformOrigin = RelativePoint.Center;
        _frame.RenderTransform = shape.RotationDegrees == 0 ? null : new RotateTransform(shape.RotationDegrees);
        WriteBox(EditedText);
    }

    private void WriteBox(string text)
    {
        if (_box.Text == text)
        {
            return;
        }

        _writingBox = true;
        try
        {
            _box.Text = text;
        }
        finally
        {
            _writingBox = false;
        }
    }

    private void OnBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TextBox.TextProperty && !_writingBox)
        {
            EditedText = _box.Text ?? string.Empty;
        }
    }

    private void OnFrameLayoutUpdated(object? sender, System.EventArgs e)
    {
        if (_frame.IsEffectivelyVisible && _frame.Bounds.Width > 0)
        {
            FocusBox();
        }
    }

    private void FocusBox()
    {
        if (Shape is null || !_focusPending)
        {
            return;
        }

        if (_box.Focus())
        {
            _focusPending = false;
            _frame.LayoutUpdated -= OnFrameLayoutUpdated;
            _box.CaretIndex = _box.Text?.Length ?? 0;
        }
    }

    // Moving the focus to another control of the window ends the editing. A window that is switched away from leaves the focus on nothing,
    // and the editing goes on.
    private void OnBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(
            () =>
            {
                if (Shape is not null && !_box.IsKeyboardFocusWithin && TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is not null)
                {
                    CommitCommand?.Execute(null);
                }
            },
            DispatcherPriority.Input);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (Shape is not null && e.Key == Key.Escape)
        {
            CommitCommand?.Execute(null);
            e.Handled = true;
        }
    }
}
