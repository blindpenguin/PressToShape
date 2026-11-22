using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class LayersViewModelTextTests
{
    private static TextShape Text(string name = "") => new() { Width = 20, Height = 10, Text = "Hello", Name = name };

    private static LayersViewModel Create(Localizer? localizer, params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        return new LayersViewModel(state, new ObjectEditor(state), localizer ?? FakeLocalizer.Create());
    }

    [Fact]
    public void Rows_TextWithoutAName_HasTheTextTypeAndIsCalledText()
    {
        var text = Text();
        var list = Create(null, text);

        var row = list.Rows.Single(r => r.Id == text.Id);

        Assert.Equal(ObjectRowType.Text, row.Type);
        Assert.Equal("Text", row.Name);
        Assert.True(row.IsText);
        Assert.False(row.IsRectangle);
        Assert.False(row.IsPath);
    }

    [Fact]
    public void Rows_NonTextShapes_AreNotMistakenForText()
    {
        var list = Create(null, new RectangleShape { Width = 1, Height = 1 });

        Assert.All(list.Rows, row => Assert.False(row.IsText));
    }

    [Fact]
    public void Rows_TextWithAName_ShowsTheName()
    {
        var list = Create(null, Text("Title"));

        Assert.Equal("Title", list.Rows[1].Name);
    }

    [Fact]
    public void Rows_TextName_IsTranslatedAndFollowsALanguageChange()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Text"] = "Schrift" });
        var list = Create(localizer, Text());
        Assert.Equal("Text", list.Rows[1].Name);

        localizer.SetLanguage("de");

        Assert.Equal("Schrift", list.Rows[1].Name);
    }

    [Fact]
    public void Rows_TextInsideAGroup_HasTheTextType()
    {
        var text = Text();
        var group = new GroupShape { Children = [text] };
        var list = Create(null, group);

        list.SetExpanded(list.Rows.Single(r => r.IsGroup), true);

        Assert.Equal(ObjectRowType.Text, list.Rows.Single(r => r.Id == text.Id).Type);
    }

    [Fact]
    public void Rows_OtherShapes_KeepTheirTypesAndNames()
    {
        var list = Create(null, new RectangleShape { Width = 1, Height = 1 }, new EllipseShape { Width = 1, Height = 1 });

        Assert.Equal([ObjectRowType.Layer, ObjectRowType.Ellipse, ObjectRowType.Rectangle], list.Rows.Select(r => r.Type).ToArray());
        Assert.Equal(["Layer", "Ellipse", "Rectangle"], list.Rows.Select(r => r.Name).ToArray());
    }
}
