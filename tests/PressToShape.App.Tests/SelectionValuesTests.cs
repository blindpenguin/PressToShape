using System;
using System.Collections.Generic;
using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionValuesTests
{
    private static RectangleShape Rect(double x, double y, double w, double h, Appearance? appearance = null, double rotation = 0) =>
        new() { X = x, Y = y, Width = w, Height = h, Rotation = rotation, Appearance = appearance ?? Appearance.Default };

    [Fact]
    public void From_EmptyList_IsNull()
    {
        Assert.Null(SelectionValues.From([]));
    }

    [Fact]
    public void From_OneShape_GivesItsBoxRotationAndAppearance()
    {
        var appearance = Appearance.Default with { Fill = Color.FromRgb(1, 2, 3), StrokeWidth = 4, Opacity = 0.5 };

        var values = SelectionValues.From([Rect(10, 20, 30, 40, appearance, rotation: 25)])!;

        Assert.Equal((10, 20, 30, 40, 25), (values.X, values.Y, values.Width, values.Height, values.Rotation));
        Assert.True(values.IsSingle);
        Assert.Equal(new Mixed<Color?>(Color.FromRgb(1, 2, 3), false), values.Fill);
        Assert.Equal(new Mixed<double>(4, false), values.StrokeWidth);
        Assert.Equal(new Mixed<double>(0.5, false), values.Opacity);
    }

    [Fact]
    public void From_RotatedShape_UsesTheUnturnedBox()
    {
        var values = SelectionValues.From([Rect(10, 20, 100, 40, rotation: 90)])!;

        Assert.Equal((10, 20, 100, 40), (values.X, values.Y, values.Width, values.Height));
    }

    [Fact]
    public void From_SeveralShapes_GivesTheBoxAroundThemAndRotationZero()
    {
        var values = SelectionValues.From([Rect(0, 0, 10, 10), Rect(100, 50, 20, 20, rotation: 90)])!;

        Assert.False(values.IsSingle);
        Assert.Equal(0, values.Rotation);
        Assert.Equal(0, values.X, 6);
        Assert.Equal(0, values.Y, 6);
        Assert.Equal(120, values.Width, 6);
        Assert.Equal(70, values.Height, 6);
    }

    [Fact]
    public void From_SeveralEqualShapes_IsNotMixed()
    {
        var values = SelectionValues.From([Rect(0, 0, 10, 10), Rect(50, 0, 10, 10)])!;

        Assert.False(values.Fill.IsMixed);
        Assert.False(values.Stroke.IsMixed);
        Assert.False(values.StrokeWidth.IsMixed);
        Assert.False(values.Opacity.IsMixed);
        Assert.Equal(Appearance.Default.Fill, values.Fill.Value);
    }

    [Fact]
    public void From_DifferentAppearances_AreMixedPerProperty()
    {
        var a = Appearance.Default with { Fill = Color.FromRgb(1, 1, 1), StrokeWidth = 2 };
        var b = Appearance.Default with { Fill = null, StrokeWidth = 2, Opacity = 0.3 };

        var values = SelectionValues.From([Rect(0, 0, 1, 1, a), Rect(5, 5, 1, 1, b)])!;

        Assert.True(values.Fill.IsMixed);
        Assert.Equal(Color.FromRgb(1, 1, 1), values.Fill.Value);
        Assert.False(values.Stroke.IsMixed);
        Assert.False(values.StrokeWidth.IsMixed);
        Assert.True(values.Opacity.IsMixed);
    }

    [Fact]
    public void From_NoFillOnAllShapes_IsNullAndNotMixed()
    {
        var none = Appearance.Default with { Fill = null };

        var values = SelectionValues.From([Rect(0, 0, 1, 1, none), Rect(5, 5, 1, 1, none)])!;

        Assert.Equal(new Mixed<Color?>(null, false), values.Fill);
    }

    [Fact]
    public void Mixed_Of_HandlesEmptyOneAndManyValues()
    {
        Assert.Equal(default, Mixed<int>.Of([]));
        Assert.Equal(new Mixed<int>(3, false), Mixed<int>.Of([3]));
        Assert.Equal(new Mixed<int>(3, false), Mixed<int>.Of([3, 3, 3]));
        Assert.Equal(new Mixed<int>(3, true), Mixed<int>.Of([3, 3, 4]));
        Assert.Throws<ArgumentNullException>(() => Mixed<int>.Of((IEnumerable<int>)null!));
    }

    [Fact]
    public void From_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SelectionValues.From(null!));
    }
}
