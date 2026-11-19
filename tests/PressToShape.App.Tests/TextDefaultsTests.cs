using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class TextDefaultsTests
{
    [Fact]
    public void EditorState_NewState_HasTheInitialTextFormat()
    {
        var state = new EditorState();

        Assert.Equal(TextDefaults.Initial, state.TextDefaults);
        Assert.Equal(TextShape.DefaultFontFamily, state.TextDefaults.FontFamily);
        Assert.Equal(TextShape.DefaultFontSize, state.TextDefaults.FontSize);
    }

    [Fact]
    public void RememberTextFormat_SetValues_AreKeptAndTheOthersStay()
    {
        var state = new EditorState();

        state.RememberTextFormat(new TextFormatChange(FontSize: 40, IsBold: true));
        state.RememberTextFormat(new TextFormatChange(Alignment: TextAlignType.Right));

        Assert.Equal(40, state.TextDefaults.FontSize);
        Assert.True(state.TextDefaults.IsBold);
        Assert.Equal(TextAlignType.Right, state.TextDefaults.Alignment);
        Assert.Equal(TextShape.DefaultFontFamily, state.TextDefaults.FontFamily);
    }

    [Fact]
    public void RememberTextFormat_ChangesNotifyTheProperty()
    {
        var state = new EditorState();
        var changed = false;
        state.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(EditorState.TextDefaults);

        state.RememberTextFormat(new TextFormatChange(LetterSpacing: 2));

        Assert.True(changed);
    }

    [Fact]
    public void RememberTextFormat_NewDocument_KeepsTheFormat()
    {
        var state = new EditorState();
        state.RememberTextFormat(new TextFormatChange(FontFamily: "Serif"));

        state.ReplaceDocument(Core.Documents.Document.Create("L"));

        Assert.Equal("Serif", state.TextDefaults.FontFamily);
    }

    [Fact]
    public void NewText_UsesTheFormatAndTheGivenValues()
    {
        var defaults = TextDefaults.Initial.With(new TextFormatChange(FontFamily: "Serif", FontSize: 30, IsItalic: true, LineSpacing: 2, LetterSpacing: 1));
        var appearance = Appearance.Default with { Fill = Color.FromRgb(1, 2, 3) };

        var text = defaults.NewText(10, 20, 100, 50, isArea: true, "Hi", appearance);

        Assert.Equal((10d, 20d, 100d, 50d), (text.X, text.Y, text.Width, text.Height));
        Assert.True(text.IsArea);
        Assert.Equal("Hi", text.Text);
        Assert.Equal(appearance, text.Appearance);
        Assert.Equal(("Serif", 30d, true, 2d, 1d), (text.FontFamily, text.FontSize, text.IsItalic, text.LineSpacing, text.LetterSpacing));
    }
}
