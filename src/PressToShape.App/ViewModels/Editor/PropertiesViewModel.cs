using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PressToShape.App.Localization;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using static PressToShape.App.Localization.Localizer;

namespace PressToShape.App.ViewModels.Editor;

/// <summary>
/// The Properties tab: the position, size, rotation and appearance of the selected shapes, and the section expansion.
/// The fields show the current values (updated in place while shapes are dragged or edited elsewhere) and every committed
/// edit is one undo step.
/// </summary>
internal sealed partial class PropertiesViewModel : LocalizedViewModel
{
    private const double MaxCoordinate = 10_000_000;
    private const double MaxStrokeWidth = 1000;

    private readonly EditorState _state;
    private readonly SelectionEditor _editor;

    // While a color or the opacity slider is previewed, the preview is built on this document so every update starts from the same shapes.
    private Document? _previewBase;
    private bool _opacityDragging;

    public PropertiesViewModel(EditorState state, SelectionEditor editor, ILocalizer localizer)
        : base(localizer)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _editor = editor ?? throw new ArgumentNullException(nameof(editor));

        X = new NumberFieldViewModel(localizer, -MaxCoordinate, MaxCoordinate, value => Transform(HistoryStepType.ChangePosition, (shapes, frame) => SelectionTransforms.SetPosition(shapes, frame, value, null)));
        Y = new NumberFieldViewModel(localizer, -MaxCoordinate, MaxCoordinate, value => Transform(HistoryStepType.ChangePosition, (shapes, frame) => SelectionTransforms.SetPosition(shapes, frame, null, value)));
        Width = new NumberFieldViewModel(localizer, 0, MaxCoordinate, value => Transform(HistoryStepType.ChangeSize, (shapes, frame) => SelectionTransforms.SetSize(shapes, frame, value, null, KeepProportions)));
        Height = new NumberFieldViewModel(localizer, 0, MaxCoordinate, value => Transform(HistoryStepType.ChangeSize, (shapes, frame) => SelectionTransforms.SetSize(shapes, frame, null, value, KeepProportions)));
        Rotation = new NumberFieldViewModel(localizer, -MaxCoordinate, MaxCoordinate, value => Transform(HistoryStepType.ChangeRotation, (shapes, frame) => SelectionTransforms.SetRotation(shapes, frame, value)));
        StrokeWidth = new NumberFieldViewModel(localizer, 0, MaxStrokeWidth, value => _editor.Apply(HistoryStepType.ChangeStrokeWidth, shape => shape with { Appearance = shape.Appearance with { StrokeWidth = value } }));
        OpacityPercent = new NumberFieldViewModel(localizer, 0, 100, value => _editor.Apply(HistoryStepType.ChangeOpacity, shape => shape with { Appearance = shape.Appearance with { Opacity = value / 100 } }));

        Fill = new ColorFieldViewModel(
            color => Preview(shape => shape with { Appearance = shape.Appearance with { Fill = color } }),
            () => CommitPreview(HistoryStepType.ChangeFill),
            color => _editor.Apply(HistoryStepType.ChangeFill, shape => shape with { Appearance = shape.Appearance with { Fill = color } }));
        Stroke = new ColorFieldViewModel(
            color => Preview(shape => shape with { Appearance = shape.Appearance with { Stroke = color } }),
            () => CommitPreview(HistoryStepType.ChangeStroke),
            color => _editor.Apply(HistoryStepType.ChangeStroke, shape => shape with { Appearance = shape.Appearance with { Stroke = color } }));

