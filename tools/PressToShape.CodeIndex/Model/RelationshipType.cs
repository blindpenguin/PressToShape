using System.Text.Json.Serialization;

namespace PressToShape.CodeIndex.Model;

/// <summary>The type of a relationship between two symbols.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<RelationshipType>))]
internal enum RelationshipType
{
    Inherits,
    Implements,
    InjectedDependency,
    References,
    Calls,
    Registers,
}
