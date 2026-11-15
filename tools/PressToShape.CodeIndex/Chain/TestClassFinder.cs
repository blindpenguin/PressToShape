using System;
using System.Collections.Generic;
using System.Linq;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>Decides whether a class is a test class: it has a method with a test attribute (<c>Fact</c>, <c>Theory</c>, <c>Test</c> or <c>TestMethod</c>, matched by name).</summary>
internal sealed class TestClassFinder
{
    private static readonly HashSet<string> TestAttributes = new(StringComparer.Ordinal) { "Fact", "Theory", "Test", "TestMethod" };

    private readonly IIndexView _view;
    private readonly Dictionary<string, bool> _cache = new(StringComparer.Ordinal);
    private HashSet<string>? _testProjects;

    public TestClassFinder(IIndexView view) => _view = view;

    /// <summary>The test class that contains the symbol, or null when the symbol is not part of a test class.</summary>
    public SymbolRecord? TestClassOf(string symbolId)
    {
        var symbol = _view.SymbolOf(symbolId);
        for (var depth = 0; symbol is not null && depth < 8; depth++)
        {
            if (IsType(symbol))
            {
                return IsTestClass(symbol) ? symbol : null;
            }

            symbol = symbol.ContainingId is { } containing ? _view.SymbolOf(containing) : null;
        }

        return null;
    }

    /// <summary>
    /// True for a symbol of a test class, and for any symbol of a project that has test classes (fakes and helpers there are not part of the product).
    /// A project counts as a test project when a test class of it has at least one outgoing relationship.
    /// </summary>
    public bool IsTestSide(string symbolId)
    {
        if (TestClassOf(symbolId) is not null)
        {
            return true;
        }

        _testProjects ??= _view.Relationships
            .Select(e => e.FromId)
            .Distinct(StringComparer.Ordinal)
            .Select(TestClassOf)
            .OfType<SymbolRecord>()
            .Select(c => c.Project)
            .ToHashSet(StringComparer.Ordinal);
        return _view.SymbolOf(symbolId) is { } symbol && _testProjects.Contains(symbol.Project);
    }

    private bool IsTestClass(SymbolRecord type)
    {
        if (!_cache.TryGetValue(type.Id, out var result))
        {
            result = _view.MembersOf(type.Id).Any(m => m.SymbolType == SymbolType.Method && m.Attributes.Any(a => TestAttributes.Contains(a.Name)));
            _cache[type.Id] = result;
        }

        return result;
    }

    private static bool IsType(SymbolRecord s) =>
        s.SymbolType is SymbolType.Class or SymbolType.Record or SymbolType.Struct or SymbolType.Interface;
}
