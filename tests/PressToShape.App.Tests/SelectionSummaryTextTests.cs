using System.Collections.Generic;
using PressToShape.App.Workspace;
using Xunit;

namespace PressToShape.App.Tests;

public class SelectionSummaryTextTests
{
    [Theory]
    [InlineData(0, "Nothing selected")]
    [InlineData(1, "1 selected")]
    [InlineData(2, "2 selected")]
    [InlineData(25, "25 selected")]
    public void Format_English_GivesTheCountText(int count, string expected)
    {
        var localizer = FakeLocalizer.Create();

        Assert.Equal(expected, SelectionSummaryText.Format(localizer, count));
    }

    [Fact]
    public void Format_German_UsesTheTranslations()
    {
        var localizer = FakeLocalizer.Create("de", new Dictionary<string, string>
        {
            ["Nothing selected"] = "Nichts ausgewählt",
            ["1 selected"] = "1 ausgewählt",
            ["{0} selected"] = "{0} ausgewählt",
        });

        Assert.Equal("Nichts ausgewählt", SelectionSummaryText.Format(localizer, 0));
        Assert.Equal("1 ausgewählt", SelectionSummaryText.Format(localizer, 1));
        Assert.Equal("7 ausgewählt", SelectionSummaryText.Format(localizer, 7));
    }

    [Fact]
    public void Format_ChangesWithTheLanguageOfTheLocalizer()
    {
        var localizer = FakeLocalizer.Create("en", new Dictionary<string, string> { ["Nothing selected"] = "Nichts ausgewählt" });
        Assert.Equal("Nothing selected", SelectionSummaryText.Format(localizer, 0));

        localizer.SetLanguage("de");

        Assert.Equal("Nichts ausgewählt", SelectionSummaryText.Format(localizer, 0));
    }
}
