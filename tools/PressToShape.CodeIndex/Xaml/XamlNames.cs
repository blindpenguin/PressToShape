using System;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>Small helpers for names as written in XAML.</summary>
internal static class XamlNames
{
    /// <summary>The name without its prefix: <c>desktop:MenuBarView</c> gives <c>MenuBarView</c>.</summary>
    public static string Local(string typeText)
    {
        var colon = typeText.IndexOf(':', StringComparison.Ordinal);
        return colon < 0 ? typeText : typeText[(colon + 1)..];
    }

    public static bool IsType(XamlNode node, string localName) => !node.IsPropertyElement && Local(node.TypeText) == localName;

    public static bool IsProperty(XamlNode node, string ownerLocalName, string property) =>
        node.IsPropertyElement && node.PropertyName == property && Local(node.TypeText) == ownerLocalName;
}
