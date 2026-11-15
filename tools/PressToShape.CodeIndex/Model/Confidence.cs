using System.Text.Json.Serialization;

namespace PressToShape.CodeIndex.Model;

/// <summary>How sure the index is about a relationship.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<Confidence>))]
internal enum Confidence
{
    /// <summary>Resolved statically by the compiler.</summary>
    Verified,

    /// <summary>Not proven statically. The relationship carries the reason.</summary>
    Inferred,
}
