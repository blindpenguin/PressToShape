namespace PressToShape.Core.IO;

/// <summary>Why a project file could not be read. The app turns each value into a translated message.</summary>
public enum ProjectFileError
{
    /// <summary>The content is not a PressToShape project file: not JSON, wrong format name or a missing entry.</summary>
    NotProjectFile,

    /// <summary>The file was written by a newer version of the format than this reader knows.</summary>
    NewerVersion,

    /// <summary>The content is larger than <see cref="ProjectFile.MaxBytes"/>.</summary>
    TooLarge,

    /// <summary>A shape has a type this version does not know.</summary>
    UnknownShapeType,

    /// <summary>A value is not valid for the document model, for example a negative width or a duplicate id.</summary>
    InvalidValue,
}
