using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>
/// The name convention of the view locator that the application registers in <c>Application.DataTemplates</c>. It is the only source of <c>inferred</c> view links.
/// The tool does not assume the convention: it reads the locator's <c>Build</c> method with Roslyn and accepts only the pattern
/// <c>Type.GetType(&lt;view model&gt;.GetType().FullName.Replace("A", "B"))</c>. <c>Type.GetType(string)</c> only finds types of the calling assembly,
/// so a view in another assembly is not a candidate.
/// </summary>
internal sealed class ViewLocatorConvention
{
    private const string DataTemplateInterface = "Avalonia.Controls.Templates.IDataTemplate";

    private readonly Compilation _compilation;
    private readonly IReadOnlySet<string> _knownIds;
    private readonly IAssemblySymbol _locatorAssembly;

    private ViewLocatorConvention(
        string find,
        string replace,
        string registrationFile,
        int registrationLine,
        string locatorId,
        Compilation compilation,
        IReadOnlySet<string> knownIds,
        IAssemblySymbol locatorAssembly)
    {
        Find = find;
        Replace = replace;
        RegistrationFile = registrationFile;
        RegistrationLine = registrationLine;
        LocatorId = locatorId;
        _compilation = compilation;
        _knownIds = knownIds;
        _locatorAssembly = locatorAssembly;
    }

    /// <summary>The text that is replaced in the view model's full type name.</summary>
    public string Find { get; }

    /// <summary>The text that replaces it.</summary>
    public string Replace { get; }

    public string RegistrationFile { get; }

    public int RegistrationLine { get; }

    /// <summary>The id of the locator class.</summary>
    public string LocatorId { get; }

    public static ConventionDetection Detect(XamlDocument application, XamlTypeResolver resolver, Compilation compilation, IReadOnlySet<string> knownIds)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentNullException.ThrowIfNull(knownIds);

        var registrations = application.Root.Children
            .Where(c => XamlNames.IsProperty(c, "Application", "DataTemplates"))
            .SelectMany(c => c.Children)
            .ToList();
        if (registrations.Count == 0)
        {
            return Failed("no data template is registered in Application.DataTemplates");
        }

        string? lastReason = null;
        foreach (var registration in registrations)
        {
            var type = resolver.Resolve(application, registration, registration.TypeText);
            if (type.Symbol is null || type.SolutionId is null || !Implements(type.Symbol, DataTemplateInterface))
            {
                lastReason = $"'{registration.TypeText}' in Application.DataTemplates is not a class of this solution that implements IDataTemplate";
                continue;
            }

            var detection = ReadBuild(type.Symbol, application, registration, compilation, knownIds);
            if (detection.Convention is not null)
            {
                return detection;
            }

            lastReason = detection.Reason;
        }

