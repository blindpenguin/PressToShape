using System.Linq;
using PressToShape.App.Workspace;
using PressToShape.Core.History;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class RecentColorsTests
{
    private static Color C(byte value) => Color.FromRgb(value, 0, 0);

    [Fact]
    public void Add_NewColors_PutsMostRecentFirst()
    {
        var recent = new RecentColors();

        recent.Add(C(1));
        recent.Add(C(2));

        Assert.Equal([C(2), C(1)], recent.Items.ToArray());
    }

    [Fact]
    public void Add_ColorAlreadyListed_MovesItToFrontWithoutDuplicate()
    {
        var recent = new RecentColors();
        recent.Add(C(1));
        recent.Add(C(2));

        recent.Add(C(1));

        Assert.Equal([C(1), C(2)], recent.Items.ToArray());
    }

    [Fact]
    public void Add_MoreThanMaxCount_DropsTheOldest()
    {
        var recent = new RecentColors();
        for (byte i = 1; i <= RecentColors.MaxCount + 1; i++)
        {
            recent.Add(C(i));
        }

        Assert.Equal(RecentColors.MaxCount, recent.Items.Count);
        Assert.DoesNotContain(C(1), recent.Items);
        Assert.Equal(C(RecentColors.MaxCount + 1), recent.Items[0]);
    }

    [Fact]
    public void Add_SameRgbWithOtherAlpha_IsAnotherColor()
    {
        var recent = new RecentColors();

        recent.Add(C(1));
        recent.Add(C(1) with { A = 10 });

        Assert.Equal(2, recent.Items.Count);
    }

    [Theory]
    [InlineData(HistoryStepType.AddSwatch, "Add swatch")]
    [InlineData(HistoryStepType.RemoveSwatch, "Remove swatch")]
    public void MessageId_SwatchSteps_HaveNames(HistoryStepType type, string expected) =>
        Assert.Equal(expected, HistoryStepNames.MessageId(type));
}
