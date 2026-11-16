using System.Collections.Generic;
using System.Linq;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using PressToShape.App.Workspace;
using PressToShape.Core.Documents;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class LayersViewModelPathTests
{
    private static PathShape Path(string name = "") =>
        PathShape.Create([new DocumentNode(0, 0), new DocumentNode(10, 10)]) with { Name = name };

    private static (EditorState State, LayersViewModel List) Create(Localizer? localizer, params Shape[] shapes)
    {
        var document = Document.Create("Layer");
        var state = new EditorState(document.AddShapes(document.Layers[0].Id, shapes));
        return (state, new LayersViewModel(state, new ObjectEditor(state), localizer ?? FakeLocalizer.Create()));
    }

    [Fact]
    public void Rows_Path_HasThePathTypeAndNoOtherShapeFlag()
    {
        var path = Path();
        var (_, list) = Create(null, path);

        var row = list.Rows.Single(r => r.Id == path.Id);

        Assert.Equal(ObjectRowType.Path, row.Type);
        Assert.True(row.IsPath);
        Assert.False(row.IsRectangle);
        Assert.False(row.IsEllipse);
        Assert.False(row.IsGroup);
        Assert.False(row.HasChildren);
    }

    [Fact]
    public void Rows_PathWithoutAName_IsCalledPath()
    {
        var (_, list) = Create(null, Path());

        Assert.Equal(["Layer", "Path"], list.Rows.Select(r => r.Name).ToArray());
    }

    [Fact]
    public void Rows_PathWithAName_ShowsTheName()
    {
        var (_, list) = Create(null, Path("Wave"));

        Assert.Equal("Wave", list.Rows[1].Name);
    }

    [Fact]
    public void Rows_PathName_IsTranslatedAndFollowsALanguageChange()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Path"] = "Pfad" });
        var (_, list) = Create(localizer, Path());
        Assert.Equal("Path", list.Rows[1].Name);

        localizer.SetLanguage("de");

        Assert.Equal("Pfad", list.Rows[1].Name);
    }

    [Fact]
    public void Rows_PathInsideAGroup_IsListedBelowTheGroup()
    {
        var path = Path();
        var group = new GroupShape { Children = [path, new RectangleShape { Width = 1, Height = 1 }] };
        var (_, list) = Create(null, group);

        // The group is closed at first, so open it.
        list.SetExpanded(list.Rows.Single(r => r.IsGroup), true);

        var row = list.Rows.Single(r => r.Id == path.Id);
        Assert.Equal(ObjectRowType.Path, row.Type);
        Assert.Equal(2, row.Depth);
    }

    [Fact]
    public void Rows_RectangleAndEllipse_AreNotMistakenForPaths()
    {
        var (_, list) = Create(null, new RectangleShape { Width = 1, Height = 1 }, new EllipseShape { Width = 1, Height = 1 });

        Assert.All(list.Rows, row => Assert.False(row.IsPath));
    }
}
