using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml;

namespace PressToShape.CodeIndex.Chain;

/// <summary>
/// The questions that the <c>chain</c> command and the Markdown pages ask of the index: which view models a view uses, which services, models, commands and tests belong to them.
/// It works on <see cref="IIndexView"/>, so both use the same rules. Nothing is guessed: missing facts are results ("none", "not statically known").
/// </summary>
internal sealed class IndexAnalysis
{
    private readonly IIndexView _view;
    private readonly TestClassFinder _tests;
    private readonly HashSet<string> _modelProjects;

    public IndexAnalysis(IIndexView view)
    {
        ArgumentNullException.ThrowIfNull(view);
        _view = view;
        _tests = new TestClassFinder(view);
        _modelProjects = view.Projects.Where(p => p.XamlFileCount == 0).Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
    }

    public IIndexView View => _view;

    /// <summary>A short name for any id: the symbol name, or the readable text of a XAML id.</summary>
    public string NameOf(string id) => _view.SymbolOf(id)?.Name ?? XamlIds.Describe(id);

    public IReadOnlyList<ViewModelLink> ViewModelsOf(string viewClassId) =>
        Links(e => e.FromId == viewClassId).OrderBy(l => l.ViewModelId, StringComparer.Ordinal).ThenBy(l => l.Confidence).ToList();

    public IReadOnlyList<ViewModelLink> ViewsOf(string viewModelId) =>
        Links(e => e.ToId == viewModelId).OrderBy(l => l.ViewId, StringComparer.Ordinal).ThenBy(l => l.Confidence).ToList();

