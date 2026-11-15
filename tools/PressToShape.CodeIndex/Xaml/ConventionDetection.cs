namespace PressToShape.CodeIndex.Xaml;

/// <summary>Either the recognized convention, or the reason why none was recognized.</summary>
internal sealed record ConventionDetection(ViewLocatorConvention? Convention, string? Reason);
