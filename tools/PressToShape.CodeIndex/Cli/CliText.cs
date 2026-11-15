namespace PressToShape.CodeIndex.Cli;

/// <summary>The help text.</summary>
internal static class CliText
{
    public const string Usage = """
        PressToShape.CodeIndex - a searchable index of the C# source for coding agents

        Usage: PressToShape.CodeIndex <command> [argument] [options]

        Commands:
          generate            Analyse the solution and write the index to docs/index
          check               Compare a fresh analysis with the committed index. Exit code 1 if they differ
          find <term>         Search symbols by name (exact, then prefix, then substring)
          show <name|id>      Signature, documentation and relationships of one symbol
          impls <name|id>     Implementations of an interface, or of an interface member
          derived <name|id>   Types that derive directly from a type
          refs <name|id>      Where a symbol is referenced or called
          deps <name|id>      Types a type takes in its constructors

        Options:
          --json              Print JSON instead of text (query commands)
          --solution <path>   The solution to analyse (default: PressToShape.sln above the current folder)
          --help              Show this text

        Symbol types in results: Class, Interface, Record, Struct, Enum, Delegate, Method, Property, Field, Event, and for XAML View, Element, Resource, Style and Template.
        A view class also shows its XAML file. Bindings, resources and handlers show whether they are verified, inferred or unresolved.

        Exit codes: 0 success, 1 failure or differences found, 2 usage error.
        Results marked "inferred" are not proven by the compiler. Each one carries a reason.
        Run `dotnet restore` before `generate` or `check`.
        """;
}
