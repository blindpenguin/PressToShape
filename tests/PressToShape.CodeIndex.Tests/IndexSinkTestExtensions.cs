using PressToShape.CodeIndex.Output;

namespace PressToShape.CodeIndex.Tests;

internal static class IndexSinkTestExtensions
{
    /// <summary>A copy of the index in which one file has other content, to test damaged or old index files. A sink does not allow a second write of a path.</summary>
    public static InMemoryFileSink WithReplaced(this InMemoryFileSink source, string path, byte[] content)
    {
        var copy = new InMemoryFileSink();
        foreach (var (file, bytes) in source.Files)
        {
            copy.Write(file, file == path ? content : bytes);
        }

        return copy;
    }
}
