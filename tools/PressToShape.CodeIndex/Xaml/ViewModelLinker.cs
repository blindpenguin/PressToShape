using System.Linq;
using Microsoft.CodeAnalysis;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// Creates the View to ViewModel links of a XAML file. Every link says where it comes from. Verified links come from <c>x:DataType</c>,
/// a <c>DataContext</c> binding or a data template. Design-time data contexts and the view locator's name convention are only <c>inferred</c>.
/// No link is made from matching names alone.
/// </summary>
internal static class ViewModelLinker
{
    private const string ContentControl = "Avalonia.Controls.ContentControl";
    private const string ContentPresenter = "Avalonia.Controls.Presenters.ContentPresenter";
    private const string ItemsControl = "Avalonia.Controls.ItemsControl";

    public static void Link(XamlContext context)
    {
        LinkRootDataType(context);
        LinkDesignTime(context);
        LinkDataTemplates(context);
        LinkDataContextBindings(context);
        LinkViewLocator(context);
    }

    private static void LinkRootDataType(XamlContext context)
    {
        var root = context.Document.Root;
        if (context.ClassId is null || XamlContext.DataTypeText(root) is not { } text)
        {
            return;
        }

        if (context.Types.Resolve(context.Document, root, text).SolutionId is { } viewModelId)
        {
            context.AddEdge(RelationshipType.UsesViewModel, context.ClassId, viewModelId, root, Confidence.Verified, "x:DataType");
        }
    }

    private static void LinkDesignTime(XamlContext context)
    {
        if (context.ClassId is null)
        {
            return;
        }

        var root = context.Document.Root;
        foreach (var property in root.Children.Where(c => XamlNames.IsProperty(c, "Design", "DataContext")))
        {
            foreach (var child in property.Children)
            {
                AddDesignTime(context, context.Types.Resolve(context.Document, child, child.TypeText).SolutionId, property);
            }
        }

        if (root.Attribute("d:DataContext")?.Parsed is { } instance && DesignInstanceType(instance) is { } typeText
            && context.Types.Resolve(context.Document, root, typeText).SolutionId is { } id)
        {
            AddDesignTime(context, id, root);
        }
    }

    private static string? DesignInstanceType(MarkupExtension extension) =>
        extension.Name == "d:DesignInstance" ? extension.NamedText("Type") ?? extension.FirstPositionalText : null;

    private static void AddDesignTime(XamlContext context, string? viewModelId, XamlNode at)
    {
        if (viewModelId is not null && context.ClassId is not null)
        {
            context.AddEdge(RelationshipType.UsesViewModel, context.ClassId, viewModelId, at, Confidence.Inferred, "design-time only (Design.DataContext), not a runtime link");
        }
    }

    private static void LinkDataTemplates(XamlContext context)
    {
        foreach (var node in context.Document.Nodes.Where(XamlContext.IsDataTemplate))
        {
            if (XamlContext.DataTypeText(node) is { } text && context.Types.Resolve(context.Document, node, text).SolutionId is { } id)
            {
                context.AddEdge(RelationshipType.UsesType, context.ElementId(node), id, node, Confidence.Verified, "DataTemplate DataType");
            }
        }
    }

    /// <summary>R4.7: <c>&lt;views:Child DataContext="{Binding Prop}" /&gt;</c> where the binding resolves to a class of the solution.</summary>
    private static void LinkDataContextBindings(XamlContext context)
    {
        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement))
        {
            if (node.Attribute("DataContext") is not { Parsed: { } binding } || !IsBinding(binding))
            {
                continue;
            }

            var view = context.Types.Resolve(context.Document, node, node.TypeText).Symbol;
            if (view is null || !context.IsViewClass(view) || BindingTarget(context, node, binding) is not { } target)
            {
                continue;
            }

            var viewId = view.OriginalDefinition.GetDocumentationCommentId();
            if (viewId is not null && target.TargetTypeId is { } targetId)
            {
                context.AddEdge(RelationshipType.UsesViewModel, viewId, targetId, node, Confidence.Verified, $"DataContext binding at {context.Document.Path}:{node.Line}");
            }
        }
    }

    private static void LinkViewLocator(XamlContext context)
    {
        if (context.Convention is not { } convention)
        {
            return;
        }

        foreach (var node in context.Document.Nodes.Where(n => !n.IsPropertyElement))
        {
            var host = context.Types.Resolve(context.Document, node, node.TypeText).Symbol;
            if (host is null)
            {
                continue;
            }

            var isContent = XamlContext.Derives(host, ContentControl) || XamlContext.Derives(host, ContentPresenter);
            var isItems = XamlContext.Derives(host, ItemsControl);
            if ((!isContent && !isItems) || HasItemOrContentTemplate(node, isContent))
            {
                continue;
            }

            var propertyName = isContent ? "Content" : "ItemsSource";
            if (node.Attribute(propertyName) is not { Parsed: { } binding } || !IsBinding(binding)
                || BindingTarget(context, node, binding) is not { TargetType: { } targetType })
            {
                continue;
            }

            var viewModel = isContent ? targetType as INamedTypeSymbol : ElementTypeOf(targetType);
            if (viewModel is null || convention.ViewFor(viewModel) is not { } view)
            {
                continue;
            }

            var viewId = view.GetDocumentationCommentId();
            var viewModelId = viewModel.OriginalDefinition.GetDocumentationCommentId();
            if (viewId is not null && viewModelId is not null && context.KnownIds.Contains(viewModelId))
            {
                var evidence = $"{XamlNames.Local(node.TypeText)} {propertyName} binding at {context.Document.Path}:{node.Line}";
                context.AddEdge(RelationshipType.UsesViewModel, viewId, viewModelId, node, Confidence.Inferred, convention.Describe(viewModel, view, evidence));
            }
        }
    }

    private static BindingResolution? BindingTarget(XamlContext context, XamlNode node, MarkupExtension binding)
    {
        var (inScope, symbol, _) = context.DataTypeAt(node);
        if (!inScope || symbol is null)
        {
            return null;
        }

        var path = binding.NamedText("Path") ?? binding.FirstPositionalText;
        var resolution = context.Bindings.Resolve(symbol, path);
        return resolution.Status == BindingStatus.Verified && resolution.PropertyId is not null ? resolution : null;
    }

    private static bool IsBinding(MarkupExtension extension) => extension.Name is "Binding" or "CompiledBinding" or "ReflectionBinding";

    private static bool HasItemOrContentTemplate(XamlNode node, bool isContent)
    {
        var templateName = isContent ? "ContentTemplate" : "ItemTemplate";
        return node.Attribute(templateName) is not null
            || node.Children.Any(c => c.IsPropertyElement && c.PropertyName == templateName);
    }

    /// <summary>The element type of a collection type: the type argument of <c>IEnumerable&lt;T&gt;</c>.</summary>
    private static INamedTypeSymbol? ElementTypeOf(ITypeSymbol collection)
    {
        var enumerable = (collection as INamedTypeSymbol is { OriginalDefinition.SpecialType: SpecialType.System_Collections_Generic_IEnumerable_T } self ? self : null)
            ?? collection.AllInterfaces.FirstOrDefault(i => i.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);
        return enumerable?.TypeArguments[0] as INamedTypeSymbol;
    }
}
