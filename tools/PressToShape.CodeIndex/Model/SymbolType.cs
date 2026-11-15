using System.Text.Json.Serialization;

namespace PressToShape.CodeIndex.Model;

/// <summary>The type of a symbol in the index. Written to JSON by name.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SymbolType>))]
internal enum SymbolType
{
    Namespace,
    Class,
    Interface,
    Record,
    Struct,
    Enum,
    Delegate,
    Method,
    Constructor,
    Property,
    Field,
    Event,
}
