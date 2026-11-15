using System.Collections.Generic;

namespace PressToShape.CodeIndex.Model;

/// <summary>The top-level file of the index: versions, projects, counts and load problems.</summary>
internal sealed record IndexManifest(
    int SchemaVersion,
    string ToolVersion,
    IReadOnlyList<ManifestProject> Projects,
    int SymbolCount,
    int RelationshipCount,
    IReadOnlyList<LoadProblem> Warnings);
