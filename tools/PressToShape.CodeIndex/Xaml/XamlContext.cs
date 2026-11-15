using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Extraction;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>The state of resolving one XAML file: the shared resolvers, the file's class, and the edges and findings collected so far.</summary>
internal sealed class XamlContext
{
    private const string ControlType = "Avalonia.Controls.Control";

    private readonly HashSet<string> _edgeKeys = new(StringComparer.Ordinal);

    public XamlContext(
        XamlWorld world,
        XamlDocument document,
        Compilation compilation,
        IReadOnlySet<string> knownIds,
        XamlTypeResolver types,
        BindingResolver bindings,
        ResourceResolver resources,
        ViewLocatorConvention? convention)
    {
        World = world;
        Document = document;
        Compilation = compilation;
        KnownIds = knownIds;
        Types = types;
        Bindings = bindings;
        Resources = resources;
        Convention = convention;
        ClassSymbol = document.XClass is { } name ? compilation.GetTypeByMetadataName(name) : null;
        var id = ClassSymbol?.GetDocumentationCommentId();
        ClassId = id is not null && knownIds.Contains(id) ? id : null;
    }

    public XamlWorld World { get; }

    public XamlDocument Document { get; }

    public Compilation Compilation { get; }

    public IReadOnlySet<string> KnownIds { get; }

    public XamlTypeResolver Types { get; }

    public BindingResolver Bindings { get; }

    public ResourceResolver Resources { get; }

    public ViewLocatorConvention? Convention { get; }

    /// <summary>The class named by <c>x:Class</c> of the root, when it exists.</summary>
    public INamedTypeSymbol? ClassSymbol { get; }

    /// <summary>The id of the <c>x:Class</c> class, when it is declared in the solution.</summary>
    public string? ClassId { get; }

    public List<RelationshipRecord> Edges { get; } = [];

    public List<string> Findings { get; } = [];

    public string FileId => XamlIds.File(Document.Path);

    public string ElementId(XamlNode node) => XamlIds.Element(Document.Path, node.Path);

    /// <summary>Adds an edge once. The same edge from the same place is not repeated.</summary>
    public void AddEdge(RelationshipType type, string fromId, string toId, XamlNode at, Confidence confidence, string? reason)
    {
        if (_edgeKeys.Add($"{type}|{fromId}|{toId}|{at.Line}|{reason}"))
        {
            Edges.Add(new RelationshipRecord(type, fromId, toId, Document.Path, at.Line, confidence, reason));
        }
    }

    /// <summary>
    /// The data type that compiled bindings at the node use: the nearest <c>x:DataType</c> of the node or its ancestors, or the <c>DataType</c> of an enclosing data template.
    /// <c>InScope</c> is false when no data type is declared. <c>Symbol</c> is null when one is declared but cannot be resolved.
    /// </summary>
    public (bool InScope, INamedTypeSymbol? Symbol, string? Text) DataTypeAt(XamlNode node)
    {
        for (var current = node; current is not null; current = current.Parent)
        {
            var text = DataTypeText(current);
            if (text is not null)
            {
                return (true, Types.Resolve(Document, current, text).Symbol, text);
            }
        }

        return (false, null, null);
    }

    /// <summary>The type text of an <c>x:DataType</c> or of a data template's <c>DataType</c> declared on the node itself.</summary>
    public static string? DataTypeText(XamlNode node)
    {
        var attribute = node.Attribute("x:DataType") ?? (IsDataTemplate(node) ? node.Attribute("DataType") : null);
        if (attribute is null)
        {
            return null;
        }

        return attribute.Parsed is { Name: "x:Type" } type ? type.FirstPositionalText : attribute.Value;
    }

    public static bool IsDataTemplate(XamlNode node) =>
        !node.IsPropertyElement && XamlNames.Local(node.TypeText) is "DataTemplate" or "TreeDataTemplate" or "HierarchicalDataTemplate";

    /// <summary>A view class is a class of the solution that derives from <c>Avalonia.Controls.Control</c> or has a XAML file (<c>x:Class</c>).</summary>
    public bool IsViewClass(INamedTypeSymbol type)
    {
        var id = type.OriginalDefinition.GetDocumentationCommentId();
        if (id is null || !KnownIds.Contains(id))
        {
            return false;
        }

        return Derives(type, ControlType)
            || World.Documents.Any(d => d.XClass is { } name && Compilation.GetTypeByMetadataName(name) is { } c && SymbolEqualityComparer.Default.Equals(c, type));
    }

    public static bool Derives(INamedTypeSymbol type, string baseTypeName) => SymbolFormats.DerivesFrom(type, baseTypeName);
}
