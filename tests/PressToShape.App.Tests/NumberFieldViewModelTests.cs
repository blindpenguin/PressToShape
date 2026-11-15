using System;
using System.Collections.Generic;
using PressToShape.App.Localization;
using PressToShape.App.ViewModels.Editor;
using Xunit;

namespace PressToShape.App.Tests;

public class NumberFieldViewModelTests
{
    private static (NumberFieldViewModel Field, List<double> Applied, Localizer Localizer) Create(string language = "en", double min = -1000, double max = 1000, double? value = 10)
    {
        var localizer = FakeLocalizer.Create(language);
        var applied = new List<double>();
        var field = new NumberFieldViewModel(localizer, min, max, applied.Add);
        field.SetValue(value);
        return (field, applied, localizer);
    }

    [Theory]
    [InlineData("en", "1.5", 1.5)]
    [InlineData("en", "  12 ", 12)]
    [InlineData("en", "-3.25", -3.25)]
    [InlineData("de", "1,5", 1.5)]
    [InlineData("de", "1.5", 1.5)]
    [InlineData("de", "-0,25", -0.25)]
    public void Commit_ValidText_IsApplied(string language, string text, double expected)
    {
        var (field, applied, _) = Create(language);

        field.Text = text;
        field.Commit();

        Assert.Equal(new[] { expected }, applied);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("1,5")]
    [InlineData("1,000")]
    [InlineData("1e3")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("1.2.3")]
    [InlineData("5%")]
    public void Commit_InvalidTextInEnglish_IsRefusedAndRevertsTheText(string text)
    {
        var (field, applied, _) = Create();

        field.Text = text;
        field.Commit();

        Assert.Empty(applied);
        Assert.Equal("10", field.Text);
    }

    [Theory]
    [InlineData("1.000,5")]
    [InlineData("1 000")]
    [InlineData("NaN")]
    public void Commit_InvalidTextInGerman_IsRefused(string text)
    {
        var (field, applied, _) = Create("de");

        field.Text = text;
        field.Commit();

        Assert.Empty(applied);
        Assert.Equal("10", field.Text);
    }

    [Fact]
    public void Commit_OutsideTheRange_IsRefused_AndTheLimitsThemselvesAreAccepted()
    {
        var (field, applied, _) = Create(min: 0, max: 100);

        field.Text = "100.5";
        field.Commit();
        field.Text = "-0.5";
        field.Commit();
        Assert.Empty(applied);

        field.Text = "100";
        field.Commit();
        field.Text = "0";
        field.Commit();
        Assert.Equal(new[] { 100.0, 0 }, applied);
    }

    [Fact]
    public void Commit_EqualValue_DoesNotCallApply_AndNormalizesTheText()
    {
        var (field, applied, _) = Create(value: 2.5);

        field.Text = "2.50";
        field.Commit();
        field.Text = "2.5004";
        field.Commit();

        Assert.Empty(applied);
        Assert.Equal("2.5", field.Text);
    }

    [Fact]
    public void Commit_AfterApply_ShowsWhatTheDocumentNowHas()
    {
        var localizer = FakeLocalizer.Create();
        NumberFieldViewModel? field = null;
        field = new NumberFieldViewModel(localizer, -1000, 1000, v => field!.SetValue(Math.Round(v, 3)));
        field.SetValue(10);

        field.Text = "7.123456";
        field.Commit();

        Assert.Equal("7.123", field.Text);
    }

    [Fact]
    public void Commit_WhenTheDocumentIgnoresTheValue_ShowsTheUnchangedValueAgain()
    {
        var (field, applied, _) = Create();

        field.Text = "7";
        field.Commit();

        Assert.Equal(new[] { 7.0 }, applied);
        Assert.Equal("10", field.Text);
    }

    [Fact]
    public void Revert_RestoresTheShownValue()
    {
        var (field, _, _) = Create(value: 4);
        field.Text = "99";

        field.Revert();

        Assert.Equal("4", field.Text);
    }

    [Theory]
    [InlineData(2, "2")]
    [InlineData(2.5, "2.5")]
    [InlineData(0.333333, "0.333")]
    [InlineData(0.0004, "0")]
    [InlineData(-0.0004, "0")]
    [InlineData(-12.3456, "-12.346")]
    [InlineData(1234567.891, "1234567.891")]
    [InlineData(0, "0")]
    public void SetValue_FormatsWithAtMostThreeDecimalsAndNoTrailingZeros(double value, string expected)
    {
        var (field, _, _) = Create(value: value);

        Assert.Equal(expected, field.Text);
    }

    [Fact]
    public void Format_UsesTheCultureOfTheLanguage_AndAChangeRefreshesTheText()
    {
        var (field, _, localizer) = Create(value: 2.5);
        Assert.Equal("2.5", field.Text);
        var raised = new List<string?>();
        field.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        localizer.SetLanguage("de");

        Assert.Equal("2,5", field.Text);
        Assert.Contains(nameof(NumberFieldViewModel.Text), raised);
    }

    [Fact]
    public void Mixed_ShowsAnEmptyText_AndAnyValidInputIsApplied()
    {
        var (field, applied, _) = Create();
        field.SetValue(10, mixed: true);

        Assert.Equal(string.Empty, field.Text);
        Assert.True(field.IsMixed);
        Assert.True(field.IsEnabled);

        field.Text = "10";
        field.Commit();

        Assert.Equal(new[] { 10.0 }, applied);
    }

    [Fact]
    public void Mixed_EmptyTextStaysEmptyAndNothingIsApplied()
    {
        var (field, applied, _) = Create();
        field.SetValue(10, mixed: true);

        field.Commit();

        Assert.Empty(applied);
        Assert.Equal(string.Empty, field.Text);
    }

    [Fact]
    public void NullValue_DisablesTheField_AndCommitDoesNothing()
    {
        var (field, applied, _) = Create();

        field.SetValue(null);
        field.Text = "5";
        field.Commit();

        Assert.False(field.IsEnabled);
        Assert.Empty(applied);
        Assert.Equal(string.Empty, field.Text);
    }

    [Fact]
    public void WhileEditing_NewValuesDoNotReplaceTheTypedText_UntilRevertedOrCommitted()
    {
        var (field, _, localizer) = Create(value: 1);
        field.IsEditing = true;
        field.Text = "12";

        field.SetValue(5);
        localizer.SetLanguage("de");

        Assert.Equal("12", field.Text);

        field.IsEditing = false;
        field.Revert();
        Assert.Equal("5", field.Text);
    }

    [Fact]
    public void Constructor_RejectsBadArguments()
    {
        var localizer = FakeLocalizer.Create();

        Assert.Throws<ArgumentNullException>(() => new NumberFieldViewModel(null!, 0, 1, _ => { }));
        Assert.Throws<ArgumentNullException>(() => new NumberFieldViewModel(localizer, 0, 1, null!));
        Assert.Throws<ArgumentException>(() => new NumberFieldViewModel(localizer, 5, 1, _ => { }));
        Assert.Throws<ArgumentException>(() => new NumberFieldViewModel(localizer, double.NaN, 1, _ => { }));
    }
}
