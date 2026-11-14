using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.Core.Tests.Shapes;

public class ColorTests
{
    [Fact]
    public void FromRgb_AnyChannels_SetsAlphaTo255()
    {
        var color = Color.FromRgb(10, 20, 30);

        Assert.Equal(255, color.A);
    }

    [Fact]
    public void Constructor_AllChannels_KeepsEachValue()
    {
        var color = new Color(1, 2, 3, 4);

        Assert.Equal((1, 2, 3, 4), (color.R, color.G, color.B, color.A));
    }

    [Fact]
    public void Equals_SameChannels_IsEqual()
    {
        Assert.Equal(new Color(1, 2, 3, 4), new Color(1, 2, 3, 4));
    }

    [Fact]
    public void Equals_DifferentAlpha_IsNotEqual()
    {
        Assert.NotEqual(new Color(1, 2, 3, 4), new Color(1, 2, 3, 5));
    }
}
