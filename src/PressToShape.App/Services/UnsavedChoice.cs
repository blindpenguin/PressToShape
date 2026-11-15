namespace PressToShape.App.Services;

/// <summary>The user's answer to the question what to do with unsaved changes.</summary>
internal enum UnsavedChoice
{
    /// <summary>Save the changes, then go on.</summary>
    Save,

    /// <summary>Throw the changes away and go on.</summary>
    DontSave,

    /// <summary>Do not go on.</summary>
    Cancel,
}
