using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Reads a XAML file into a <see cref="XamlDocument"/>. DTDs and external entities are prohibited. Malformed files give a problem, not an exception.</summary>
internal static class XamlReader
{
    private const int MaxDepth = 256;
    private const long MaxCharacters = 10_000_000;

    private static readonly HashSet<string> MetadataNames = new(StringComparer.Ordinal)
    {
        "x:Class", "x:DataType", "DataType", "TargetType", "Selector", "x:Key", "x:Name", "Name", "Source", "Property",
    };

    public static XamlReadResult Read(string path, string project, string text)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(project);
        ArgumentNullException.ThrowIfNull(text);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            MaxCharactersInDocument = MaxCharacters,
        };

        try
        {
            using var reader = XmlReader.Create(new StringReader(text.TrimStart('﻿')), settings);
            var document = XDocument.Load(reader, LoadOptions.SetLineInfo);
            var root = document.Root ?? throw new XmlException("The file has no root element.");

            var nodes = new List<XamlNode>();
            var findings = new List<string>();
            Convert(root, null, "/" + TypeText(root), nodes, 0);
            if (nodes.Count > 0)
            {
                CheckDuplicateNames(nodes, findings);
            }

            return new XamlReadResult(new XamlDocument(path, project, nodes[0], nodes, findings), null);
        }
        catch (XmlException ex)
        {
            return Failed(path, project, ex.LineNumber, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Failed(path, project, 0, ex.Message);
        }
    }

    private static XamlReadResult Failed(string path, string project, int line, string message)
    {
        var where = line > 0 ? $"{path}:{line}" : path;
        return new XamlReadResult(null, new LoadProblem(project, $"{where}: {message}", false));
    }

    private static XamlNode Convert(XElement element, XamlNode? parent, string path, List<XamlNode> nodes, int depth)
    {
        if (depth > MaxDepth)
        {
            throw new InvalidOperationException($"The elements are nested more than {MaxDepth} levels deep.");
        }

        var typeText = TypeText(element);
        var (ownerText, propertyName) = SplitPropertyElement(typeText);
        var node = new XamlNode(nodes.Count, parent, ownerText, propertyName, NameOf(element), Line(element), path);
        nodes.Add(node);

        foreach (var attribute in element.Attributes())
        {
            if (attribute.IsNamespaceDeclaration)
            {
                node.Namespaces[attribute.Name.Namespace == XNamespace.Xmlns ? attribute.Name.LocalName : string.Empty] = attribute.Value;
                continue;
            }

            if (Classify(PrefixedName(attribute.Name, element), attribute) is { } kept)
            {
                node.Attributes.Add(kept);
            }
        }

        var counters = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var child in element.Elements())
        {
            var childType = TypeText(child);
            counters[childType] = counters.GetValueOrDefault(childType) + 1;
            node.Children.Add(Convert(child, node, $"{path}/{childType}[{counters[childType]}]", nodes, depth + 1));
        }

        return node;
    }

    private static XamlAttribute? Classify(string name, XAttribute attribute)
    {
        var value = attribute.Value;
        var line = Line(attribute);
        if (value.StartsWith('{') && !value.StartsWith("{}", StringComparison.Ordinal))
        {
            MarkupExtensionParser.TryParse(value, out var parsed);
            return new XamlAttribute(name, value, line, XamlAttributeRole.MarkupExtension, parsed);
        }

        if (MetadataNames.Contains(name))
        {
            return new XamlAttribute(name, value, line, XamlAttributeRole.Metadata, null);
        }

        return IsHandlerCandidate(name, value) ? new XamlAttribute(name, value, line, XamlAttributeRole.HandlerCandidate, null) : null;
    }

    /// <summary>An unprefixed, non-attached attribute whose value is a plain identifier.</summary>
    private static bool IsHandlerCandidate(string name, string value) =>
        !name.Contains(':', StringComparison.Ordinal) && !name.Contains('.', StringComparison.Ordinal)
        && value.Length > 0 && (char.IsLetter(value[0]) || value[0] == '_')
        && value.All(c => char.IsLetterOrDigit(c) || c == '_');

    private static void CheckDuplicateNames(List<XamlNode> nodes, List<string> findings)
    {
        foreach (var group in nodes.Where(n => n.Name is not null).GroupBy(n => n.Name!, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            findings.Add($"The name '{group.Key}' is declared {group.Count()} times (lines {string.Join(", ", group.Select(n => n.Line))}).");
        }
    }

    private static (string Owner, string? Property) SplitPropertyElement(string typeText)
    {
        var dot = typeText.IndexOf('.', StringComparison.Ordinal);
        return dot < 0 ? (typeText, null) : (typeText[..dot], typeText[(dot + 1)..]);
    }

    private static string? NameOf(XElement element) =>
        element.Attributes().FirstOrDefault(a => a.Name.LocalName == "Name" && (a.Name.Namespace == XNamespace.None || a.Name.NamespaceName.EndsWith("/winfx/2006/xaml", StringComparison.Ordinal)))?.Value;

    private static string TypeText(XElement element) => PrefixedName(element.Name, element);

    private static string PrefixedName(XName name, XElement scope)
    {
        var prefix = name.Namespace == XNamespace.None ? null : scope.GetPrefixOfNamespace(name.Namespace);
        return string.IsNullOrEmpty(prefix) ? name.LocalName : $"{prefix}:{name.LocalName}";
    }

    private static int Line(object node) => node is IXmlLineInfo info && info.HasLineInfo() ? info.LineNumber : 0;
}
