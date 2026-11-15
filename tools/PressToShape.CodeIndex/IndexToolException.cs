using System;

namespace PressToShape.CodeIndex;

/// <summary>A failure the user can fix. The message says what to do, and the command line prints it without a stack trace.</summary>
internal sealed class IndexToolException : Exception
{
    public IndexToolException(string message)
        : base(message)
    {
    }

    public IndexToolException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
