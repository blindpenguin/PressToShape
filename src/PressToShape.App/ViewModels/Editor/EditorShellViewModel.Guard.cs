using System;
using System.Threading.Tasks;
using PressToShape.App.Services;

namespace PressToShape.App.ViewModels.Editor;

internal abstract partial class EditorShellViewModel
{
    private readonly IMessageDialogs? _messages;

    /// <summary>
    /// Like <see cref="ConfirmCloseAsync"/>, but a failure does not escape: it is reported to the user and closing is not allowed.
    /// The view calls this from an <c>async void</c> handler, where an exception would end the app.
    /// </summary>
    public Task<bool> TryConfirmCloseAsync() => GuardAsync(ConfirmCloseAsync);

    /// <summary>Runs a flow that may close or leave the app. If it fails, the user gets an error message and the answer is "stay".</summary>
    /// <param name="flow">The flow. It returns whether the app was told to end.</param>
    protected async Task<bool> GuardAsync(Func<Task<bool>> flow)
    {
        ArgumentNullException.ThrowIfNull(flow);

        try
        {
            return await flow();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await ReportFailureAsync();
            return false;
        }
    }

    private async Task ReportFailureAsync()
    {
        try
        {
            if (_messages is not null)
            {
                await _messages.ShowErrorAsync(_localizer.T("The operation could not be done."));
            }
        }
        catch (Exception)
        {
            // The message itself failed. Nothing more can be shown, and the app stays open.
        }
    }
}
