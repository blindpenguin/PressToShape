using System.Collections.Generic;

namespace PressToShape.CodeIndex.Query;

/// <summary>Reads index files by their path relative to the index folder. Returns null when the file does not exist.</summary>
internal interface IIndexFileSource
{
    byte[]? Read(string relativePath);

    /// <summary>Every file below the index folder, as sorted relative paths with <c>/</c>.</summary>
    IReadOnlyList<string> ListFiles();
}