        _state.PropertyChanged += OnStateChanged;
        RefreshValues();
    }

    public NumberFieldViewModel X { get; }

    public NumberFieldViewModel Y { get; }

    public NumberFieldViewModel Width { get; }

    public NumberFieldViewModel Height { get; }

    /// <summary>The rotation in degrees. Disabled for several shapes.</summary>
    public NumberFieldViewModel Rotation { get; }

    public NumberFieldViewModel StrokeWidth { get; }

    /// <summary>The opacity from 0 to 100.</summary>
    public NumberFieldViewModel OpacityPercent { get; }

    public ColorFieldViewModel Fill { get; }

    public ColorFieldViewModel Stroke { get; }

    public bool HasSelection => _state.HasSelection;

    public bool ShowNothingSelectedHint => !_state.HasSelection;

    /// <summary>"Nothing selected", "1 selected" or "N selected".</summary>
    public string Header => _state.SelectedCount switch
    {
        0 => Localizer.T(N("Nothing selected")),
        1 => Localizer.T(N("1 selected")),
        var count => string.Format(Localizer.Culture, Localizer.T("{0} selected"), count),
    };

    /// <summary>The value of the opacity slider, 0 to 100. Follows the shapes unless the slider is being dragged.</summary>
    [ObservableProperty] public partial double OpacitySliderValue { get; private set; } = 100;

    /// <summary>Whether changing the width or height changes the other one in the same ratio.</summary>
    [ObservableProperty] public partial bool KeepProportions { get; set; }

    [ObservableProperty] public partial bool IsTransformExpanded { get; set; } = true;

    [ObservableProperty] public partial bool IsAppearanceExpanded { get; set; } = true;

    [ObservableProperty] public partial bool IsPathExpanded { get; set; }

    [ObservableProperty] public partial bool IsAlignExpanded { get; set; } = true;

    [ObservableProperty] public partial bool IsQuickActionsExpanded { get; set; } = true;

    /// <summary>Sets the rotation of every selected shape to 0 as one step.</summary>
    [RelayCommand(CanExecute = nameof(CanResetTransform))]
    private void ResetTransform() => _editor.ApplyTransform(HistoryStepType.ResetTransform, SelectionTransforms.ResetRotation);

    /// <summary>The user started to move the opacity slider. The changes until <see cref="EndOpacityDrag"/> are one step.</summary>
    public void BeginOpacityDrag()
    {
        if (OpacityPercent.IsEnabled)
        {
            _opacityDragging = true;
        }
    }

    /// <summary>Shows the slider's opacity on the canvas. Ignored unless a drag began.</summary>
    public void PreviewOpacity(double percent)
    {
        if (!_opacityDragging || !double.IsFinite(percent))
        {
            return;
        }

        var opacity = Math.Clamp(percent, 0, 100) / 100;
        OpacitySliderValue = opacity * 100;
        Preview(shape => shape with { Appearance = shape.Appearance with { Opacity = opacity } });
    }

    /// <summary>The slider was let go. Records what was shown as one step, if the opacity changed.</summary>
    public void EndOpacityDrag()
    {
        if (_opacityDragging)
        {
            _opacityDragging = false;
            CommitPreview(HistoryStepType.ChangeOpacity);
        }
    }

    private bool CanResetTransform() => SelectedShapes().Any(shape => shape.Rotation != 0);

    private IReadOnlyList<Shape> SelectedShapes() => SelectionQueries.SelectedShapes(_state.Document, _state.SelectedShapeIds);

    private void Transform(HistoryStepType type, Func<IReadOnlyList<Shape>, SelectionFrame, IReadOnlyList<Shape>> change) =>
        _editor.ApplyTransform(type, shapes => SelectionGeometry.FrameOf(shapes) is { } frame ? change(shapes, frame) : shapes);

    // Shows a color on the canvas while the picker is open. Not a step yet: the picker closing records it.
    private void Preview(Func<Shape, Shape> change)
    {
        _previewBase ??= _state.Document;
        var changed = new Dictionary<Guid, Shape>();
        foreach (var shape in SelectionQueries.SelectedShapes(_previewBase, _state.SelectedShapeIds))
        {
            var replacement = change(shape);
            if (!Equals(replacement, shape))
            {
                changed[shape.Id] = replacement;
            }
        }

        var document = _previewBase.UpdateShapes(changed);
        _state.Preview(document);
    }

    private void CommitPreview(HistoryStepType type)
    {
        _previewBase = null;
        _state.Commit(type);
    }

    private void RefreshValues()
    {
        var shapes = SelectedShapes();
        var values = SelectionValues.From(shapes);
        if (values is null)
        {
            foreach (var field in new[] { X, Y, Width, Height, Rotation, StrokeWidth, OpacityPercent })
            {
                field.SetValue(null);
            }

            Fill.SetEnabled(false);
            Stroke.SetEnabled(false);
            ResetTransformCommand.NotifyCanExecuteChanged();
            return;
        }

        X.SetValue(values.X);
        Y.SetValue(values.Y);
        Width.SetValue(values.Width);
        Height.SetValue(values.Height);
        Rotation.SetValue(values.IsSingle ? values.Rotation : null);
        StrokeWidth.SetValue(values.StrokeWidth.Value, values.StrokeWidth.IsMixed);
        OpacityPercent.SetValue(values.Opacity.Value * 100, values.Opacity.IsMixed);
        if (!_opacityDragging)
        {
            OpacitySliderValue = values.Opacity.Value * 100;
        }
        Fill.SetEnabled(true);
        Stroke.SetEnabled(true);
        Fill.SetValue(values.Fill.Value, values.Fill.IsMixed);
        Stroke.SetValue(values.Stroke.Value, values.Stroke.IsMixed);
        ResetTransformCommand.NotifyCanExecuteChanged();
    }

    private void OnStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(EditorState.Document):
                RefreshValues();
                break;
            case nameof(EditorState.SelectedShapeIds):
                _previewBase = null;
                RefreshValues();
                break;
            case nameof(EditorState.HasSelection):
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(ShowNothingSelectedHint));
                break;
            case nameof(EditorState.SelectedCount):
                OnPropertyChanged(nameof(Header));
                break;
        }
    }
}
