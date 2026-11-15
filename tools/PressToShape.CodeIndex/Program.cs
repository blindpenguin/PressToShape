using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PressToShape.CodeIndex.Cli;

namespace PressToShape.CodeIndex;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            return await new CliApp(Console.Out, Console.Error, Directory.GetCurrentDirectory(), CliServices.Default).RunAsync(args, cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            await Console.Error.WriteLineAsync("Cancelled.");
            return CliApp.Failure;
        }
    }
}
