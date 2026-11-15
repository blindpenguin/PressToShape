using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace PressToShape.CodeIndex.Tests;

public class ProjectReferenceTests
{
    [Fact]
    public void SrcProjects_References_NoToolOrRoslynOrLocator()
    {
        var csprojFiles = FindProjectFiles("src");

        Assert.NotEmpty(csprojFiles);
        foreach (var file in csprojFiles)
        {
            var document = XDocument.Load(file);
            var projectReferences = document.Descendants("ProjectReference")
                .Select(e => (string?)e.Attribute("Include") ?? string.Empty);
            var packageReferences = document.Descendants("PackageReference")
                .Select(e => (string?)e.Attribute("Include") ?? string.Empty);

            Assert.DoesNotContain(projectReferences, r => r.Contains("PressToShape.CodeIndex", StringComparison.Ordinal));
            Assert.DoesNotContain(packageReferences, IsToolPackage);
        }
    }

    [Fact]
    public void AllProjects_PackageReference_HasNoVersionAttribute()
    {
        var csprojFiles = FindProjectFiles("src").Concat(FindProjectFiles("tests")).Concat(FindProjectFiles("tools")).ToArray();

        Assert.NotEmpty(csprojFiles);
        foreach (var file in csprojFiles)
        {
            var withVersion = XDocument.Load(file).Descendants("PackageReference")
                .Where(e => e.Attribute("Version") is not null || e.Attribute("VersionOverride") is not null)
                .Select(e => (string?)e.Attribute("Include"))
                .ToArray();

            Assert.True(withVersion.Length == 0, Path.GetFileName(file) + " has a version on: " + string.Join(", ", withVersion));
        }
    }

    private static bool IsToolPackage(string name) =>
        name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal)
        || name.StartsWith("Microsoft.Build", StringComparison.Ordinal);

    private static string[] FindProjectFiles(string folder)
    {
        var path = RepoPaths.Combine(folder);
        return Directory.Exists(path)
            ? Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories)
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                            && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .ToArray()
            : [];
    }
}
