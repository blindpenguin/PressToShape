using System.Collections.Generic;

namespace PressToShape.CodeIndex.Model;

/// <summary>An attribute applied to a symbol, with its arguments as written in source.</summary>
internal sealed record AttributeRecord(string Name, IReadOnlyList<string> Arguments);
