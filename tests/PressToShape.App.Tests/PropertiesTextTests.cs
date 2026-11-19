using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.Services;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using PressToShape.Core.Tests;
using Xunit;

namespace PressToShape.App.Tests;

public class PropertiesTextTests
{
    private sealed class FakeFontCatalog(params string[] families) : IFontCatalog
    {
        public IReadOnlyList<string> Families { get; } = families;
    }

    private sealed class Fixture
    {
        public Fixture(Tool tool = Tool.Select, params TextShape[] texts)
        {
            var document = Document.Create("L");
            document = document.AddShapes(document.Layers[0].Id, texts);
            State = new EditorState(document) { ActiveTool = tool };
            Properties = new PropertiesViewModel(
                State,
                new SelectionEditor(State),
                FakeLocalizer.Create(),
                textMeasurer: new FakeTextMeasurer(),
                fonts: new FakeFontCatalog("Inter", "Serif", "Mono"));
        }

        public EditorState State { get; }

        public PropertiesViewModel Properties { get; }

        public TextShape Text(TextShape original) => (TextShape)State.Document.FindShape(original.Id)!.Shape;

        public int Steps => State.History.Steps.Count;
    }

    private static TextShape Text(string content = "Hello", double x = 10, double size = 20) =>
        new() { X = x, Y = 10, Width = 60, Height = 24, Text = content, FontSize = size, FontFamily = "Serif" };

