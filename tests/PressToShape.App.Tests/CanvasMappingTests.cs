using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class CanvasMappingTests
{
    [Theory]
    [InlineData(100, 0, 100, 100)]
    [InlineData(150, 50, 100, 100)]
    [InlineData(150, 50, 200, 50)]
    [InlineData(150, 50, 50, 200)]
    [InlineData(10, 50, 100, -40)]
    public void ToDocument_KnownValues_ReturnsDocumentPosition(double screen, double origin, double zoom, double expected)
    {
        Assert.Equal(expected, CanvasMapping.ToDocument(screen, origin, zoom), precision: 9);
    }

    [Theory]
    [InlineData(100, 0, 100, 100)]
    [InlineData(100, 50, 100, 150)]
    [InlineData(100, 50, 200, 250)]
    [InlineData(100, 50, 50, 100)]
    [InlineData(-40, 50, 100, 10)]
    public void ToScreen_KnownValues_ReturnsScreenPosition(double value, double origin, double zoom, double expected)
    {
        Assert.Equal(expected, CanvasMapping.ToScreen(value, origin, zoom), precision: 9);
    }

    [Theory]
    [InlineData(25, 0)]
    [InlineData(50, 13.5)]
    [InlineData(100, 0)]
    [InlineData(100, 80)]
    [InlineData(300, 24)]
    public void ToScreenThenToDocument_AnyZoomAndOrigin_ReturnsTheStart(double zoom, double origin)
    {
        foreach (var value in new[] { -250.5, 0, 1, 333.25 })
        {
            var screen = CanvasMapping.ToScreen(value, origin, zoom);

            Assert.Equal(value, CanvasMapping.ToDocument(screen, origin, zoom), precision: 9);
        }
    }
}
