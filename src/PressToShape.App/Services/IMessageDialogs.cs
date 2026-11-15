using System.Threading.Tasks;

namespace PressToShape.App.Services;

/// <summary>The questions and messages shown to the user. The texts are translated by the caller.</summary>
internal interface IMessageDialogs
{
    /// <summary>Asks whether unsaved changes of the named document should be saved. Closing the question counts as Cancel.</summary>
    Task<UnsavedChoice> AskUnsavedChangesAsync(string documentName);

    /// <summary>Shows an error message and returns when the user has dismissed it.</summary>
    Task ShowErrorAsync(string message);
}
