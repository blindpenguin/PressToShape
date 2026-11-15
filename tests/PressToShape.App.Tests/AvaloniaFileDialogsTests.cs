using System;
using Avalonia.Platform.Storage;
using PressToShape.App.Services;
using Xunit;

namespace PressToShape.App.Tests;

public class AvaloniaFileDialogsTests
{
    [Fact]
    public void ChooseReference_WithALocalPath_ReturnsThePathAndDoesNotRegister()
    {
        var registered = false;

        var result = AvaloniaFileDialogs.ChooseReference("/work/a.p2s", () =>
        {
            registered = true;
            return "storage:x/a.p2s";
        });

        Assert.Equal("/work/a.p2s", result);
        Assert.False(registered);
    }

    [Fact]
    public void ChooseReference_WithoutAPath_RegistersAndReturnsTheReference()
    {
        var result = AvaloniaFileDialogs.ChooseReference(null, () => "storage:x/a.p2s");

        Assert.Equal("storage:x/a.p2s", result);
    }

    [Fact]
    public void ChooseReference_WithoutAPathAndWithoutRegistry_ReturnsNull()
    {
        Assert.Null(AvaloniaFileDialogs.ChooseReference(null, null));
    }

    [Theory]
    [InlineData("plan.p2s", "p2s")]
    [InlineData("PLAN.P2S", "p2s")]
    [InlineData("Untitled.svg", "svg")]
    public void BuildSaveOptions_SuggestedNameWithTheEnding_PassesNoDefaultExtension(string suggested, string extension)
    {
        var options = AvaloniaFileDialogs.BuildSaveOptions("Save", suggested, extension, new FilePickerFileType("Type"));

        Assert.Null(options.DefaultExtension);
        Assert.Equal(suggested, options.SuggestedFileName);
    }

    [Theory]
    [InlineData("plan", "p2s")]
    [InlineData("plan.txt", "p2s")]
    [InlineData("planp2s", "p2s")]
    public void BuildSaveOptions_SuggestedNameWithoutTheEnding_KeepsTheDefaultExtension(string suggested, string extension)
    {
        var options = AvaloniaFileDialogs.BuildSaveOptions("Save", suggested, extension, new FilePickerFileType("Type"));

        Assert.Equal(extension, options.DefaultExtension);
    }

    [Fact]
    public void BuildSaveOptions_SetsTitleOverwritePromptAndTheFileType()
    {
        var type = new FilePickerFileType("Drawing") { Patterns = ["*.p2s"] };

        var options = AvaloniaFileDialogs.BuildSaveOptions("Save drawing", "a.p2s", "p2s", type);

        Assert.Equal("Save drawing", options.Title);
        Assert.True(options.ShowOverwritePrompt);
        Assert.Same(type, Assert.Single(options.FileTypeChoices!));
    }

    [Fact]
    public void Constructor_NullOwnerOrLocalizer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AvaloniaFileDialogs(null!, FakeLocalizer.Create()));
        Assert.Throws<ArgumentNullException>(() => new AvaloniaFileDialogs(() => null, null!));
    }
}
