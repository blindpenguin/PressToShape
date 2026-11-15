using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesViewModelTests
{
    private static readonly Color Red = Color.FromRgb(255, 0, 0);
    private static readonly Color Green = Color.FromRgb(0, 255, 0);

    private sealed class Fixture
    {
        public Fixture(string language = "en", Dictionary<string, string>? german = null)
        {
            Localizer = FakeLocalizer.Create(language, german);
            State = new EditorState();
            Editor = new SelectionEditor(State);
            Properties = new PropertiesViewModel(State, Editor, Localizer);
        }

        public Localizer Localizer { get; }

        public EditorState State { get; }

        public SelectionEditor Editor { get; }

        public PropertiesViewModel Properties { get; }

        public Guid[] Add(params Shape[] shapes)
        {
            State.Edit(HistoryStepType.DrawRectangle, d => shapes.Aggregate(d, (doc, s) => doc.AddShape(State.ActiveLayerId, s)));
            var ids = shapes.Select(s => s.Id).ToArray();
            State.SetSelection(ids);
            return ids;
        }

        public Shape Shape(Guid id) => State.Document.FindShape(id)!.Shape;

        public int Steps => State.History.Steps.Count;

        public HistoryStepType LastStep => State.History.Steps[^1];
    }

    private static RectangleShape Rect(double x = 10, double y = 20, double w = 100, double h = 50, double rotation = 0, Appearance? appearance = null) =>
        new() { X = x, Y = y, Width = w, Height = h, Rotation = rotation, Appearance = appearance ?? Appearance.Default };

    private static void Type(NumberFieldViewModel field, string text)
    {
        field.Text = text;
        field.Commit();
    }

    [Fact]
    public void CommitPendingEdits_AppliesOnlyTheFieldsBeingEdited_AsOneStepEach()
    {
        var f = new Fixture();
        var id = f.Add(Rect(x: 10, y: 20))[0];
        var steps = f.Steps;
        f.Properties.X.IsEditing = true;
        f.Properties.X.Text = "40";
        f.Properties.Y.Text = "99"; // typed text that is not being edited stays unapplied

        f.Properties.CommitPendingEdits();

        Assert.Equal(40, ((RectangleShape)f.Shape(id)).X);
        Assert.Equal(20, ((RectangleShape)f.Shape(id)).Y);
        Assert.False(f.Properties.X.IsEditing);
        Assert.Equal(steps + 1, f.Steps);
    }

    // ---- values ----

    [Fact]
    public void NothingSelected_DisablesEverything_AndShowsTheHeader()
    {
        var f = new Fixture();

        Assert.False(f.Properties.HasSelection);
        Assert.True(f.Properties.ShowNothingSelectedHint);
        Assert.Equal("Nothing selected", f.Properties.Header);
        foreach (var field in new[] { f.Properties.X, f.Properties.Y, f.Properties.Width, f.Properties.Height, f.Properties.Rotation, f.Properties.StrokeWidth, f.Properties.OpacityPercent })
        {
            Assert.False(field.IsEnabled);
            Assert.Equal(string.Empty, field.Text);
        }

        Assert.False(f.Properties.ResetTransformCommand.CanExecute(null));
    }

    [Fact]
    public void NothingSelected_ColorFieldsStayEnabled_AndShowTheDefaults()
    {
        var f = new Fixture();

        Assert.True(f.Properties.Fill.IsEnabled);
        Assert.True(f.Properties.Stroke.IsEnabled);
        Assert.Equal(Appearance.Default.Fill, f.Properties.Fill.Color);
        Assert.True(f.Properties.Fill.HasColor);
        Assert.Equal(Appearance.Default.Stroke, f.Properties.Stroke.Color);
    }

    [Fact]
    public void NothingSelected_ColorEdit_ChangesTheDefaultsAndNotTheDocumentOrHistory()
    {
        var f = new Fixture();
        var document = f.State.Document;
        var steps = f.Steps;

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(Red);
        Assert.Equal(Red, f.State.DefaultAppearance.Fill);
        f.Properties.Fill.Update(Green);
        f.Properties.Fill.EndEdit();

        Assert.Equal(Green, f.State.DefaultAppearance.Fill);
        Assert.Equal(Appearance.Default.Stroke, f.State.DefaultAppearance.Stroke);
        Assert.Same(document, f.State.Document);
        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void NothingSelected_NoneAndSetColor_ChangeTheDefaults()
    {
        var f = new Fixture();

        f.Properties.Stroke.SetNone();
        Assert.Null(f.State.DefaultAppearance.Stroke);
        Assert.True(f.Properties.Stroke.IsNone);

        f.Properties.Stroke.SetColor(Red);
        Assert.Equal(Red, f.State.DefaultAppearance.Stroke);
        Assert.False(f.Properties.Stroke.IsNone);
    }

    [Fact]
    public void SelectedShapes_ColorEdit_LeavesTheDefaultsAlone()
    {
        var f = new Fixture();
        f.Add(Rect());

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(Red);
        f.Properties.Fill.EndEdit();
        f.Properties.Stroke.SetColor(Green);

        Assert.Same(Appearance.Default, f.State.DefaultAppearance);
    }

    [Fact]
    public void ClearingTheSelection_ColorFieldsShowTheDefaultsAgain()
    {
        var f = new Fixture();
        f.Add(Rect(appearance: Appearance.Default with { Fill = Red }));
        Assert.Equal(Red, f.Properties.Fill.Color);

        f.State.SetSelection([]);

        Assert.True(f.Properties.Fill.IsEnabled);
        Assert.Equal(Appearance.Default.Fill, f.Properties.Fill.Color);
    }

    [Fact]
    public void ColorEdit_ThatChangedTheColor_AddsItToTheRecentColorsOnce()
    {
        var f = new Fixture();
        f.Add(Rect());

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(Red);
        f.Properties.Fill.Update(Green);
        Assert.Empty(f.Properties.RecentColors);
        f.Properties.Fill.EndEdit();

        Assert.Equal([Green], f.Properties.RecentColors.ToArray());
    }

    [Fact]
    public void ColorEdit_WithoutChange_AndNone_AddNoRecentColor()
    {
        var f = new Fixture();
        f.Add(Rect());

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.EndEdit();
        f.Properties.Fill.SetNone();

        Assert.Empty(f.Properties.RecentColors);
    }

    [Fact]
    public void SetColor_AddsItToTheRecentColors_AndRaisesPropertyChanged()
    {
        var f = new Fixture();
        var raised = new List<string?>();
        f.Properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.Properties.Stroke.SetColor(Red);

        Assert.Equal([Red], f.Properties.RecentColors.ToArray());
        Assert.Contains(nameof(PropertiesViewModel.RecentColors), raised);
    }

    [Fact]
    public void OneShape_ShowsItsValues()
    {
        var f = new Fixture();
        f.Add(Rect(10.5, 20, 100, 50, rotation: 30, appearance: Appearance.Default with { Fill = Red, StrokeWidth = 3, Opacity = 0.25 }));

        Assert.Equal("1 selected", f.Properties.Header);
        Assert.Equal("10.5", f.Properties.X.Text);
        Assert.Equal("20", f.Properties.Y.Text);
        Assert.Equal("100", f.Properties.Width.Text);
        Assert.Equal("50", f.Properties.Height.Text);
        Assert.Equal("30", f.Properties.Rotation.Text);
        Assert.True(f.Properties.Rotation.IsEnabled);
        Assert.Equal("3", f.Properties.StrokeWidth.Text);
        Assert.Equal("25", f.Properties.OpacityPercent.Text);
        Assert.Equal(Red, f.Properties.Fill.Color);
        Assert.True(f.Properties.Fill.HasColor);
        Assert.True(f.Properties.ResetTransformCommand.CanExecute(null));
    }

    [Fact]
    public void SeveralShapes_ShowTheirBox_RotationIsDisabled_AndDifferentValuesAreMixed()
    {
        var f = new Fixture();
        f.Add(
            Rect(0, 0, 10, 10, appearance: Appearance.Default with { Fill = Red, Opacity = 0.5 }),
            Rect(100, 50, 20, 20, appearance: Appearance.Default with { Fill = Green, Opacity = 0.5 }));

        Assert.Equal("2 selected", f.Properties.Header);
        Assert.Equal("0", f.Properties.X.Text);
        Assert.Equal("120", f.Properties.Width.Text);
        Assert.Equal("70", f.Properties.Height.Text);
        Assert.False(f.Properties.Rotation.IsEnabled);
        Assert.True(f.Properties.Fill.IsMixed);
        Assert.False(f.Properties.Fill.HasColor);
        Assert.False(f.Properties.Stroke.IsMixed);
        Assert.Equal("50", f.Properties.OpacityPercent.Text);
        Assert.False(f.Properties.OpacityPercent.IsMixed);
    }

    [Fact]
    public void MixedStrokeWidth_ShowsAnEmptyText()
    {
        var f = new Fixture();
        f.Add(Rect(appearance: Appearance.Default with { StrokeWidth = 1 }), Rect(appearance: Appearance.Default with { StrokeWidth = 4 }));

        Assert.True(f.Properties.StrokeWidth.IsMixed);
        Assert.Equal(string.Empty, f.Properties.StrokeWidth.Text);
        Assert.True(f.Properties.StrokeWidth.IsEnabled);
    }

    [Fact]
    public void Header_FollowsTheLanguage()
    {
        var f = new Fixture("en", new Dictionary<string, string> { ["Nothing selected"] = "Nichts ausgewählt", ["1 selected"] = "1 ausgewählt", ["{0} selected"] = "{0} ausgewählt" });
        f.Localizer.SetLanguage("de");

        Assert.Equal("Nichts ausgewählt", f.Properties.Header);
        f.Add(Rect());
        Assert.Equal("1 ausgewählt", f.Properties.Header);
        f.Add(Rect(), Rect());
        Assert.Equal("2 ausgewählt", f.Properties.Header);
    }

    // ---- editing fields ----

    [Fact]
    public void EditingPosition_MovesTheShape_AsOneChangePositionStep()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var steps = f.Steps;

        Type(f.Properties.X, "42");
        Type(f.Properties.Y, "7,5".Replace(',', '.'));

        Assert.Equal((42, 7.5), (f.Shape(id).X, f.Shape(id).Y));
        Assert.Equal(steps + 2, f.Steps);
        Assert.Equal(HistoryStepType.ChangePosition, f.LastStep);
        Assert.Equal("42", f.Properties.X.Text);
    }

    [Fact]
    public void EditingPosition_OfSeveralShapes_MovesTheGroupBox()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(0, 0, 10, 10), Rect(100, 0, 10, 10));

        Type(f.Properties.X, "50");

        Assert.Equal(50, f.Shape(ids[0]).X);
        Assert.Equal(150, f.Shape(ids[1]).X);
    }

    [Fact]
    public void EditingSize_ChangesTheShape_AsOneChangeSizeStep()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var steps = f.Steps;

        Type(f.Properties.Width, "200");
        Type(f.Properties.Height, "10");

        Assert.Equal((200, 10), (f.Shape(id).Width, f.Shape(id).Height));
        Assert.Equal((10, 20), (f.Shape(id).X, f.Shape(id).Y));
        Assert.Equal(steps + 2, f.Steps);
        Assert.Equal(HistoryStepType.ChangeSize, f.LastStep);
    }

    [Fact]
    public void KeepProportions_ChangesTheOtherSideInTheSameRatio()
    {
        var f = new Fixture();
        var id = f.Add(Rect(w: 100, h: 50))[0];
        f.Properties.KeepProportions = true;

        Type(f.Properties.Width, "200");
        Assert.Equal((200, 100), (f.Shape(id).Width, f.Shape(id).Height));
        Assert.Equal("100", f.Properties.Height.Text);

        Type(f.Properties.Height, "20");
        Assert.Equal((40, 20), (f.Shape(id).Width, f.Shape(id).Height));
    }

    [Fact]
    public void EditingRotation_SetsTheAngle_Normalized()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];

        Type(f.Properties.Rotation, "-90");

        Assert.Equal(270, f.Shape(id).Rotation, 6);
        Assert.Equal(HistoryStepType.ChangeRotation, f.LastStep);
        Assert.Equal("270", f.Properties.Rotation.Text);
    }

    [Fact]
    public void EditingStrokeWidthAndOpacity_AreOneStepEach_WithTheRightType()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];

        Type(f.Properties.StrokeWidth, "5");
        Assert.Equal(5, f.Shape(id).Appearance.StrokeWidth);
        Assert.Equal(HistoryStepType.ChangeStrokeWidth, f.LastStep);

        Type(f.Properties.OpacityPercent, "40");
        Assert.Equal(0.4, f.Shape(id).Appearance.Opacity, 6);
        Assert.Equal(HistoryStepType.ChangeOpacity, f.LastStep);
    }

    [Fact]
    public void EditingAMixedField_AppliesToAllShapes()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(appearance: Appearance.Default with { StrokeWidth = 1 }), Rect(appearance: Appearance.Default with { StrokeWidth = 4 }));

        Type(f.Properties.StrokeWidth, "2");

        Assert.All(ids, id => Assert.Equal(2, f.Shape(id).Appearance.StrokeWidth));
        Assert.False(f.Properties.StrokeWidth.IsMixed);
        Assert.Equal("2", f.Properties.StrokeWidth.Text);
    }

    [Fact]
    public void EqualValuesAndInvalidText_CreateNoStep_AndRevert()
    {
        var f = new Fixture();
        f.Add(Rect());
        var steps = f.Steps;

        Type(f.Properties.X, "10");
        Type(f.Properties.Width, "abc");
        Type(f.Properties.Height, "-5");
        Type(f.Properties.OpacityPercent, "101");
        Type(f.Properties.StrokeWidth, "");

        Assert.Equal(steps, f.Steps);
        Assert.Equal("100", f.Properties.Width.Text);
        Assert.Equal("50", f.Properties.Height.Text);
        Assert.Equal("100", f.Properties.OpacityPercent.Text);
    }

    [Fact]
    public void FieldEdits_UndoAndRedo_RefreshTheFields()
    {
        var f = new Fixture();
        f.Add(Rect());
        Type(f.Properties.X, "42");

        f.State.Undo();
        Assert.Equal("10", f.Properties.X.Text);

        f.State.Redo();
        Assert.Equal("42", f.Properties.X.Text);
    }

    [Fact]
    public void GermanCulture_FormatsAndParsesWithAComma()
    {
        var f = new Fixture("de");
        var id = f.Add(Rect(10.5))[0];

        Assert.Equal("10,5", f.Properties.X.Text);
        Type(f.Properties.Y, "3,25");

        Assert.Equal(3.25, f.Shape(id).Y);
        Assert.Equal("3,25", f.Properties.Y.Text);
    }

    [Fact]
    public void LanguageSwitch_ReformatsTheNumbers()
    {
        var f = new Fixture();
        f.Add(Rect(10.5));

        f.Localizer.SetLanguage("de");

        Assert.Equal("10,5", f.Properties.X.Text);
    }

    // ---- live refresh ----

    [Fact]
    public void ADragPreview_RefreshesTheFields_InPlace()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var field = f.Properties.X;

        f.State.Preview(f.State.Document.UpdateShape(id, s => s with { X = 77 }));

        Assert.Same(field, f.Properties.X);
        Assert.Equal("77", f.Properties.X.Text);
    }

    [Fact]
    public void AFieldBeingEdited_IsNotOverwrittenByARefresh()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        f.Properties.Y.IsEditing = true;
        f.Properties.Y.Text = "99";

        f.State.Preview(f.State.Document.UpdateShape(id, s => s with { X = 5, Y = 6 }));

        Assert.Equal("99", f.Properties.Y.Text);
        Assert.Equal("5", f.Properties.X.Text);
    }

    // ---- colors ----

    [Fact]
    public void ColorEdit_PreviewsLive_AndIsOneChangeFillStepWhenThePickerCloses()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var steps = f.Steps;

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(Red);
        Assert.Equal(Red, f.Shape(id).Appearance.Fill);
        f.Properties.Fill.Update(Green);
        Assert.Equal(Green, f.Shape(id).Appearance.Fill);
        Assert.Equal(steps, f.Steps);
        f.Properties.Fill.EndEdit();

        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.ChangeFill, f.LastStep);
        Assert.Equal(Green, f.Shape(id).Appearance.Fill);

        f.State.Undo();
        Assert.Equal(Appearance.Default.Fill, f.Shape(id).Appearance.Fill);
    }

    [Fact]
    public void Pick_WithASelection_SetsTheFieldAsOneStep_AndAddsARecentColor()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var steps = f.Steps;

        f.Properties.StartPick(ColorPickTarget.Stroke);
        Assert.Equal(ColorPickTarget.Stroke, f.State.ColorPick);
        f.State.CompleteColorPick(Red);

        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.ChangeStroke, f.LastStep);
        Assert.Equal(Red, f.Shape(id).Appearance.Stroke);
        Assert.Equal([Red], f.Properties.RecentColors.ToArray());
        Assert.Same(Appearance.Default, f.State.DefaultAppearance);
    }

    [Fact]
    public void Pick_WithoutASelection_ChangesTheDefaultsOnly()
    {
        var f = new Fixture();
        var steps = f.Steps;

        f.Properties.StartPick(ColorPickTarget.Fill);
        f.State.CompleteColorPick(Green);

        Assert.Equal(Green, f.State.DefaultAppearance.Fill);
        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void Pick_WithoutAColorOrCancelled_ChangesNothing()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var steps = f.Steps;

        f.Properties.StartPick(ColorPickTarget.Fill);
        f.State.CompleteColorPick(null);
        f.Properties.StartPick(ColorPickTarget.Fill);
        f.State.CancelColorPick();

        Assert.Equal(steps, f.Steps);
        Assert.Equal(Appearance.Default.Fill, f.Shape(id).Appearance.Fill);
        Assert.Empty(f.Properties.RecentColors);
    }

    [Fact]
    public void Pick_OfTheColorAlreadySet_RecordsNoStep()
    {
        var f = new Fixture();
        f.Add(Rect());
        var steps = f.Steps;

        f.Properties.StartPick(ColorPickTarget.Fill);
        f.State.CompleteColorPick(Appearance.Default.Fill);

        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void ColorPicker_ClosedWithoutAChange_RecordsNoStep()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];
        var steps = f.Steps;
        var before = f.Shape(id);

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.EndEdit();

        Assert.Equal(steps, f.Steps);
        Assert.Equal(before, f.Shape(id));
    }

    [Fact]
    public void MixedFill_OpeningAndClosingThePickerWithoutAPick_LeavesTheShapesAlone()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(appearance: Appearance.Default with { Fill = Red }), Rect(appearance: Appearance.Default with { Fill = Green }));
        var steps = f.Steps;

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.EndEdit();

        Assert.True(f.Properties.Fill.IsMixed);
        Assert.Equal(steps, f.Steps);
        Assert.Equal(Red, f.Shape(ids[0]).Appearance.Fill);
        Assert.Equal(Green, f.Shape(ids[1]).Appearance.Fill);
    }

    [Fact]
    public void MixedFill_PickingAColor_SetsAllShapesAsOneStep()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(appearance: Appearance.Default with { Fill = Red }), Rect(appearance: Appearance.Default with { Fill = Green }));
        var steps = f.Steps;
        var blue = Color.FromRgb(0, 0, 255);

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(blue);
        f.Properties.Fill.EndEdit();

        Assert.Equal(steps + 1, f.Steps);
        Assert.All(ids, id => Assert.Equal(blue, f.Shape(id).Appearance.Fill));
        Assert.False(f.Properties.Fill.IsMixed);
    }

    [Fact]
    public void MixedStroke_None_RemovesTheStrokeOfAllShapesAsOneStep()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(appearance: Appearance.Default with { Stroke = Red }), Rect(appearance: Appearance.Default with { Stroke = Green }));
        var steps = f.Steps;

        f.Properties.Stroke.ToggleNone();

        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.ChangeStroke, f.LastStep);
        Assert.All(ids, id => Assert.Null(f.Shape(id).Appearance.Stroke));
        Assert.True(f.Properties.Stroke.IsNone);

        f.State.Undo();
        Assert.Equal(Red, f.Shape(ids[0]).Appearance.Stroke);
        Assert.Equal(Green, f.Shape(ids[1]).Appearance.Stroke);
    }

    [Fact]
    public void StrokeColorEdit_IsAChangeStrokeStep_ForAllSelectedShapes()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(), Rect(x: 200));

        f.Properties.Stroke.BeginEdit();
        f.Properties.Stroke.Update(Red);
        f.Properties.Stroke.EndEdit();

        Assert.All(ids, id => Assert.Equal(Red, f.Shape(id).Appearance.Stroke));
        Assert.Equal(HistoryStepType.ChangeStroke, f.LastStep);
    }

    [Fact]
    public void ClosingThePickerWithoutAChange_MakesNoStep()
    {
        var f = new Fixture();
        f.Add(Rect());
        var steps = f.Steps;

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.EndEdit();

        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void NoneAndColorOutsideThePicker_AreImmediateSteps()
    {
        var f = new Fixture();
        var id = f.Add(Rect())[0];

        f.Properties.Fill.SetNone();
        Assert.Null(f.Shape(id).Appearance.Fill);
        Assert.Equal(HistoryStepType.ChangeFill, f.LastStep);
        Assert.False(f.Properties.Fill.HasColor);

        f.Properties.Stroke.SetColor(Green);
        Assert.Equal(Green, f.Shape(id).Appearance.Stroke);
        Assert.Equal(HistoryStepType.ChangeStroke, f.LastStep);
    }

    [Fact]
    public void ColorField_WithMixedFills_BecomesAColorOnTheFirstUpdate()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(appearance: Appearance.Default with { Fill = Red }), Rect(appearance: Appearance.Default with { Fill = Green }));

        f.Properties.Fill.BeginEdit();
        f.Properties.Fill.Update(Red);
        f.Properties.Fill.EndEdit();

        Assert.All(ids, id => Assert.Equal(Red, f.Shape(id).Appearance.Fill));
        Assert.False(f.Properties.Fill.IsMixed);
    }

    // ---- reset transform and flags ----

    [Fact]
    public void ResetTransform_IsOneStep_ClearsTheRotation_AndDisablesItself()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(rotation: 30), Rect(x: 300, rotation: 80));
        var steps = f.Steps;

        Assert.True(f.Properties.ResetTransformCommand.CanExecute(null));
        f.Properties.ResetTransformCommand.Execute(null);

        Assert.All(ids, id => Assert.Equal(0, f.Shape(id).Rotation));
        Assert.Equal(steps + 1, f.Steps);
        Assert.Equal(HistoryStepType.ResetTransform, f.LastStep);
        Assert.False(f.Properties.ResetTransformCommand.CanExecute(null));
    }

    [Fact]
    public void SelectionChanges_CreateNoSteps_AndSectionFlagsKeepWorking()
    {
        var f = new Fixture();
        var ids = f.Add(Rect(), Rect(x: 200));
        var steps = f.Steps;

        f.State.SetSelection(ids.Take(1));
        f.State.ClearSelection();

        Assert.Equal(steps, f.Steps);
        Assert.True(f.Properties.IsTransformExpanded);
        Assert.False(f.Properties.IsPathExpanded);
        f.Properties.IsPathExpanded = true;
        Assert.True(f.Properties.IsPathExpanded);
    }

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        var state = new EditorState();
        var localizer = FakeLocalizer.Create();

        Assert.Throws<ArgumentNullException>(() => new PropertiesViewModel(null!, new SelectionEditor(state), localizer));
        Assert.Throws<ArgumentNullException>(() => new PropertiesViewModel(state, null!, localizer));
        Assert.Throws<ArgumentNullException>(() => new PropertiesViewModel(state, new SelectionEditor(state), null!));
    }

    // ---- sections per tool ----

    [Fact]
    public void ActiveTool_Changes_RaisesPropertyChangedForEverySectionFlag()
    {
        var f = new Fixture();
        var raised = new List<string?>();
        f.Properties.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        f.State.ActiveTool = Tool.Pen;

        foreach (var name in new[]
                 {
                     nameof(PropertiesViewModel.ShowTransform), nameof(PropertiesViewModel.ShowAppearance), nameof(PropertiesViewModel.ShowPath),
                     nameof(PropertiesViewModel.ShowAlign), nameof(PropertiesViewModel.ShowQuickActions), nameof(PropertiesViewModel.ShowHeader),
                     nameof(PropertiesViewModel.ShowNoPropertiesHint),
                 })
        {
            Assert.Contains(name, raised);
        }
    }

    [Fact]
    public void Sections_SelectTool_ShowsAllButPath()
    {
        var f = new Fixture();

        Assert.True(f.Properties.ShowTransform);
        Assert.True(f.Properties.ShowAppearance);
        Assert.False(f.Properties.ShowPath);
        Assert.True(f.Properties.ShowAlign);
        Assert.True(f.Properties.ShowQuickActions);
        Assert.True(f.Properties.ShowHeader);
        Assert.False(f.Properties.ShowNoPropertiesHint);
    }

    [Fact]
    public void Sections_PenTool_ShowsAppearanceAndPathOnly()
    {
        var f = new Fixture();

        f.State.ActiveTool = Tool.Pen;

        Assert.False(f.Properties.ShowTransform);
        Assert.True(f.Properties.ShowAppearance);
        Assert.True(f.Properties.ShowPath);
        Assert.False(f.Properties.ShowAlign);
        Assert.False(f.Properties.ShowQuickActions);
        Assert.True(f.Properties.ShowHeader);
        Assert.False(f.Properties.ShowNoPropertiesHint);
    }

    [Fact]
    public void Sections_ToolWithoutProperties_HidesEverythingAndShowsTheHint()
    {
        var f = new Fixture();

        foreach (var tool in new[] { Tool.Hand, Tool.Zoom, Tool.Eyedropper })
        {
            f.State.ActiveTool = tool;

            Assert.False(f.Properties.ShowTransform);
            Assert.False(f.Properties.ShowAppearance);
            Assert.False(f.Properties.ShowPath);
            Assert.False(f.Properties.ShowAlign);
            Assert.False(f.Properties.ShowQuickActions);
            Assert.False(f.Properties.ShowHeader);
            Assert.True(f.Properties.ShowNoPropertiesHint);
        }
    }

    [Fact]
    public void ActiveTool_Switched_KeepsExpansionAndFieldValues()
    {
        var f = new Fixture();
        f.Add(Rect(x: 10, y: 20));
        f.Properties.IsTransformExpanded = false;
        f.Properties.IsPathExpanded = true;

        f.State.ActiveTool = Tool.Hand;
        f.State.ActiveTool = Tool.Select;

        Assert.False(f.Properties.IsTransformExpanded);
        Assert.True(f.Properties.IsPathExpanded);
        Assert.Equal("10", f.Properties.X.Text);
        Assert.True(f.Properties.X.IsEnabled);
        Assert.Equal("20", f.Properties.Y.Text);
    }
}
