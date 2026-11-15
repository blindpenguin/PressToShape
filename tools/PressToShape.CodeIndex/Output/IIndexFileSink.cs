namespace PressToShape.CodeIndex.Output;

/// <summary>Receives the files of one index run. Paths are relative to the index folder and use <c>/</c>.</summary>
internal interface IIndexFileSink
{
    void Write(string relativePath, byte[] content);
}
