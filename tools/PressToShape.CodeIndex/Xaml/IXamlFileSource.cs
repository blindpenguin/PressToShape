using System.Collections.Generic;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Lists and reads XAML files by repo-relative path. Production reads the disk, tests use memory.</summary>
internal interface IXamlFileSource
{
    /// <summary>The <c>.axaml</c> files below a project folder (repo-relative), sorted ordinally, without <c>bin</c> and <c>obj</c>.</summary>
    IReadOnlyList<string> List(string projectDirectory);

    string Read(string repoRelativePath);
}