        return Failed(lastReason ?? "no usable data template was found");
    }

    /// <summary>The view class that the locator would create for a view model, or null (no such type, same name, or a type of another assembly).</summary>
    public INamedTypeSymbol? ViewFor(INamedTypeSymbol viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var name = FullName(viewModel);
        var candidateName = name.Replace(Find, Replace, StringComparison.Ordinal);
        if (candidateName == name)
        {
            return null;
        }

        var candidate = _compilation.GetTypeByMetadataName(candidateName);
        var id = candidate?.GetDocumentationCommentId();
        return candidate is not null && id is not null && _knownIds.Contains(id)
            && SymbolEqualityComparer.Default.Equals(candidate.ContainingAssembly, _locatorAssembly)
            ? candidate
            : null;
    }

    /// <summary>The text that explains an inferred link: the convention, the registration, and the evidence for the view model.</summary>
    public string Describe(INamedTypeSymbol viewModel, INamedTypeSymbol view, string evidence) =>
        $"ViewLocator (registered in Application.DataTemplates at {RegistrationFile}:{RegistrationLine}) picks views by name: "
        + $"'{Find}' is replaced by '{Replace}' in the full type name, so {FullName(viewModel)} maps to {FullName(view)}. Evidence: {evidence}";

    private static ConventionDetection ReadBuild(
        INamedTypeSymbol locator, XamlDocument application, XamlNode registration, Compilation compilation, IReadOnlySet<string> knownIds)
    {
        var build = locator.GetMembers("Build").OfType<IMethodSymbol>().FirstOrDefault(m => !m.IsStatic && m.Parameters.Length >= 1);
        if (build?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not MethodDeclarationSyntax syntax)
        {
            return Failed($"{locator.Name} has no readable Build method");
        }

        var model = compilation.GetSemanticModel(syntax.SyntaxTree);
        if (model.GetOperation(syntax) is not { } body)
        {
            return Failed($"the Build method of {locator.Name} could not be analysed");
        }

        var operations = body.DescendantsAndSelf().ToList();
        var invocations = operations.OfType<IInvocationOperation>().ToList();
        var replace = invocations.FirstOrDefault(IsFullNameReplace);
        if (replace is null)
        {
            return Failed($"the Build method of {locator.Name} does not use <type>.FullName.Replace(\"..\", \"..\") with constant text");
        }

        var getType = invocations.FirstOrDefault(i => IsTypeGetType(i) && FlowsFrom(i.Arguments[0].Value, replace, operations));
        if (getType is null)
        {
            return Failed($"the Build method of {locator.Name} does not pass the replaced name to Type.GetType");
        }

        var id = locator.GetDocumentationCommentId();
        if (id is null)
        {
            return Failed($"{locator.Name} has no id");
        }

        var find = (string)replace.Arguments[0].Value.ConstantValue.Value!;
        var replacement = (string)replace.Arguments[1].Value.ConstantValue.Value!;
        return new ConventionDetection(
            new ViewLocatorConvention(find, replacement, application.Path, registration.Line, id, compilation, knownIds, locator.ContainingAssembly), null);
    }

    private static bool IsFullNameReplace(IInvocationOperation invocation) =>
        invocation.TargetMethod is { Name: "Replace", ContainingType.SpecialType: SpecialType.System_String }
        && invocation.Arguments.Length >= 2
        && IsConstantString(invocation.Arguments[0].Value)
        && IsConstantString(invocation.Arguments[1].Value)
        && Unwrap(invocation.Instance) is IPropertyReferenceOperation { Property: { Name: "FullName", ContainingType: { Name: "Type" } type } }
        && type.ContainingNamespace.ToDisplayString() == "System";

    private static bool IsTypeGetType(IInvocationOperation invocation) =>
        invocation.TargetMethod is { Name: "GetType", IsStatic: true, ContainingType: { Name: "Type" } type }
        && type.ContainingNamespace.ToDisplayString() == "System"
        && invocation.Arguments.Length >= 1;

    private static bool IsConstantString(IOperation? operation) => operation?.ConstantValue is { HasValue: true, Value: string };

    private static bool FlowsFrom(IOperation? argument, IInvocationOperation replace, List<IOperation> operations)
    {
        var value = Unwrap(argument);
        if (ReferenceEquals(value, replace))
        {
            return true;
        }

        if (value is not ILocalReferenceOperation local)
        {
            return false;
        }

        return operations.OfType<IVariableDeclaratorOperation>().Any(d => SymbolEqualityComparer.Default.Equals(d.Symbol, local.Local) && ReferenceEquals(Unwrap(d.Initializer?.Value), replace))
            || operations.OfType<ISimpleAssignmentOperation>().Any(a => Unwrap(a.Target) is ILocalReferenceOperation target
                && SymbolEqualityComparer.Default.Equals(target.Local, local.Local) && ReferenceEquals(Unwrap(a.Value), replace));
    }

    private static IOperation? Unwrap(IOperation? operation)
    {
        while (operation is IConversionOperation conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }

    private static bool Implements(INamedTypeSymbol type, string interfaceName) =>
        type.AllInterfaces.Any(i => i.ToDisplayString() == interfaceName);

    /// <summary>The runtime full name: namespace, containing types separated by <c>+</c>, and the metadata name.</summary>
    private static string FullName(INamedTypeSymbol type)
    {
        if (type.ContainingType is { } outer)
        {
            return FullName(outer) + "+" + type.MetadataName;
        }

        var ns = type.ContainingNamespace is { IsGlobalNamespace: false } containing ? containing.ToDisplayString() : string.Empty;
        return ns.Length == 0 ? type.MetadataName : $"{ns}.{type.MetadataName}";
    }

    private static ConventionDetection Failed(string reason) => new(null, reason);
}
