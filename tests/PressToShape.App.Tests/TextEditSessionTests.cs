using PressToShape.App.Workspace;
using PressToShape.Core.Shapes;
using Xunit;

namespace PressToShape.App.Tests;

public class TextEditSessionTests
{
    private static TextShape Text(string content) => new() { Text = content, FontSize = 12 };

    [Fact]
    public void Outcome_NewTextWithContent_IsCreate()
    {
        var session = new TextEditSession(Text(string.Empty), isNew: true);

        session.SetContent("Hello");

        Assert.Equal(TextEditOutcome.Create, session.Outcome);
        Assert.Equal("Hello", session.Text.Text);
    }

    [Fact]
    public void Outcome_NewTextThatStaysEmpty_IsNone()
    {
        var session = new TextEditSession(Text(string.Empty), isNew: true);

        Assert.Equal(TextEditOutcome.None, session.Outcome);
    }

    [Fact]
    public void Outcome_NewTextTypedAndThenErased_IsNone()
    {
        var session = new TextEditSession(Text(string.Empty), isNew: true);
        session.SetContent("a");
        session.SetContent(string.Empty);

        Assert.Equal(TextEditOutcome.None, session.Outcome);
    }

    [Fact]
    public void Outcome_ExistingTextUnchanged_IsNone()
    {
        var session = new TextEditSession(Text("Hello"), isNew: false);

        Assert.Equal(TextEditOutcome.None, session.Outcome);
    }

    [Fact]
    public void Outcome_ExistingTextChangedAndChangedBack_IsNone()
    {
        var session = new TextEditSession(Text("Hello"), isNew: false);
        session.SetContent("Hello!");
        session.SetContent("Hello");

        Assert.Equal(TextEditOutcome.None, session.Outcome);
    }

    [Fact]
    public void Outcome_ExistingTextWithOtherContent_IsUpdate()
    {
        var session = new TextEditSession(Text("Hello"), isNew: false);

        session.SetContent("Bye");

        Assert.Equal(TextEditOutcome.Update, session.Outcome);
    }

    [Fact]
    public void Outcome_ExistingTextErased_IsDelete()
    {
        var session = new TextEditSession(Text("Hello"), isNew: false);

        session.SetContent(string.Empty);

        Assert.Equal(TextEditOutcome.Delete, session.Outcome);
    }

    [Fact]
    public void Outcome_WhiteSpaceOnly_CountsAsContent()
    {
        var session = new TextEditSession(Text(string.Empty), isNew: true);

        session.SetContent("  ");

        Assert.Equal(TextEditOutcome.Create, session.Outcome);
    }

    [Fact]
    public void SetBox_KeepsTheContentAndTheId()
    {
        var text = Text("Hello");
        var session = new TextEditSession(text, isNew: false);

        session.SetBox(1, 2, 30, 40);

        Assert.Equal(text.Id, session.TextId);
        Assert.Equal("Hello", session.Text.Text);
        Assert.Equal((1d, 2d, 30d, 40d), (session.Text.X, session.Text.Y, session.Text.Width, session.Text.Height));
        Assert.Equal(TextEditOutcome.None, session.Outcome);
    }
}
