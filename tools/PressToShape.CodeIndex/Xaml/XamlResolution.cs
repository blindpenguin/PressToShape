using System.Collections.Generic;
using PressToShape.CodeIndex.Model;
using PressToShape.CodeIndex.Xaml.Model;

namespace PressToShape.CodeIndex.Xaml;

/// <summary>The result for one XAML file: its record and the relationships that start at it.</summary>
internal sealed record XamlResolution(XamlFileRecord File, IReadOnlyList<RelationshipRecord> Relationships);
