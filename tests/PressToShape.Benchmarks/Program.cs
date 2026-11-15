using System;
using BenchmarkDotNet.Running;

namespace PressToShape.Benchmarks;

internal static class Program
{
    // The benchmarks run in child processes that inherit this limit, so a runaway benchmark fails with an out-of-memory error
    // instead of using up the memory of the computer. The value is hexadecimal bytes (0x60000000 is 1.5 GB). Set the variable
    // yourself before the run to use another limit.
    private const string HeapLimitVariable = "DOTNET_GCHeapHardLimit";
    private const string DefaultHeapLimit = "0x60000000";

    // Run in Release: dotnet run -c Release --project tests/PressToShape.Benchmarks -- --filter "*"
    // A quick pass: add --job short. See README.md.
    private static void Main(string[] args)
    {
        if (Environment.GetEnvironmentVariable(HeapLimitVariable) is null)
        {
            Environment.SetEnvironmentVariable(HeapLimitVariable, DefaultHeapLimit);
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
