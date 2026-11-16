namespace PressToShape.App.Workspace;

/// <summary>Which slots the ring menu shows, decided by what the pointer opened it on.</summary>
internal enum RingMenuMode
{
    /// <summary>Something is selected: the actions for the selection.</summary>
    Selection,

    /// <summary>Nothing is selected: the actions for the empty canvas.</summary>
    EmptyCanvas,
}