    /// <summary>The constructor dependencies of a type, with implementations and the places where they are created.</summary>
    public IReadOnlyList<ServiceDependency> Services(string typeId) =>
        _view.Relationships
            .Where(e => e.Type == RelationshipType.InjectedDependency && e.FromId == typeId)
            .Select(e => e.ToId)
            .Distinct(StringComparer.Ordinal)
            .Select(ToDependency)
            .OrderBy(d => d.Name, StringComparer.Ordinal)
            .ThenBy(d => d.TypeId, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Types of projects without views that the type or its members reference (A1), without the services that it takes. Member ids are mapped to their type.
    /// </summary>
    public IReadOnlyList<ModelUse> Models(string typeId)
    {
        var owner = _view.SymbolOf(typeId);
        var own = new HashSet<string>(_view.MembersOf(typeId).Select(m => m.Id), StringComparer.Ordinal) { typeId };
        var services = Services(typeId).Select(s => s.TypeId).ToHashSet(StringComparer.Ordinal);
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var edge in _view.Relationships.Where(e => own.Contains(e.FromId) && e.Type is RelationshipType.References or RelationshipType.Calls or RelationshipType.InjectedDependency))
        {
            var target = TypeOf(edge.ToId);
            if (target is not null && !own.Contains(target.Id) && !services.Contains(target.Id) && _modelProjects.Contains(target.Project) && target.Project != owner?.Project)
            {
                counts[target.Id] = counts.GetValueOrDefault(target.Id) + 1;
            }
        }

        return counts
            .Select(c => (Symbol: _view.SymbolOf(c.Key)!, Count: c.Value))
            .Select(x => new ModelUse(x.Symbol.Id, x.Symbol.Name, x.Symbol.Project, x.Count))
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ThenBy(m => m.TypeId, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>The command properties of a view model: generated or assigned commands, and properties that XAML binds as commands.</summary>
    public IReadOnlyList<CommandInfo> Commands(string viewModelId)
    {
        var bound = BoundCommandIds();
        return _view.MembersOf(viewModelId)
            .Where(m => m.SymbolType == SymbolType.Property && (bound.Contains(m.Id) || HasHandlerEdge(m.Id) || IsGeneratedFromMethod(m)))
            .Select(m => CommandOf(m.Id))
            .OfType<CommandInfo>()
            .OrderBy(c => c.Name, StringComparer.Ordinal)
            .ToList();
    }

    public CommandInfo? CommandOf(string propertyId)
    {
        var property = _view.SymbolOf(propertyId);
        return property is null ? null : new CommandInfo(propertyId, property.Name, HandlerOf(propertyId), BoundAt(propertyId));
    }

    /// <summary>True when the id is a property that is a command: bound as a command in XAML, generated from a method, or assigned from a method.</summary>
    public bool IsCommand(string id) => _view.SymbolOf(id) is { SymbolType: SymbolType.Property } property
        && (BoundCommandIds().Contains(id) || HasHandlerEdge(id) || IsGeneratedFromMethod(property));

    public HandlerInfo HandlerOf(string commandMemberId)
    {
        var edge = _view.Relationships.FirstOrDefault(e => e.Type == RelationshipType.ExecutesMethod && e.FromId == commandMemberId);
        if (edge is not null)
        {
            return new HandlerInfo(edge.ToId, NameOf(edge.ToId), Location(edge), edge.Confidence, edge.Reason);
        }

        if (_view.SymbolOf(commandMemberId) is { GeneratedFrom: { } from } && _view.SymbolOf(from) is { SymbolType: SymbolType.Method } method)
        {
            return new HandlerInfo(method.Id, method.Name, $"{method.File}:{method.StartLine}", Confidence.Verified, "[RelayCommand]");
        }

        return HandlerInfo.Unknown("not statically known: the index follows [RelayCommand] methods and commands created from a method group, not lambdas, parameters or locals");
    }

    /// <summary>Test classes that reference the ids (or members of the types among them), grouped per test class.</summary>
    public IReadOnlyList<TestUse> TestsFor(IEnumerable<string> ids)
    {
        var targets = new HashSet<string>(StringComparer.Ordinal);
        foreach (var id in ids)
        {
            targets.Add(id);
            if (id.StartsWith("T:", StringComparison.Ordinal))
            {
                _view.MembersOf(id).ToList().ForEach(m => targets.Add(m.Id));
            }
        }

        var uses = new Dictionary<string, (SymbolRecord Class, int Count, string File, int Line)>(StringComparer.Ordinal);
        foreach (var edge in _view.Relationships.Where(e => targets.Contains(e.ToId) && e.Type != RelationshipType.Inherits && e.Type != RelationshipType.Implements))
        {
            if (_tests.TestClassOf(edge.FromId) is not { } testClass)
            {
                continue;
            }

            var (file, line) = (edge.File ?? testClass.File, edge.Line ?? testClass.StartLine);
            uses[testClass.Id] = uses.TryGetValue(testClass.Id, out var existing)
                ? (existing.Class, existing.Count + 1, existing.File, existing.Line)
                : (testClass, 1, file, line);
        }

        return uses.Values
            .Select(u => new TestUse(u.Class.Id, u.Class.Name, u.File, u.Line, u.Count))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ThenBy(t => t.ClassId, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>The places where a type is created (calls of its constructors, and creations of a type without an explicit constructor), without creations in test projects.</summary>
    public IReadOnlyList<Place> CreationSites(string typeId)
    {
        var targets = _view.MembersOf(typeId).Where(m => m.SymbolType == SymbolType.Constructor).Select(m => m.Id).ToHashSet(StringComparer.Ordinal);
        targets.Add(typeId);
        return _view.Relationships
            .Where(e => e.Type == RelationshipType.Calls && targets.Contains(e.ToId) && !_tests.IsTestSide(e.FromId))
            .Select(e => new Place(e.FromId, NameWithOwner(e.FromId), Location(e)))
            .Distinct()
            .OrderBy(p => p.Location, StringComparer.Ordinal)
            .ThenBy(p => p.FromId, StringComparer.Ordinal)
            .ToList();
    }

    private IEnumerable<ViewModelLink> Links(Func<RelationshipRecord, bool> filter) =>
        _view.Relationships
            .Where(e => e.Type == RelationshipType.UsesViewModel && filter(e))
            .Select(e => new ViewModelLink(e.FromId, e.ToId, e.Confidence, e.Reason, e.File, e.Line))
            .Distinct();

    /// <summary>True for symbols of test classes and of projects that have them. The pages leave these out, because they describe the product.</summary>
    public bool IsTestSide(string id) => _tests.IsTestSide(id);

    /// <summary>A type as a service: its implementations when it is an interface, and where they or it are created.</summary>
    public ServiceDependency ServiceOf(string typeId) => ToDependency(typeId);

    private ServiceDependency ToDependency(string typeId)
    {
        var symbol = _view.SymbolOf(typeId);
        var isInterface = symbol?.SymbolType == SymbolType.Interface;
        var implementations = isInterface
            ? _view.Relationships
                .Where(e => e.Type == RelationshipType.Implements && e.ToId == typeId)
                .Select(e => e.FromId)
                .Distinct(StringComparer.Ordinal)
                .Where(id => !_tests.IsTestSide(id))
                .Select(id => new Implementation(id, NameOf(id), CreationSites(id)))
                .OrderBy(i => i.Name, StringComparer.Ordinal)
                .ToList()
            : [];
        return new ServiceDependency(typeId, NameOf(typeId), isInterface, symbol?.Doc.Summary, implementations, isInterface ? [] : CreationSites(typeId));
    }

    private HashSet<string> BoundCommandIds() =>
        _view.XamlFiles.SelectMany(f => f.Bindings).Where(b => b.IsCommand && b.TargetId is not null).Select(b => b.TargetId!).ToHashSet(StringComparer.Ordinal);

    private IReadOnlyList<Place> BoundAt(string propertyId) =>
        _view.XamlFiles
            .SelectMany(f => f.Bindings.Where(b => b.TargetId == propertyId).Select(b => new Place(b.Id, XamlIds.Describe(b.Id), $"{f.File}:{b.Line}")))
            .OrderBy(p => p.Location, StringComparer.Ordinal)
            .ThenBy(p => p.FromId, StringComparer.Ordinal)
            .ToList();

    private bool HasHandlerEdge(string id) => _view.Relationships.Any(e => e.Type == RelationshipType.ExecutesMethod && e.FromId == id);

    private bool IsGeneratedFromMethod(SymbolRecord property) =>
        property.GeneratedFrom is { } from && _view.SymbolOf(from) is { SymbolType: SymbolType.Method };

    private SymbolRecord? TypeOf(string id)
    {
        var symbol = _view.SymbolOf(id);
        for (var depth = 0; symbol is not null && depth < 8; depth++)
        {
            if (symbol.SymbolType is SymbolType.Class or SymbolType.Record or SymbolType.Struct or SymbolType.Enum or SymbolType.Interface)
            {
                return symbol;
            }

            symbol = symbol.ContainingId is { } containing ? _view.SymbolOf(containing) : null;
        }

        return null;
    }

    private string NameWithOwner(string memberId)
    {
        var member = _view.SymbolOf(memberId);
        return member is null ? memberId : member.ContainingId is { } owner && _view.SymbolOf(owner) is { } type ? $"{type.Name}.{member.Name}" : member.Name;
    }

    private static string Location(RelationshipRecord edge) => edge.File is null ? string.Empty : edge.Line is { } line ? $"{edge.File}:{line}" : edge.File;
}
