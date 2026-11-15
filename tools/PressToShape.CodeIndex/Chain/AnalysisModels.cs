using System.Collections.Generic;
using PressToShape.CodeIndex.Model;

namespace PressToShape.CodeIndex.Chain;

/// <summary>A link between a view class and a view model class, with its evidence.</summary>
internal sealed record ViewModelLink(string ViewId, string ViewModelId, Confidence Confidence, string? Reason, string? File, int? Line);

/// <summary>A place where an object is created or a command is bound: a member and its source location.</summary>
internal sealed record Place(string FromId, string FromName, string Location);

/// <summary>A class that implements a service interface, and where it is created.</summary>
internal sealed record Implementation(string TypeId, string Name, IReadOnlyList<Place> CreatedAt);

/// <summary>A type that a view model takes in its constructors. For an interface the implementations are listed. A class is its own implementation.</summary>
internal sealed record ServiceDependency(
    string TypeId,
    string Name,
    bool IsInterface,
    string? Summary,
    IReadOnlyList<Implementation> Implementations,
    IReadOnlyList<Place> CreatedAt);

/// <summary>A type of a project without views that a view model references, with the number of references.</summary>
internal sealed record ModelUse(string TypeId, string Name, string Project, int References);

/// <summary>The method that runs when a command executes, or the reason why it is not statically known.</summary>
internal sealed record HandlerInfo(string? MethodId, string? MethodName, string? Location, Confidence? Confidence, string? Reason)
{
    public bool IsKnown => MethodId is not null;

    public static HandlerInfo Unknown(string reason) => new(null, null, null, null, reason);
}

/// <summary>A command property of a view model, its handler, and the XAML places that bind it.</summary>
internal sealed record CommandInfo(string PropertyId, string Name, HandlerInfo Handler, IReadOnlyList<Place> BoundAt);

/// <summary>A test class that references the analysed symbols.</summary>
internal sealed record TestUse(string ClassId, string Name, string File, int Line, int References);
