using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Reads the structure of resource dictionaries: own entries, theme variants and merged dictionaries.</summary>
internal static class ResourceDictionaries
{
    /// <summary>The dictionary behind a <c>*.Resources</c> property element: its single <c>ResourceDictionary</c> child, or the property element itself when entries sit directly in it.</summary>
    public static XamlNode DictionaryOf(XamlNode resourcesProperty) =>
        resourcesProperty.Children is [var only] && XamlNames.IsType(only, "ResourceDictionary") ? only : resourcesProperty;

    /// <summary>The dictionary of a document that is included as resources or styles: a <c>ResourceDictionary</c> root, or the <c>Resources</c> of a <c>Styles</c> root.</summary>
    public static XamlNode? DictionaryOfDocument(XamlDocument document)
    {
        var root = document.Root;
        if (XamlNames.IsType(root, "ResourceDictionary"))
        {
            return root;
        }

        var resources = root.Children.FirstOrDefault(c => c.IsPropertyElement && c.PropertyName == "Resources");
        return resources is null ? null : DictionaryOf(resources);
    }

    /// <summary>Entries with an <c>x:Key</c> directly in the dictionary.</summary>
    public static IEnumerable<XamlNode> OwnEntries(XamlNode dictionary) =>
        dictionary.Children.Where(c => !c.IsPropertyElement && c.AttributeValue("x:Key") is not null);

    /// <summary>The theme variant dictionaries (<c>ResourceDictionary.ThemeDictionaries</c>), each with its key (Light, Dark, Default).</summary>
    public static IEnumerable<(string Variant, XamlNode Dictionary)> ThemeDictionaries(XamlNode dictionary) =>
        dictionary.Children
            .Where(c => XamlNames.IsProperty(c, "ResourceDictionary", "ThemeDictionaries"))
            .SelectMany(c => c.Children)
            .Where(c => XamlNames.IsType(c, "ResourceDictionary") && c.AttributeValue("x:Key") is not null)
            .Select(c => (c.AttributeValue("x:Key")!, c));

    /// <summary>The merged dictionaries in file order: includes with a <c>Source</c> and inline dictionaries.</summary>
    public static IEnumerable<XamlNode> MergedDictionaries(XamlNode dictionary) =>
        dictionary.Children
            .Where(c => XamlNames.IsProperty(c, "ResourceDictionary", "MergedDictionaries"))
            .SelectMany(c => c.Children);

    /// <summary>Every resource definition of a document, with its stable id.</summary>
    public static IReadOnlyList<ResourceEntry> Definitions(XamlDocument document)
    {
        var entries = new List<ResourceEntry>();
        foreach (var host in Hosts(document))
        {
            AddEntries(entries, document, host.Dictionary, host.Dictionary.Path, null);
        }

        return entries;
    }

    private static IEnumerable<(XamlNode Host, XamlNode Dictionary)> Hosts(XamlDocument document)
    {
        foreach (var node in document.Nodes)
        {
            if (node.IsPropertyElement && node.PropertyName == "Resources")
            {
                yield return (node, DictionaryOf(node));
            }
            else if (node.Parent is null && XamlNames.IsType(node, "ResourceDictionary"))
            {
                yield return (node, node);
            }
        }
    }

    private static void AddEntries(List<ResourceEntry> entries, XamlDocument document, XamlNode dictionary, string scopePath, string? variant)
    {
        foreach (var entry in OwnEntries(dictionary))
        {
            var key = entry.AttributeValue("x:Key")!;
            entries.Add(new ResourceEntry(XamlIds.Resource(document.Path, scopePath, key, variant), key, document, entry, scopePath, variant));
        }

        foreach (var (name, themed) in ThemeDictionaries(dictionary))
        {
            AddEntries(entries, document, themed, scopePath, name);
        }
    }
}
