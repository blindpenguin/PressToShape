namespace PressToShape.CodeIndex.Model;

/// <summary>The version of the index file format. Change it for every breaking change of the JSON.</summary>
internal static class IndexSchema
{
    public const int Version = 2;

    /// <summary>Written to the manifest. A constant, so the output does not change with the build or the commit.</summary>
    public const string ToolVersion = "1.0.0";
}
