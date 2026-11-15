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

    // XAML relationships. Appended, so the order of the earlier values never changes.
    UsesViewModel,
    CodeBehind,
    BindsTo,
    ReferencesResource,
    UsesConverter,
    HandledBy,
    UsesType,
    Includes,
}
