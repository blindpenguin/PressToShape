using System.Collections.Generic;
using System.Linq;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>One element of a XAML file. A property element such as <c>UserControl.Resources</c> is a node with <see cref="PropertyName"/> set.</summary>
internal sealed class XamlNode
{
    public XamlNode(int index, XamlNode? parent, string typeText, string? propertyName, string? name, int line, string path)
    {
        Index = index;
        Parent = parent;
        TypeText = typeText;
        PropertyName = propertyName;
        Name = name;
        Line = line;
        Path = path;
    }

    /// <summary>Position in document order, starting at 0 for the root.</summary>
    public int Index { get; }

    public XamlNode? Parent { get; }

    /// <summary>The element name as written, with its prefix, for example <c>desktop:MenuBarView</c>. For a property element this is the owner type (<c>UserControl</c>).</summary>
    public string TypeText { get; }

    /// <summary>The property of a property element (<c>Resources</c>), otherwise null.</summary>
    public string? PropertyName { get; }

    public bool IsPropertyElement => PropertyName is not null;

    /// <summary><c>x:Name</c> or <c>Name</c>.</summary>
    public string? Name { get; }

    public int Line { get; }

    /// <summary>The element path, for example <c>/UserControl[1]/DockPanel[1]/Button[2]</c>. The root has no index.</summary>
    public string Path { get; }

    public List<XamlAttribute> Attributes { get; } = [];

    public List<XamlNode> Children { get; } = [];

    /// <summary>The <c>xmlns</c> declarations on this element only. The default namespace has the empty prefix.</summary>
    public Dictionary<string, string> Namespaces { get; } = new(System.StringComparer.Ordinal);

    public XamlAttribute? Attribute(string name) => Attributes.FirstOrDefault(a => a.Name == name);

    public string? AttributeValue(string name) => Attribute(name)?.Value;

    public IEnumerable<XamlNode> DescendantsAndSelf()
    {
        yield return this;
        foreach (var child in Children.SelectMany(c => c.DescendantsAndSelf()))
        {
            yield return child;
        }
    }
}
