namespace PressToShape.CodeIndex.Model;

/// <summary>A directed relationship between two symbols, referring to them only by id.</summary>
internal sealed record RelationshipRecord(
    RelationshipType Type,
    string FromId,
    string ToId,
    string? File,
    int? Line,
    Confidence Confidence,
    string? Reason);