    [Fact]
    public void ShowText_SelectToolWithoutATextSelected_IsHidden()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);

        Assert.False(f.Properties.ShowText);
        Assert.False(f.Properties.HasTextSelected);
    }

    [Fact]
    public void ShowText_SelectToolWithATextSelected_IsShown()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);

        f.State.Select(text.Id);

        Assert.True(f.Properties.ShowText);
    }

    [Fact]
    public void ShowText_SelectToolWithARectangleSelected_IsHidden()
    {
        var f = new Fixture(Tool.Select);
        var rectangle = new RectangleShape { Width = 5, Height = 5 };
        f.State.Edit(Core.History.HistoryStepType.DrawRectangle, d => d.AddShape(f.State.ActiveLayerId, rectangle));
        f.State.Select(rectangle.Id);

        Assert.False(f.Properties.ShowText);
    }

    [Fact]
    public void ShowText_TextToolWithNothingSelected_IsShown()
    {
        var f = new Fixture(Tool.Text);

        Assert.True(f.Properties.ShowText);
    }

    [Fact]
    public void ShowText_FollowsTheTool()
    {
        var f = new Fixture(Tool.Rectangle);
        var changed = new List<string>();
        f.Properties.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        f.State.ActiveTool = Tool.Text;

        Assert.Contains(nameof(PropertiesViewModel.ShowText), changed);
        Assert.True(f.Properties.ShowText);
    }

    [Fact]
    public void Values_SingleText_ShowItsFormat()
    {
        var text = Text() with { FontSize = 33, IsBold = true, IsItalic = false, Alignment = TextAlignType.Center, LineSpacing = 1.5, LetterSpacing = 2 };
        var f = new Fixture(Tool.Select, text);

        f.State.Select(text.Id);

        Assert.Equal("33", f.Properties.FontSize.Text);
        Assert.Equal("1.5", f.Properties.LineSpacing.Text);
        Assert.Equal("2", f.Properties.LetterSpacing.Text);
        Assert.True(f.Properties.IsBold);
        Assert.False(f.Properties.IsItalic);
        Assert.True(f.Properties.IsAlignedCenter);
        Assert.False(f.Properties.IsAlignedLeft);
        Assert.Equal("Serif", f.Properties.SelectedFont!.Name);
    }

    [Fact]
    public void Values_TextsThatDiffer_AreMixed()
    {
        var first = Text("a", size: 20);
        var second = Text("b", x: 100, size: 40) with { FontFamily = "Mono", IsBold = true, Alignment = TextAlignType.Right };
        var f = new Fixture(Tool.Select, first, second);

        f.State.SetSelection([first.Id, second.Id]);

        Assert.True(f.Properties.FontSize.IsMixed);
        Assert.Null(f.Properties.SelectedFont);
        Assert.False(f.Properties.IsBold);
        Assert.False(f.Properties.IsAlignedLeft || f.Properties.IsAlignedRight || f.Properties.IsAlignedCenter);
    }

    [Fact]
    public void Values_TextsThatAgree_AreNotMixed()
    {
        var first = Text("a");
        var second = Text("b", x: 100);
        var f = new Fixture(Tool.Select, first, second);

        f.State.SetSelection([first.Id, second.Id]);

        Assert.False(f.Properties.FontSize.IsMixed);
        Assert.Equal("Serif", f.Properties.SelectedFont!.Name);
    }

    [Fact]
    public void FontSize_Typed_ChangesTheTextAsOneStepAndMeasuresAPointText()
    {
        var text = Text() with { Width = 0, Height = 0 };
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);
        var steps = f.Steps;

        f.Properties.FontSize.Text = "40";
        f.Properties.FontSize.Commit();

        var changed = f.Text(text);
        Assert.Equal(40, changed.FontSize);
        Assert.Equal(steps + 1, f.Steps);
        var size = new FakeTextMeasurer().Layout(changed);
        Assert.Equal((size.Width, size.Height), (changed.Width, changed.Height));
        Assert.Equal(40, f.State.TextDefaults.FontSize);
    }

    [Fact]
    public void FontSize_AreaText_KeepsItsBox()
    {
        var text = Text() with { IsArea = true, Width = 100, Height = 50 };
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);

        f.Properties.FontSize.Text = "40";
        f.Properties.FontSize.Commit();

        Assert.Equal((100d, 50d), (f.Text(text).Width, f.Text(text).Height));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("2001")]
    [InlineData("abc")]
    [InlineData("")]
    public void FontSize_InvalidInput_ChangesNothingAndComesBack(string typed)
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);
        var document = f.State.Document;

        f.Properties.FontSize.Text = typed;
        f.Properties.FontSize.Commit();

        Assert.Same(document, f.State.Document);
        Assert.Equal("20", f.Properties.FontSize.Text);
    }

    [Fact]
    public void LineSpacingAndLetterSpacing_Typed_ChangeTheText()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);

        f.Properties.LineSpacing.Text = "2";
        f.Properties.LineSpacing.Commit();
        f.Properties.LetterSpacing.Text = "-3";
        f.Properties.LetterSpacing.Commit();

        Assert.Equal((2d, -3d), (f.Text(text).LineSpacing, f.Text(text).LetterSpacing));
    }

    [Fact]
    public void LineSpacing_OutOfRange_IsRefused()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);

        f.Properties.LineSpacing.Text = "9";
        f.Properties.LineSpacing.Commit();

        Assert.Equal(1, f.Text(text).LineSpacing);
    }

    [Fact]
    public void BoldAndItalic_Set_ChangeTheTextAndKeepTheOtherFlag()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);

        f.Properties.IsBold = true;
        f.Properties.IsItalic = true;
        f.Properties.IsBold = false;

        Assert.False(f.Text(text).IsBold);
        Assert.True(f.Text(text).IsItalic);
        Assert.True(f.Properties.IsItalic);
    }

    [Fact]
    public void Alignment_Command_ChangesTheTextAndTheToggles()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);

        f.Properties.SetTextAlignmentCommand.Execute(TextAlignType.Right);

        Assert.Equal(TextAlignType.Right, f.Text(text).Alignment);
        Assert.True(f.Properties.IsAlignedRight);
        Assert.False(f.Properties.IsAlignedLeft);
    }

    [Fact]
    public void SelectedFont_Chosen_ChangesTheFontOfEverySelectedText()
    {
        var first = Text("a");
        var second = Text("b", x: 100) with { FontFamily = "Mono" };
        var f = new Fixture(Tool.Select, first, second);
        f.State.SetSelection([first.Id, second.Id]);
        var steps = f.Steps;

        f.Properties.SelectedFont = f.Properties.FontFamilies.Single(item => item.Name == "Inter");

        Assert.Equal("Inter", f.Text(first).FontFamily);
        Assert.Equal("Inter", f.Text(second).FontFamily);
        Assert.Equal(steps + 1, f.Steps);
    }

    [Fact]
    public void SelectedFont_SetToNull_DoesNothing()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);
        var document = f.State.Document;

        f.Properties.SelectedFont = null;

        Assert.Same(document, f.State.Document);
    }

    [Fact]
    public void FontFamilies_FontNotInstalled_IsListedAndMarked()
    {
        var text = Text() with { FontFamily = "Gone Font" };
        var f = new Fixture(Tool.Select, text);

        f.State.Select(text.Id);

        var selected = f.Properties.SelectedFont!;
        Assert.Equal("Gone Font", selected.Name);
        Assert.False(selected.IsInstalled);
        Assert.Equal("Gone Font (not installed)", selected.Display);
        Assert.Equal(["Inter", "Serif", "Mono"], f.Properties.FontFamilies.Where(item => item.IsInstalled).Select(item => item.Name));
        Assert.Equal("Gone Font", f.Text(text).FontFamily);
    }

    [Fact]
    public void FontFamilies_InstalledFont_HasNoExtraEntry()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);

        f.State.Select(text.Id);

        Assert.Equal(3, f.Properties.FontFamilies.Count);
        Assert.All(f.Properties.FontFamilies, item => Assert.True(item.IsInstalled));
    }

    [Fact]
    public void TextTool_NothingSelected_ControlsShowAndSetTheDefaultsForTheNextText()
    {
        var f = new Fixture(Tool.Text);
        var document = f.State.Document;

        Assert.Equal("24", f.Properties.FontSize.Text);
        Assert.True(f.Properties.FontSize.IsEnabled);

        f.Properties.FontSize.Text = "50";
        f.Properties.FontSize.Commit();
        f.Properties.IsItalic = true;
        f.Properties.SetTextAlignmentCommand.Execute(TextAlignType.Center);

        Assert.Same(document, f.State.Document);
        Assert.Equal(50, f.State.TextDefaults.FontSize);
        Assert.True(f.State.TextDefaults.IsItalic);
        Assert.Equal(TextAlignType.Center, f.State.TextDefaults.Alignment);
        Assert.Equal("50", f.Properties.FontSize.Text);
        Assert.True(f.Properties.IsItalic);
        Assert.True(f.Properties.IsAlignedCenter);
    }

    [Fact]
    public void Change_LockedText_IsNotChanged()
    {
        var text = Text() with { IsLocked = true };
        var f = new Fixture(Tool.Text, text);

        f.State.SetSelection([text.Id]);
        f.Properties.IsBold = true;

        // A locked text cannot be selected, so the controls edit the defaults and the text stays as it is.
        Assert.False(f.Text(text).IsBold);
    }

    [Fact]
    public void Change_SameValue_MakesNoStep()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);
        var steps = f.Steps;

        f.Properties.IsBold = false;

        Assert.Equal(steps, f.Steps);
    }

    [Fact]
    public void Change_UndoRestoresTheFormatAndTheControls()
    {
        var text = Text();
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);
        f.Properties.IsBold = true;

        f.State.Undo();

        Assert.False(f.Text(text).IsBold);
        Assert.False(f.Properties.IsBold);
    }

    [Fact]
    public void FontsWithoutACatalog_GiveAnEmptyListButStillShowAMissingFont()
    {
        var text = Text();
        var document = Document.Create("L");
        document = document.AddShapes(document.Layers[0].Id, [text]);
        var state = new EditorState(document) { ActiveTool = Tool.Select };
        var properties = new PropertiesViewModel(state, new SelectionEditor(state), FakeLocalizer.Create());

        state.Select(text.Id);

        Assert.Equal(["Serif"], properties.FontFamilies.Select(item => item.Name));
        Assert.False(properties.FontFamilies[0].IsInstalled);
    }

    [Fact]
    public void FontFamilies_IsTheSameCollectionAfterChanges_SoTheBoxCanFollowIt()
    {
        var text = Text() with { FontFamily = "Gone Font" };
        var f = new Fixture(Tool.Select, text);
        f.State.Select(text.Id);
        var list = f.Properties.FontFamilies;

        f.Properties.SelectedFont = list.Single(item => item.Name == "Mono");
        f.Properties.FontSize.Text = "30";
        f.Properties.FontSize.Commit();

        Assert.Same(list, f.Properties.FontFamilies);
        Assert.Equal(["Inter", "Serif", "Mono", "Gone Font"], list.Select(item => item.Name));
    }
}
