using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PressToShape.CodeIndex.Cli;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

/// <summary>The agent rule file is documentation that must stay true: it names real commands and is scoped to the UI files. These tests read it from the repository.</summary>
public class RuleFileTests
{
    private static readonly string[] Commands = ["find", "show", "chain", "refs", "check", "generate"];

    private static string Rule() => File.ReadAllText(RepoPaths.Combine(".claude/rules/code-index.md"));

    [Fact]
    public void Rule_HasPathsFrontmatterForTheUiFiles()
    {
        var rule = Rule();

        Assert.StartsWith("---\npaths:\n", rule, StringComparison.Ordinal);
        var frontmatter = rule[..rule.IndexOf("\n---", 3, StringComparison.Ordinal)];
        foreach (var path in new[] { "src/**/*.axaml", "src/**/*.axaml.cs", "src/PressToShape.App/ViewModels/**", "src/PressToShape.App/Views/**", "src/PressToShape.App/Controls/**", "src/PressToShape.App/Styles/**" })
        {
            Assert.Contains($"  - \"{path}\"", frontmatter, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Rule_NamesTheCommandsAndEachOneIsInTheHelpText()
    {
        var rule = Rule();

        foreach (var command in Commands)
        {
            Assert.Contains($"`{command}", rule, StringComparison.Ordinal);
            Assert.Matches(new Regex($@"^\s+{command}\b", RegexOptions.Multiline), CliText.Usage);
        }
    }

    [Fact]
    public void Rule_NamesOnlyCommandsThatExist()
    {
        var named = Regex.Matches(Rule(), @"`(find|show|chain|refs|impls|derived|deps|check|generate|[a-z]+) [<`]").Select(m => m.Groups[1].Value).Distinct();

        foreach (var command in named)
        {
            Assert.Matches(new Regex($@"^\s+{command}\b", RegexOptions.Multiline), CliText.Usage);
        }
    }

    [Fact]
    public void Rule_HasTheSevenStepsInOrder()
    {
        var steps = Regex.Matches(Rule(), @"^(\d)\. ", RegexOptions.Multiline).Select(m => m.Groups[1].Value);

        Assert.Equal(["1", "2", "3", "4", "5", "6", "7"], steps);
    }

    [Fact]
    public void Rule_ForbidsAssumingFromNamesAndExplainsInferredLinks()
    {
        var rule = Rule();

        Assert.Contains("Never assume a view model from matching file or class names", rule, StringComparison.Ordinal);
        Assert.Contains("`x:DataType`", rule, StringComparison.Ordinal);
        Assert.Contains("`inferred` is a hypothesis", rule, StringComparison.Ordinal);
    }

    [Fact]
    public void Rule_HasNoAbsolutePathsAndNoWordKind()
    {
        var rule = Rule();

        Assert.DoesNotContain("/home/", rule, StringComparison.Ordinal);
        Assert.DoesNotContain("kind", rule, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ProjectOverview_PointsToTheRuleAndReadmeDescribesChain()
    {
        var overview = File.ReadAllText(RepoPaths.Combine(".claude/rules/project-overview.md"));
        var readme = File.ReadAllText(RepoPaths.Combine("README.md"));

        Assert.Contains("`code-index.md`", overview, StringComparison.Ordinal);
        Assert.Contains("docs/index/", overview, StringComparison.Ordinal);
        Assert.Contains("-- chain ", readme, StringComparison.Ordinal);
        Assert.Contains("docs/index/markdown/avalonia/", readme, StringComparison.Ordinal);
    }
}
