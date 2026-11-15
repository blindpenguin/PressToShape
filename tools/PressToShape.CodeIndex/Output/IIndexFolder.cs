using System.Collections.Generic;
using PressToShape.CodeIndex.Query;

namespace PressToShape.CodeIndex.Output;

/// <summary>The index folder as the command line uses it: read the files, write new ones, and remove files a run no longer produces.</summary>
internal interface IIndexFolder : IIndexFileSink, IIndexFileSource
{
    /// <summary>Deletes the files that were not written since this object was created. Returns their paths.</summary>
    IReadOnlyList<string> RemoveStale();
}
