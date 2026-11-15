namespace PressToShape.CodeIndex.Model;

/// <summary>A project that could not be loaded or compiled. Warnings do not change the exit status.</summary>
internal sealed record LoadProblem(string Project, string Reason, bool IsWarning);
