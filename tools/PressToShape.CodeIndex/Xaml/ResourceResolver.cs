using System;
using System.Collections.Generic;
using System.Linq;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Resolves <c>StaticResource</c> and <c>DynamicResource</c> keys in the order of Avalonia's lookup: the element and its ancestors, the application resources
/// (with merged dictionaries from the last to the first), then the resources of the files included in <c>Application.Styles</c>.
/// The nearest level wins. At one level, a key defined in several theme variants gives one hit per variant.
/// </summary>
internal sealed class ResourceResolver
{
    private readonly XamlWorld _world;

    public ResourceResolver(XamlWorld world)
    {
        ArgumentNullException.ThrowIfNull(world);
        _world = world;
    }

    public ResourceLookup Resolve(XamlDocument document, XamlNode node, string key)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(key);

        for (var current = node; current is not null; current = current.Parent)
        {
            var hits = LookupAt(document, current, key);
            if (hits.Count > 0)
            {
                return new ResourceLookup(hits, null);
            }
        }

        var application = _world.ApplicationFor(document);
        if (application is not null)
        {
            var hits = LookupApplication(application, key);
            if (hits.Count > 0)
            {
                return new ResourceLookup(hits, null);
            }
        }

        return ResourceLookup.NotFound();
    }

    private List<ResourceEntry> LookupAt(XamlDocument document, XamlNode node, string key)
    {
        var hits = new List<ResourceEntry>();
        if (!node.IsPropertyElement && node.Parent is null && XamlNames.IsType(node, "ResourceDictionary"))
        {
            hits.AddRange(LookupIn(document, node, node.Path, key, null, []));
        }

        foreach (var resources in node.Children.Where(c => c.IsPropertyElement && c.PropertyName == "Resources"))
        {
            var dictionary = ResourceDictionaries.DictionaryOf(resources);
            hits.AddRange(LookupIn(document, dictionary, dictionary.Path, key, null, []));
        }

        return hits;
    }

    private List<ResourceEntry> LookupApplication(XamlDocument application, string key)
    {
        foreach (var resources in application.Root.Children.Where(c => XamlNames.IsProperty(c, "Application", "Resources")))
        {
            var dictionary = ResourceDictionaries.DictionaryOf(resources);
            var hits = LookupIn(application, dictionary, dictionary.Path, key, null, []);
            if (hits.Count > 0)
            {
                return hits;
            }
        }

        foreach (var styles in application.Root.Children.Where(c => XamlNames.IsProperty(c, "Application", "Styles")))
        {
            foreach (var style in Enumerable.Reverse(styles.Children))
            {
                var hits = LookupInStyle(application, style, key);
                if (hits.Count > 0)
                {
                    return hits;
                }
            }
        }

        return [];
    }

    private List<ResourceEntry> LookupInStyle(XamlDocument owner, XamlNode style, string key)
    {
        if (style.AttributeValue("Source") is { } source && _world.ResolveInclude(owner, source) is { } target)
        {
            return ResourceDictionaries.DictionaryOfDocument(target) is { } dictionary
                ? LookupIn(target, dictionary, dictionary.Path, key, null, [target.Path])
                : [];
        }

        var inline = style.Children.FirstOrDefault(c => c.IsPropertyElement && c.PropertyName == "Resources");
        return inline is null ? [] : LookupIn(owner, ResourceDictionaries.DictionaryOf(inline), inline.Path, key, null, []);
    }

    private List<ResourceEntry> LookupIn(XamlDocument document, XamlNode dictionary, string scopePath, string key, string? variant, HashSet<string> visited)
    {
        var own = ResourceDictionaries.OwnEntries(dictionary).Where(e => e.AttributeValue("x:Key") == key).ToList();
        if (own.Count > 0)
        {
            return own.Select(e => new ResourceEntry(XamlIds.Resource(document.Path, scopePath, key, variant), key, document, e, scopePath, variant)).ToList();
        }

        var themed = ResourceDictionaries.ThemeDictionaries(dictionary)
            .SelectMany(t => LookupIn(document, t.Dictionary, scopePath, key, t.Variant, visited))
            .ToList();
        if (themed.Count > 0)
        {
            return themed;
        }

        foreach (var merged in Enumerable.Reverse(ResourceDictionaries.MergedDictionaries(dictionary).ToList()))
        {
            var hits = LookupMerged(document, merged, key, variant, visited);
            if (hits.Count > 0)
            {
                return hits;
            }
        }

        return [];
    }

    private List<ResourceEntry> LookupMerged(XamlDocument owner, XamlNode merged, string key, string? variant, HashSet<string> visited)
    {
        if (merged.AttributeValue("Source") is not { } source)
        {
            return XamlNames.IsType(merged, "ResourceDictionary") ? LookupIn(owner, merged, merged.Path, key, variant, visited) : [];
        }

        var target = _world.ResolveInclude(owner, source);
        if (target is null || !visited.Add(target.Path))
        {
            return [];
        }

        try
        {
            return ResourceDictionaries.DictionaryOfDocument(target) is { } dictionary
                ? LookupIn(target, dictionary, dictionary.Path, key, variant, visited)
                : [];
        }
        finally
        {
            visited.Remove(target.Path);
        }
    }
}
