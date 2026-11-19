using System.Collections.Generic;

namespace PressToShape.App.Services;

/// <summary>The font families the user can choose for a text. A small interface of ours, so the Properties view model needs no fonts in tests.</summary>
internal interface IFontCatalog
{
    /// <summary>The names of the font families, the default font first, then the installed fonts in alphabetical order. No name twice.</summary>
    IReadOnlyList<string> Families { get; }
}
